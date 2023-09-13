using SentimentGpt.ServiceModel.Types;
using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SentimentGpt.ServiceModel;

[AutoPopulate(nameof(Recording.CreatedDate),  Eval = "utcNow")]
[AutoPopulate(nameof(Recording.IpAddress),  Eval = "Request.RemoteIp")]
public class TranscribeAudio : ICreateDb<Recording>, IReturn<Recording>
{
    [Input(Type="file"), UploadTo("recordings")]
    public string Path { get; set; }
}

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
