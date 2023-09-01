using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace SentimentGpt.Migrations;

public class Migration1001 : MigrationBase
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
    
    public override void Up()
    {
        Db.CreateTable<SentimentResponse>();
    }
    
    public override void Down()
    {
        Db.DropTable<SentimentResponse>();
    }
}