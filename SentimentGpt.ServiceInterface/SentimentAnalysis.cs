using System.Net;
using Microsoft.Extensions.Logging;
using SentimentGpt.ServiceModel;
using SentimentGpt.ServiceModel.Types;
using ServiceStack;
using ServiceStack.Gpt;
using ServiceStack.OrmLite;

namespace SentimentGpt.ServiceInterface;

public class SentimentAnalysisService : Service
{
    public ITypeChatProvider TypeChatProvider { get; set; }
    public IPromptProvider PromptProvider { get; set; }
    public AppConfig Config { get; set; }
    public ILoggerFactory LoggerFactory { get; set; }
    public ILogger Logger => LoggerFactory.CreateLogger(typeof(SentimentAnalysisService));

    public IAutoQueryDb AutoQuery { get; set; }
    public ISpeechToText SpeechToText { get; set; }
    
    public async Task<object> Any(TranscribeAudio request)
    {
        var recording = (Recording)await AutoQuery.CreateAsync(request, Request);

        var transcribeStart = DateTime.UtcNow;
        await Db.UpdateOnlyAsync(() => new Recording { TranscribeStart = transcribeStart },
            where: x => x.Id == recording.Id);

        ResponseStatus? responseStatus = null;
        try
        {
            var response = await SpeechToText.TranscribeAsync(request.Path);
            var transcribeEnd = DateTime.UtcNow;
            await Db.UpdateOnlyAsync(() => new Recording
            {
                Provider = SpeechToText.GetType().Name,
                Transcript = response.Transcript,
                TranscriptConfidence = response.Confidence,
                TranscriptResponse = response.ApiResponse,
                TranscribeEnd = transcribeEnd,
                TranscribeDurationMs = (int)(transcribeEnd - transcribeStart).TotalMilliseconds,
                Error = response.ResponseStatus.ToJson(),
            }, where: x => x.Id == recording.Id);
            responseStatus = response.ResponseStatus;
        }
        catch (Exception e)
        {
            await Db.UpdateOnlyAsync(() => new Recording { Error = e.ToString() },
                where: x => x.Id == recording.Id);
            responseStatus = e.ToResponseStatus();
        }

        recording = await Db.SingleByIdAsync<Recording>(recording.Id);

        WriteJsonFile($"/speech-to-text/{recording.CreatedDate:yyyy/MM/dd}/{recording.CreatedDate.TimeOfDay.TotalMilliseconds}.json", 
            recording.ToJson());

        if (responseStatus != null)
            throw new HttpError(responseStatus, HttpStatusCode.BadRequest);

        return recording;
    }
    
    public async Task<object> Post(ProcessSentiment request)
    {
        try
        {
            var schema = await PromptProvider.CreateSchemaAsync();
            var prompt = await PromptProvider.CreatePromptAsync(request.UserMessage);
            var result = await TypeChatProvider.TranslateMessageAsync(CreateTypeChatRequest(schema, prompt, request.UserMessage));
            var response = result.Result.FromJson<SentimentResult>();
            var sentiment = response.Sentiment ?? SentimentType.Neutral;
            var sentimentResponse = new SentimentResponse
            {
                Text = request.UserMessage,
                Sentiment = sentiment,
            };
            Db.Insert(sentimentResponse);
            return sentimentResponse;
        }
        catch (Exception e)
        {
            Logger.LogError(e,"Error processing sentiment");
            throw;
        }
    }
    
    void WriteJsonFile(string path, string json)
    {
        ThreadPool.QueueUserWorkItem(_ => {
            try
            {
                VirtualFiles.WriteFile(path, json);
            }
            catch (Exception ignore) {}
        });
    }    
    
    public TypeChatRequest CreateTypeChatRequest(string schema, string prompt,string userMessage) => new(schema, prompt, userMessage) {
        NodePath = Config.NodePath,
        NodeProcessTimeoutMs = Config.NodeProcessTimeoutMs,
        WorkingDirectory = Environment.CurrentDirectory,
        SchemaPath = Config.SiteConfig.GptPath.CombineWith("schema.ts"),
        TypeChatTranslator = TypeChatTranslator.Json
    };
}

