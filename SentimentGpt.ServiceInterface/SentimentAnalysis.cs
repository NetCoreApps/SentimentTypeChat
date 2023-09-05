using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using SentimentGpt.ServiceModel;
using ServiceStack;
using ServiceStack.Gpt;
using ServiceStack.OrmLite;
using ServiceStack.Script;
using ServiceStack.Text;

namespace SentimentGpt.ServiceInterface;



public class SentimentAnalysisService : Service
{
    public ITypeChatProvider TypeChatProvider { get; set; }
    public IPromptProvider PromptProvider { get; set; }
    public AppConfig Config { get; set; }
    public ILoggerFactory LoggerFactory { get; set; }
    public ILogger Logger => LoggerFactory.CreateLogger(typeof(SentimentAnalysisService));
    
    public TypeChatRequest CreateTypeChatRequest(string userMessage) => new(PromptProvider, userMessage) {
        NodePath = Config.NodePath,
        NodeProcessTimeoutMs = Config.NodeProcessTimeoutMs,
        WorkingDirectory = Environment.CurrentDirectory,
        SchemaPath = Config.SiteConfig.GptPath.CombineWith("schema.ts"),
        TypeChatTranslator = TypeChatTranslator.Json
    };
    
    public async Task<object> Post(ProcessSentiment request)
    {
        try
        {
            var result = await TypeChatProvider.TranslateMessageAsync(CreateTypeChatRequest(request.UserRequest));
            var response = result.Result.FromJson<SentimentResult>();
            var sentiment = response.Sentiment ?? SentimentType.Neutral;
            var sentimentResponse = new SentimentResponse
            {
                Text = request.UserRequest,
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
}

