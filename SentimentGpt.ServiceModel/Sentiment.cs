using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SentimentGpt.ServiceModel;

public class SentimentResponse
{
    [AutoIncrement]
    public int Id { get; set; }
    
    public string? Text { get; set; }
    public SentimentType Sentiment { get; set; }
}

public enum SentimentType
{
    Positive,
    Negative,
    Neutral,
}

public class QuerySentimentResponse : QueryDb<SentimentResponse>
{
    public int? Id { get; set; }
}

public class SentimentResult
{
    public SentimentType? Sentiment { get; set; }
}



public class ProcessSentiment : GptRequestBase<SentimentResult>
{

}

public class GptRequestBase<T> : IGptRequest<T>
{
    public string UserRequest { get; set; }
    public Dictionary<string,object>? PromptContext { get; set; }
}

public interface IGptRequest<T> : IReturn<T>
{
    string UserRequest { get; set; }
    Dictionary<string,object>? PromptContext { get; set; }
}



