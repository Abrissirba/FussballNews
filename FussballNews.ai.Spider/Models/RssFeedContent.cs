namespace FussballNews.ai.Spider.Models;

public record RssItem(
    string? Title,
    string? Link, 
    string? Description,
    string? Content,
    List<string>? Categories,
    string? Guid,
    DateTime? PubDate
);

public record RssImage(
    string? Url,
    string? Title,
    string? Link,
    int? Width,
    int? Height
);

public record RssChannel(
    int? Ttl,
    string? Copyright,
    string? Author,
    DateTime? LastBuildDate,
    DateTime? PubDate,
    string? Title,
    string? Description,
    string? Link,
    string? Language,
    RssImage? Image,
    string? Category,
    List<RssItem>? Items
);

public record RssFeedContent(RssChannel? Channel)
{
    public static RssFeedContent? Parse(string content)
    {
        var doc = new System.Xml.XmlDocument();
        var nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
        doc.LoadXml(content);

        var channel = doc.SelectSingleNode("//channel");
        if (channel == null) {
            return null;
        }

        var image = channel.SelectSingleNode("image");
        var items = channel.SelectNodes("item");

        RssImage? rssImage = null;
        if (image != null)
        {
            rssImage = new RssImage(
                image.SelectSingleNode("url")?.InnerText,
                image.SelectSingleNode("title")?.InnerText,
                image.SelectSingleNode("link")?.InnerText,
                int.TryParse(image.SelectSingleNode("width")?.InnerText, out int width) ? width : null,
                int.TryParse(image.SelectSingleNode("height")?.InnerText, out int height) ? height : null
            );
        }

        var rssItems = new List<RssItem>();
        if (items != null)
        {
            foreach (System.Xml.XmlNode item in items)
            {
                var categories = new List<string>();
                var categoryNodes = item.SelectNodes("category");
                if (categoryNodes != null)
                {
                    foreach (System.Xml.XmlNode category in categoryNodes)
                    {
                        if (category.InnerText != null)
                        {
                            categories.Add(category.InnerText);
                        }
                    }
                }

                DateTime? pubDate = null;
                if (DateTime.TryParse(item.SelectSingleNode("pubDate")?.InnerText, out DateTime parsedDate))
                {
                    pubDate = parsedDate;
                }

                rssItems.Add(new RssItem(
                    Title: item.SelectSingleNode("title")?.InnerText,
                    Link: item.SelectSingleNode("link")?.InnerText,
                    Description: item.SelectSingleNode("description")?.InnerText,
                    Content: item.SelectSingleNode("content:encoded", nsmgr)?.InnerText,
                    Categories: categories,
                    Guid: item.SelectSingleNode("guid")?.InnerText,
                    PubDate: pubDate
                ));
            }
        }

        DateTime? lastBuildDate = null;
        if (DateTime.TryParse(channel.SelectSingleNode("lastBuildDate")?.InnerText, out DateTime parsedLastBuildDate))
        {
            lastBuildDate = parsedLastBuildDate;
        }

        DateTime? channelPubDate = null;
        if (DateTime.TryParse(channel.SelectSingleNode("pubDate")?.InnerText, out DateTime parsedPubDate))
        {
            channelPubDate = parsedPubDate;
        }

        var rssChannel = new RssChannel(
            Ttl: int.TryParse(channel.SelectSingleNode("ttl")?.InnerText, out int ttl) ? ttl : null,
            Copyright: channel.SelectSingleNode("copyright")?.InnerText,
            Author: channel.SelectSingleNode("author")?.InnerText,
            LastBuildDate: lastBuildDate,
            PubDate: channelPubDate,
            Title: channel.SelectSingleNode("title")?.InnerText,
            Description: channel.SelectSingleNode("description")?.InnerText,
            Link: channel.SelectSingleNode("link")?.InnerText,
            Language: channel.SelectSingleNode("language")?.InnerText,
            Image: rssImage,
            Category: channel.SelectSingleNode("category")?.InnerText,
            Items: rssItems
        );

        return new RssFeedContent(rssChannel);
    }

    public IReadOnlyList<string> GetItemUrls()     {
        if (Channel?.Items == null)
        {
            return Array.Empty<string>();
        }
        return Channel.Items
            .Where(item => !string.IsNullOrEmpty(item.Link))
            .Select(item => item.Link!.Replace("#omrss", ""))
            .ToList();
    }
}
