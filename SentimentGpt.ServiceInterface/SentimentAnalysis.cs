using System.Data;
using System.Diagnostics;
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
    
    public async Task<object> Post(ProcessSentiment request)
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
}

