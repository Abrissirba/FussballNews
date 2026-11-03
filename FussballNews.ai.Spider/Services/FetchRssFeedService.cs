using FussballNews.ai.Spider.Models;

namespace FussballNews.ai.Spider.Services;

public interface IFetchRssFeedService
{
    Task<RssFeedContent?> FetchRssFeedAsync(string url);
}

public class FetchRssFeedService(
    IHttpClientFactory httpClientFactory
) : IFetchRssFeedService
{
    public async Task<RssFeedContent?> FetchRssFeedAsync(string url)
    {
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch RSS feed from {url}: {response.StatusCode}");
        }
        var content = await response.Content.ReadAsStringAsync();
        return RssFeedContent.Parse(content);
    }
}