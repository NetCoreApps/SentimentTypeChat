using ServiceStack;
using ServiceStack.Gpt;
using ServiceStack.Script;

namespace SentimentGpt.ServiceInterface;

public class SentimentPromptProvider : IPromptProvider
{
    public AppConfig Config { get; set; }

    public SentimentPromptProvider(AppConfig config)
    {
        Config = config;
    }

    public async Task<string> CreateSchemaAsync(TypeChatRequest request, CancellationToken token = default)
    {
        var file = new FileInfo(Config.SiteConfig.GptPath.CombineWith("schema.ss"));
        if (file == null)
            throw HttpError.NotFound($"{Config.SiteConfig.GptPath}/schema.ss not found");

        var tpl = await file.ReadAllTextAsync(token: token);
        var context = new ScriptContext
        {
            Plugins = { new TypeScriptPlugin() }
        }.Init();

        var output = await new PageResult(context.OneTimePage(tpl))
        {
            Args = new Dictionary<string, object>()
        }.RenderScriptAsync(token: token);
        return output;
    }

    public async Task<string> CreatePromptAsync(TypeChatRequest request, CancellationToken token = default)
    {
        var file = new FileInfo(Config.SiteConfig.GptPath.CombineWith("prompt.ss"));
        if (file == null)
            throw HttpError.NotFound($"{Config.SiteConfig.GptPath}/prompt.ss not found");

        var schema = await CreateSchemaAsync(request, token: token);
        var tpl = await file.ReadAllTextAsync(token: token);
        var context = new ScriptContext
        {
            Plugins = { new TypeScriptPlugin() }
        }.Init();

        var prompt = await new PageResult(context.OneTimePage(tpl))
        {
            Args =
            {
                [nameof(schema)] = schema,
                [nameof(request)] = request,
            }
        }.RenderScriptAsync(token: token);

        return prompt;
    }
}