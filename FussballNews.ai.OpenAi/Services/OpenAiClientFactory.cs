using Azure;
using Azure.AI.OpenAI;
using FussballNews.ai.OpenAi.Configuration;
using OpenAI;

namespace FussballNews.ai.OpenAi.Services;

public interface IOpenAiClientFactory
{
    OpenAIClient CreateClient();
}

public class OpenAiClientFactory : IOpenAiClientFactory
{
    private readonly AiOptions _aiOptions;

    public OpenAiClientFactory(
        AiOptions aiOptions
    )
    {
        _aiOptions = aiOptions;
    }
    public OpenAIClient CreateClient()
    {
        if (_aiOptions.Endpoint != null && _aiOptions.Endpoint.Contains("azure.com"))
                    {
            return new AzureOpenAIClient(new Uri(_aiOptions.Endpoint), new AzureKeyCredential(_aiOptions.ApiKey));
        }
        return new OpenAIClient(new System.ClientModel.ApiKeyCredential(_aiOptions.ApiKey));
    }
}
