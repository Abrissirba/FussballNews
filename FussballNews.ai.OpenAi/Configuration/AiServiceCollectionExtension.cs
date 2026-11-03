using FussballNews.ai.OpenAi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FussballNews.ai.OpenAi.Configuration;

public static class AiServiceCollectionExtension
{
    public static IServiceCollection AddFussballNewsOpenAi(
        this IServiceCollection services,
        Action<AiOptions> configureOptions
    )
    {
        var options = new AiOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IOpenAiClientFactory, OpenAiClientFactory>();
        return services;
    }
}