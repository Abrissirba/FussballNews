using FussballNews.ai.OpenAi.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace FussballNews.ai.OpenAi.Services;

public interface IChatService
{
    Task<string> GenerateResponseAsync(
        string chatMessage,
        string? systemPrompt
    );
}

public class ChatService : IChatService
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _modelName;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        AiOptions aiOptions,
        IOpenAiClientFactory openAiClientFactory,
        ILogger<ChatService> logger
    )
    {
        _openAIClient = openAiClientFactory.CreateClient();
        _modelName = aiOptions.ChatModelName;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(
        string chatMessage,
        string? systemPrompt
    )
    {
        var client = _openAIClient.GetChatClient(_modelName);
        var responseWithoutStream = await client.CompleteChatAsync(new List<ChatMessage> {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(chatMessage)
        }, new ChatCompletionOptions
        {
            MaxOutputTokenCount = 3300,
            Temperature = (float?)0.5
        });
        return responseWithoutStream.Value.Content.First().Text;
    }
}
