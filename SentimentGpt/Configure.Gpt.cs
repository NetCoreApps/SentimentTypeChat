using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Reliability;
using SentimentGpt.ServiceInterface;
using SentimentGpt.ServiceModel;

[assembly: HostingStartup(typeof(SentimentGpt.ConfigureGpt))]

namespace SentimentGpt;

public class ConfigureGpt : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            // Call Open AI Chat API directly without going through node TypeChat
            var gptProvider = context.Configuration.GetValue<string>("GptChatProvider");
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ConfigureGpt>>();
            logger.LogInformation($"Using GptChatProvider: {gptProvider}");
            if (gptProvider == nameof(KernelChatProvider<SentimentResult>))
            {
                var kernel = Kernel.Builder
                    .WithOpenAIChatCompletionService(
                        Environment.GetEnvironmentVariable("OPENAI_MODEL")!, 
                        Environment.GetEnvironmentVariable("OPENAI_API_KEY")!)
                    .WithRetryHandlerFactory(new DefaultHttpRetryHandlerFactory(
                        new HttpRetryConfig
                        {
                            MaxRetryCount = 3,
                        }))
                    .Build();
                services.AddSingleton(kernel);
                services.AddSingleton<ITypeChatProvider<SentimentResult>>(c => 
                    new KernelChatProvider<SentimentResult>(c.Resolve<AppConfig>(), c.Resolve<IKernel>()));
            }
            else if (gptProvider == nameof(NodeTypeChatProvider<SentimentResult>))
            {
                // Call Open AI Chat API through node TypeChat
                services.AddSingleton<ITypeChatProvider<SentimentResult>>(c =>
                    new NodeTypeChatProvider<SentimentResult>(c.Resolve<AppConfig>()));
            }
        });
}