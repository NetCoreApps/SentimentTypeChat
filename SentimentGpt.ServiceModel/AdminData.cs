using ServiceStack;

namespace SentimentGpt.ServiceModel;

public class AdminData : IGet, IReturn<AdminDataResponse> {}

public class PageStats
{
    public string Label { get; set; }
    public int Total { get; set; }
}

public class AdminDataResponse
{
    public List<PageStats> PageStats { get; set; }
}

public class ProcessSentiment : IReturn<SentimentResult>
{
    public string UserMessage { get; set; }
}