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
