using FussballNews.ai.Model;
using FussballNews.ai.Spider.Messages.Commands;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Rebus.Bus;

namespace FussballNews.ai.Spider.Services;

public class RssFeedSpider(
    IBus bus,
    FussballNewsAiDbContext dbContext
) : IJob
{
    private readonly IBus _bus = bus;

    public async Task Execute(IJobExecutionContext context)
    {
        // Get RSS feeds that:
        // 1. Are currently active
        // 2. Haven't been triggered in the last 60 minutes (LastFetchTriggered is older than 60 min ago)
        // 3. Don't have a pending fetch (last completed fetch is newer or equal to last trigger)
        var rssFeeds = await dbContext.RssFeeds
            .Where(x => 
                x.IsActive 
                && 
                x.LastFetchTriggered == null
                ||
                (
                    x.LastFetchTriggered < DateTime.UtcNow.AddMinutes(-60)
                    //&& 
                    //x.LastFetchedCompleted >= x.LastFetchTriggered // error prone
                )
            ).ToListAsync();

        foreach (var rssFeed in rssFeeds)
        {
            await _bus.Publish(new FetchRssFeedCommand(rssFeed.Id));
        }
    }
}
