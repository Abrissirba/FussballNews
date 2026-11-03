namespace FussballNews.ai.Model.Models;

public class Article
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public required string Title { get; set; }
    public required string HtmlBlobUrl { get; set; }
    public string? Markdown { get; set; }
    public DateTime Published { get; set; }
    public DateTime Created { get; set; }
    public required string Url { get; set; }
    public string? Language { get; set; }

    public Source? Source { get; set; }
    public ICollection<ArticleVector> ArticleVectors { get; set; } = new List<ArticleVector>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public void DeleteVectors()
    {
        ArticleVectors.Clear();
    }
}