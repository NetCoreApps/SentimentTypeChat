using ServiceStack.Gpt;

namespace SentimentGpt.ServiceInterface;

public class AppConfig
{
    public string Project { get; set; }
    public string Location { get; set; }
    public SiteConfig SiteConfig { get; set; }
    public string NodePath { get; set; }
    public string? FfmpegPath { get; set; }
    public string? WhisperPath { get; set; }
    public int NodeProcessTimeoutMs { get; set; } = 120 * 1000;
    
    public GoogleCloudSpeechConfig GoogleCloudSpeechConfig() => new()
    {
        Project = Project,
        Location = Location,
        Bucket = SiteConfig.Bucket,
        RecognizerId = SiteConfig.RecognizerId,
        PhraseSetId = SiteConfig.PhraseSetId,
    };
}

public class SiteConfig
{
    public string GptPath { get; set; }
    public string Bucket { get; set; }
    public string RecognizerId { get; set; }
    public string PhraseSetId { get; set; }
}
