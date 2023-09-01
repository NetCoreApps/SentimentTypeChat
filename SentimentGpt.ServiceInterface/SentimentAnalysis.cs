using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using SentimentGpt.ServiceModel;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Script;
using ServiceStack.Text;

namespace SentimentGpt.ServiceInterface;



public class SentimentAnalysisService : Service
{
    public ITypeChatProvider<SentimentResult> SentimentChatProvider { get; set; }
    public ILoggerFactory LoggerFactory { get; set; }
    public ILogger Logger => LoggerFactory.CreateLogger(typeof(SentimentAnalysisService));
    
    public async Task<object> Post(ProcessSentiment request)
    {
        try
        {
            var result = await SentimentChatProvider.ProcessAsync(request);
            var sentiment = result.Sentiment ?? SentimentType.Neutral;
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

