using FussballNews.ai.Model;
using FussballNews.ai.Model.Helpers;
using FussballNews.ai.Model.Models;
using FussballNews.ai.Spider.Messages.Commands;
using FussballNews.ai.OpenAi.Services;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Microsoft.Extensions.Logging;

namespace FussballNews.ai.Spider.Handlers;

public class VectorizeArticleCommandHandler(
    FussballNewsAiDbContext dbContext,
    IEmbeddingService embeddingService,
    ILogger<VectorizeArticleCommandHandler> logger
) : IHandleMessages<VectorizeArticleCommand>
{
    public async Task Handle(VectorizeArticleCommand message)
    {
        logger.LogInformation("Vectorizing article with ID: {ArticleId}", message.ArticleId);
        var article = await dbContext.Articles
            .Include(a => a.ArticleVectors)
            .FirstOrDefaultAsync(a => a.Id == message.ArticleId);
        if (article == null)
        {
            throw new Exception($"Article with ID: {message.ArticleId} not found");
        }
        // delete existing vectors
        article.DeleteVectors();

        var chunks = ChunkTextWithOverlapBySentence(article.Markdown!, 3500);
        var index = 0;
        foreach (var chunk in chunks)
        {
            var vector = await embeddingService.GenerateEmbeddingsAsync(chunk);
            article.ArticleVectors.Add(new ArticleVector
            {
                ArticleId = message.ArticleId,
                Content = chunk,
                Embedding = vector.ToArray(),
                Index = index++,
                Created = DateTime.UtcNow
            });
        }
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Article vectorized with ID: {ArticleId} and {VectorCount} vectors", message.ArticleId, article.ArticleVectors.Count);
    }

    /// <summary>
    /// Splits text into overlapping chunks by character length.
    /// </summary>
    /// <param name="text">The input text to chunk.</param>
    /// <param name="maxChunkLength">Maximum length of each chunk (in characters).</param>
    /// <param name="overlapPercentage">Percentage of overlap between chunks (0-100).</param>
    /// <returns>List of text chunks.</returns>
    public static List<string> ChunkTextWithOverlapBySentence(string text, int maxChunkLength, int overlapPercentage = 10)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        // Validate overlap percentage
        if (overlapPercentage < 0 || overlapPercentage >= 100)
        {
            throw new ArgumentException("Overlap percentage must be between 0 and 100", nameof(overlapPercentage));
        }

        int startIndex = 0;
        int stride = (int)(maxChunkLength * (1 - overlapPercentage / 100.0));
        
        while (startIndex < text.Length)
        {
            int length = Math.Min(maxChunkLength, text.Length - startIndex);
            string chunk = text.Substring(startIndex, length);
            chunks.Add(chunk);
            startIndex += stride;
        }

        return chunks;
    }
}