using FussballNews.ai.OpenAi.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace FussballNews.ai.OpenAi.Services;


public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string text);
}
public class EmbeddingService : IEmbeddingService
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _modelName;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        AiOptions aiOptions,
        IOpenAiClientFactory openAiClientFactory,
        ILogger<EmbeddingService> logger
    )
    {
        _openAIClient = openAiClientFactory.CreateClient();
        _modelName = aiOptions.EmbeddingModelName;
        _logger = logger;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string text)
    {
        var client = _openAIClient.GetEmbeddingClient(_modelName);
        var response = await client.GenerateEmbeddingAsync(text);
        return response.Value.ToFloats();
    }
}