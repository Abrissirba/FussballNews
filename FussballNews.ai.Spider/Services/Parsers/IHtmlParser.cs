namespace FussballNews.ai.Spider.Services.Parsers;

public record ArticleContent(
    string? Title,
    string? MarkdownContent,
    string FullHtmlContent,
    DateTime? PublishedDate
);

public interface IHtmlParser
{
    public string TypeName { get; }

    Task<ArticleContent?> ParseArticleAsync(string url);
}