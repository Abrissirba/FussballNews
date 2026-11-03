using FussballNews.ai.Model;
using FussballNews.ai.OpenAi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FussballNews.ai.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IEmbeddingService embeddingService,
    IChatService chatService,
    FussballNewsAiDbContext dbContext
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var embedding = await embeddingService.GenerateEmbeddingsAsync(request.Message);
        var vectors = await dbContext
            .ArticleVectors
            .Where(p => EF.Functions.VectorDistance("cosine", p.Embedding, embedding.ToArray()) < 0.4)
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, embedding.ToArray()))
            .Take(5)
            .Select(x => new
            {
                ArticleId = x.ArticleId,
                VectorDistance = EF.Functions.VectorDistance("cosine", x.Embedding, embedding.ToArray()),
                Content = x.Content
            })
            .ToArrayAsync();
        var articleIds = vectors.Select(v => v.ArticleId).ToArray();
        var articles = await dbContext.Articles.Where(a => articleIds.Contains(a.Id)).ToArrayAsync();

        var articleContentsString = string.Join("\n", articles.Select(a =>
            $"Title: {a.Title}\nPublished: {a.Published:yyyy-MM-dd HH:mm}\nUrl: {a.Url}\nContent:\n{a.Markdown}\n"
        ));

        var systemPrompt = $"""
        You are a helpful assistant that is an export on football. 
        You have access to the latest news and will answer on questions from the user.
        Use the specified soures as much as possible.
        Add sources to the generated answer so that the user can check the news themselves for mor info.
        You have access to the following articles:
        {articleContentsString}
        """;
        var response = await chatService.GenerateResponseAsync(request.Message, systemPrompt);
        return Ok(new ChatResponse(response));
    }
}

public record ChatRequest(string Message);

public record ChatResponse(string Message);