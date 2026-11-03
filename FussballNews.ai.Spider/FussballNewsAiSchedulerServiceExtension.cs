using FussballNews.ai.Spider.Handlers;
using FussballNews.ai.Spider.Messages.Commands;
using FussballNews.ai.Spider.Messages.Events;
using FussballNews.ai.Spider.Services;
using FussballNews.ai.Spider.Services.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Rebus.Bus;
using Rebus.Config;
using System.ComponentModel;

namespace FussballNews.ai.Spider;

public static class FussballNewsAiSpiderServiceExtension
{
    public static void AddFussballNewsAiSpider(this IServiceCollection services)
    {
        services.AutoRegisterHandlersFromAssemblyOf<RssFeedSpider>();
        services.AddScoped<IFetchRssFeedService, FetchRssFeedService>();
        services.AddScoped<IHtmlParser, KickerParser>();
        services.AddScoped<IParserResolver, ParserResolver>();
        services.AddScoped<IArticleHtmlStorage, ArticleHtmlStorage>();

        services.AddQuartz(q =>
        {
            var rssFeedJobKey = new JobKey($"Job:rssFeedUpdate");
            q.AddJob<RssFeedSpider>(opts => opts
                .WithIdentity(rssFeedJobKey)
            );

            q.AddTrigger(opts => opts
                .ForJob(rssFeedJobKey)
                .WithIdentity($"Trigger:rssFeedUpdate")
                // Run immediately when the program starts and then every hour
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(1)
                    .RepeatForever()
                )
                .StartAt(DateTimeOffset.UtcNow.AddSeconds(5)) // Start 5 seconds after application starts
            );

        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    public static async Task AddRebusSubscriptions(IBus bus)
    {
        await Task.WhenAll(
            bus.Subscribe<FetchRssFeedCommand>(),
            bus.Subscribe<FetchArticleCommand>(),
            bus.Subscribe<ArticleFetchedEvent>(),
            bus.Subscribe<VectorizeArticleCommand>()
        );
    }
}