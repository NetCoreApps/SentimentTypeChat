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
            ("SentimentResponses", typeof(SentimentResponse)),
            ("Recordings",         typeof(Recording)),
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
}
