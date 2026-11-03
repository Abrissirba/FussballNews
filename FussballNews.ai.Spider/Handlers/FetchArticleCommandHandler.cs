using Azure.Storage.Blobs;
using DnsClient.Internal;
using FussballNews.ai.Model;
using FussballNews.ai.Model.Models;
using FussballNews.ai.Spider.Messages.Commands;
using FussballNews.ai.Spider.Messages.Events;
using FussballNews.ai.Spider.Services;
using FussballNews.ai.Spider.Services.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace FussballNews.ai.Spider.Handlers;

public class FetchArticleCommandHandler(
    IBus bus,
    FussballNewsAiDbContext dbContext,
    IArticleHtmlStorage articleHtmlStorage,
    ILogger<FetchArticleCommandHandler> logger,
    IParserResolver parserResolver
) : IHandleMessages<FetchArticleCommand>
{
    public async Task Handle(FetchArticleCommand command)
    {
        logger.LogInformation("Handling FetchArticleCommand for URL: {Url}", command.Url);
        if (string.IsNullOrWhiteSpace(command.Url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(command.Url));
        }
        var source = await dbContext.Sources.FindAsync(command.SourceId);
        IHtmlParser parser = await parserResolver.GetParserAsync(command.SourceId, command.RssFeedId);
        var articleContent = await parser.ParseArticleAsync(command.Url);
        if (!string.IsNullOrEmpty(articleContent?.MarkdownContent))
        {
            var blobName = await articleHtmlStorage.UploadArticleHtmlAsync(command.Url, articleContent.FullHtmlContent);
            var article = await dbContext.Articles.FirstOrDefaultAsync(a => a.Url == command.Url);
            if (article == null)
            {
                logger.LogInformation("Creating new article for URL: {Url}", command.Url);
                article = new Article
                {
                    Url = command.Url,
                    SourceId = command.SourceId,
                    HtmlBlobUrl = blobName,
                    Title = articleContent.Title,
                    Markdown = articleContent.MarkdownContent,
                    Published = articleContent.PublishedDate ?? DateTime.UtcNow,
                    Created = DateTime.UtcNow,
                    Language = source!.Language,
                    Tags = new List<Tag>(),
                    ArticleVectors = new List<ArticleVector>(),
                };
                dbContext.Articles.Add(article);
            }
            else
            {
                logger.LogInformation("Updating existing article for URL: {Url}", command.Url);
                article.Markdown = articleContent.MarkdownContent;
                article.Title = articleContent.Title;
                article.ArticleVectors = new List<ArticleVector>();
            }
            await dbContext.SaveChangesAsync();
            await bus.Publish(new ArticleFetchedEvent(
                ArticleId: article!.Id
            ));
            logger.LogInformation("Article fetched and published for URL: {Url}", command.Url);
        }
        else
        {
            logger.LogWarning($"Failed to parse article content from {command.Url}. The parser returned null or empty content.");
            //throw new Exception($"Failed to parse article content from {command.Url}. The parser returned null or empty content.");
        }
    }
}