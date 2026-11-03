namespace FussballNews.ai.Spider.Messages.Commands;

public record FetchArticleCommand(
    string Url,
    int SourceId,
    int? RssFeedId
);