namespace FussballNews.ai.Model.Models;
public class Source
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public required string ParserType { get; set; }
    public required string Language { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<RssFeed> RssFeeds { get; set; } = new List<RssFeed>();
}