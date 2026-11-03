namespace FussballNews.ai.Model.Models;
public class RssFeed
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string? ParserType { get; set; }
    public string? Language { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastFetchTriggered { get; set; }
    public DateTime? LastFetchedCompleted { get; set; }

    public required Source Source { get; set; }
}