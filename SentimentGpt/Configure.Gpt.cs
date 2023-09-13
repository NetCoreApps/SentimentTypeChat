using Microsoft.SemanticKernel;
using ServiceStack.Gpt;
using SentimentGpt.ServiceInterface;

[assembly: HostingStartup(typeof(SentimentGpt.ConfigureGpt))]

namespace SentimentGpt;

public class ConfigureGpt : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<SentimentPromptProvider>();
            services.AddSingleton<IPromptProviderFactory>(c => new PromptProviderFactory {
                Providers = {
                    [Tags.Sentiment] = c.Resolve<SentimentPromptProvider>()
                }
            });

            // Call Open AI Chat API directly without going through node TypeChat
            var gptProvider = context.Configuration.GetValue<string>("TypeChatProvider");
            if (gptProvider == nameof(KernelTypeChat))
            {
                var kernel = Kernel.Builder
                    .WithOpenAIChatCompletionService(
                        Environment.GetEnvironmentVariable("OPENAI_MODEL")!, 
                        Environment.GetEnvironmentVariable("OPENAI_API_KEY")!)
                    .Build();
                services.AddSingleton(kernel);
                services.AddSingleton<ITypeChat>(c =>  new KernelTypeChat(c.Resolve<IKernel>()));
            }
            else if (gptProvider == nameof(NodeTypeChat))
            {
                // Call Open AI Chat API through node TypeChat
                services.AddSingleton<ITypeChat>(c => new NodeTypeChat());
            }
            else throw new NotSupportedException($"Unknown TypeChat Provider: {gptProvider}");
        });
}