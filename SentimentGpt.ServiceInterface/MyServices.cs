using ServiceStack;
using SentimentGpt.ServiceModel;
using SentimentGpt.ServiceModel.Types;
using ServiceStack.Gpt;
using ServiceStack.OrmLite;

namespace SentimentGpt.ServiceInterface;

public class MyServices : Service
{
    public object Any(Hello request)
    {
        return new HelloResponse { Result = $"Hello, {request.Name}!" };
    }

    public async Task<object> Any(AdminData request)
    {
        var tables = new (string Label, Type Type)[] 
        {
            ("Bookings", typeof(Booking)),
            ("Coupons",  typeof(Coupon)),
        };
        var dialect = Db.GetDialectProvider();
        var totalSql = tables.Map(x => $"SELECT '{x.Label}', COUNT(*) FROM {dialect.GetQuotedTableName(x.Type.GetModelMetadata())}")
            .Join(" UNION ");
        var results = await Db.DictionaryAsync<string,int>(totalSql);
        
        return new AdminDataResponse {
            PageStats = tables.Map(x => new PageStats {
                Label = x.Label, 
                Total = results[x.Label],
            })
        };
    }
    
    public IAutoQueryDb AutoQuery { get; set; }
    public ISpeechToText SpeechToText { get; set; }
    
    public async Task<object> Any(TranscribeAudio request)
    {
        var recording = (Recording)await AutoQuery.CreateAsync(request, Request);

        var transcribeStart = DateTime.UtcNow;
        await Db.UpdateOnlyAsync(() => new Recording { TranscribeStart = transcribeStart },
            where: x => x.Id == recording.Id);

        try
        {
            var result = await SpeechToText.TranscribeAsync(request.Path);
            var transcribeEnd = DateTime.UtcNow;
            await Db.UpdateOnlyAsync(() => new Recording
            {
                Transcript = result.Transcript,
                TranscriptConfidence = result.Confidence,
                TranscriptResponse = result.ApiResponse,
                TranscribeEnd = transcribeEnd,
                TranscribeDurationMs = (int)(transcribeEnd - transcribeStart).TotalMilliseconds,
            }, where: x => x.Id == recording.Id);
        }
        catch (Exception e)
        {
            await Db.UpdateOnlyAsync(() => new Recording { Error = e.Message },
                where: x => x.Id == recording.Id);
        }

        recording = await Db.SingleByIdAsync<Recording>(recording.Id);

        WriteJsonFile($"/speech-to-text/{recording.CreatedDate:yyyy/MM/dd}/{recording.CreatedDate.TimeOfDay.TotalMilliseconds}.json", 
            recording.ToJson());

        return recording;
    }
    
    void WriteJsonFile(string path, string json)
    {
        ThreadPool.QueueUserWorkItem(_ => {
            try
            {
                VirtualFiles.WriteFile(path, json);
            }
            catch (Exception ignore) {}
        });
    }
}

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