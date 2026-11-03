using FussballNews.ai.Model;

namespace FussballNews.ai.Spider.Services.Parsers;

public interface IParserResolver
{
    Task<IHtmlParser> GetParserAsync(int sourceId, int? rssFeedId);
}

public class ParserResolver(
    IEnumerable<IHtmlParser> parsers,
    FussballNewsAiDbContext dbContext
) : IParserResolver
{
    public async Task<IHtmlParser> GetParserAsync(int sourceId, int? rssFeedId)
    {
        string? parserType = null;
        var rssFeed = rssFeedId != null ? await dbContext.RssFeeds.FindAsync(rssFeedId.Value) : null;
        if (string.IsNullOrEmpty(rssFeed?.ParserType))
        {
            var source = await dbContext.Sources.FindAsync(sourceId);
            if (source == null)
            {
                throw new Exception($"Source {sourceId} not found");
            }
            parserType = source.ParserType;
        }
        else {
            parserType = rssFeed.ParserType;
        }
        if (parserType == null)
        {
            throw new Exception($"No parser type found for source {sourceId} and rss feed {rssFeedId}");
        }
        var parser = parsers.FirstOrDefault(p => p.TypeName == parserType);
        if (parser == null)
        {
            throw new Exception($"Parser type {parserType} not found");
        }
        return parser;
    }
}