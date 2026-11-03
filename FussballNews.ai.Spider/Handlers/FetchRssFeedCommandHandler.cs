using DnsClient.Internal;
using FussballNews.ai.Model;
using FussballNews.ai.Spider.Mappings;
using FussballNews.ai.Spider.Messages.Commands;
using FussballNews.ai.Spider.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace FussballNews.ai.Spider.Handlers;

public class FetchRssFeedCommandHandler(
    IBus bus,
    FussballNewsAiDbContext dbContext,
    IFetchRssFeedService fetchRssFeedService,
    ILogger<FetchRssFeedCommandHandler> logger
) : IHandleMessages<FetchRssFeedCommand>
{
    public async Task Handle(FetchRssFeedCommand command)
    {
        logger.LogInformation("Handling FetchRssFeedCommand for RssFeedId: {RssFeedId}", command.RssFeedId);
        var rssFeed = await dbContext.RssFeeds.FindAsync(command.RssFeedId);
        if (rssFeed == null)
        {
            throw new Exception($"RSS feed with id {command.RssFeedId} not found");
        }

        var rssFeedContent = await fetchRssFeedService.FetchRssFeedAsync(rssFeed.Url);
        if (rssFeedContent == null)
        {
            throw new Exception($"Failed to fetch RSS feed from {rssFeed.Url}");
        }

        rssFeed.Update(rssFeedContent);
        if (rssFeedContent.Channel?.Items != null)
        {
            var urls = rssFeedContent.GetItemUrls();
            var unknownUrls = urls.Where(x => !x.EndsWith("/video") && !x.EndsWith("/slideshow")).ToList();
            if (unknownUrls.Any())
            {
                logger.LogWarning("Found {UnknownUrlCount} unknown URLs in RSS feed for RssFeedId: {RssFeedId}", unknownUrls.Count, rssFeed.Id);
            }
            urls = urls.Where(x => !x.EndsWith("/video") && !x.EndsWith("/slideshow")).ToList();
            var articles = await dbContext
                .Articles
                .Where(a => urls.Contains(a.Url))
                .ToListAsync();
            var newArticles = rssFeedContent
                .Channel
                .Items
                .Where(item => !articles.Any(a => a.Url.ToLower() == item.Link?.ToLower())).ToList();
            logger.LogInformation("Found {NewArticleCount} new articles for RssFeedId: {RssFeedId}", newArticles.Count, rssFeed.Id);
            foreach (var item in newArticles)
            {
                await bus.Publish(new FetchArticleCommand(
                    Url: item.Link!,
                    SourceId: rssFeed.SourceId,
                    RssFeedId: rssFeed.Id
                ));
            }
        }
        rssFeed.LastFetchTriggered = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Finished handling FetchRssFeedCommand for RssFeedId: {RssFeedId}", command.RssFeedId);
    }
}