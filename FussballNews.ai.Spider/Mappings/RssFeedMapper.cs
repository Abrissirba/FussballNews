using FussballNews.ai.Model.Models;
using FussballNews.ai.Spider.Models;

namespace FussballNews.ai.Spider.Mappings;

public static class RssFeedMapper
{
    public static RssFeed Update(this RssFeed rssFeed, RssFeedContent rssFeedContent)
    {
        if (rssFeed.Language != rssFeedContent.Channel?.Language)
        {
            rssFeed.Language = string.IsNullOrEmpty(rssFeedContent.Channel?.Language) ? rssFeed.Language : rssFeedContent.Channel!.Language;
        }
        if (rssFeed.Title != rssFeedContent.Channel?.Title)
        {
            rssFeed.Title = string.IsNullOrEmpty(rssFeedContent.Channel?.Title) ? rssFeed.Title : rssFeedContent.Channel!.Title;
        }
        if (rssFeed.Description != rssFeedContent.Channel?.Description)
        {
            rssFeed.Description = string.IsNullOrEmpty(rssFeedContent.Channel?.Description) ? rssFeed.Description : rssFeedContent.Channel!.Description;
        }
        if (rssFeed.ImageUrl != rssFeedContent.Channel?.Image?.Url)
        {
            rssFeed.ImageUrl = string.IsNullOrEmpty(rssFeedContent.Channel?.Image?.Url) ? rssFeed.ImageUrl : rssFeedContent.Channel!.Image!.Url;
        }
        if (rssFeed.Category != rssFeedContent.Channel?.Category)
        {
            rssFeed.Category = string.IsNullOrEmpty(rssFeedContent.Channel?.Category) ? rssFeed.Category : rssFeedContent.Channel!.Category;
        }
        return rssFeed;
    }
}