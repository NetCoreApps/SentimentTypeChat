using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace SentimentGpt.Migrations;

public class Migration1000 : MigrationBase
{
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

    public class Recording
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Path { get; set; }
        public string Provider { get; set; }
        public string? Transcript { get; set; }
        public float? TranscriptConfidence { get; set; }
        public string? TranscriptResponse { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TranscribeStart { get; set; }
        public DateTime? TranscribeEnd { get; set; }
        public int? TranscribeDurationMs { get; set; }
        public int? DurationMs { get; set; }
        public string? IpAddress { get; set; }
        public string? Error { get; set; }
    }

    public override void Up()
    {
        Db.CreateTable<SentimentResponse>();
        Db.CreateTable<Recording>();
    }

    public override void Down()
    {
        Db.DropTable<Recording>();
        Db.DropTable<SentimentResponse>();
    }
}
