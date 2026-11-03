using AngleSharp;
using AngleSharp.Dom;
using ReverseMarkdown;
using System.Globalization;
using System.Net;
using static ReverseMarkdown.Config;

namespace FussballNews.ai.Spider.Services.Parsers;

public class KickerParser(
    IHttpClientFactory httpClientFactory
) : IHtmlParser
{
    public string TypeName { get; } = "kicker";
    private static HttpClient _httpClient;
    private static CookieContainer _cookieContainer = new CookieContainer();

    public async Task<ArticleContent?> ParseArticleAsync(string url)
    {
        if (_httpClient == null)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer
            };

            _httpClient = new HttpClient(handler);

            var rnd = Random.Shared;
            int major = rnd.Next(80, 130);
            int minor = rnd.Next(0, 10);
            int build = rnd.Next(0, 5000);
            int patch = rnd.Next(0, 100);

            string userAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{major}.{minor}.{build}.{patch} Safari/537.36";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            //// Set a cookie manually
            //cookieContainer.Add(new Uri("https://example.com"), new Cookie("mycookie", "myvalue"));
            _cookieContainer.Add(new Uri("https://kicker.de.com"), new Cookie("aws-waf-token", "2f567891-688e-4e47-95c6-017e7d6201a5:DQoAlc80n78BAAAA:cD0nF5AmsQM0OmlVglMZDfJS1omAKsJRapYj4ayTBqSKtemd00e0ceEu5O+jqG9zc1s6KGThEziMTkOIJdF20IQdXu8uaS1wgHUbQpCDr3S/8ps1rExyUHXQAvc87N/vxH0nl/OyTOLhA+gXLy30XSlu9XqHEq1znqdoRxCMdcmtSHWL/Oo927hd2ZLuajRK"));

            //// Make a request (cookies will be sent)
            //var response = await client.GetAsync("https://example.com");

            //// Read cookies set by the server
            //var cookies = cookieContainer.GetCookies(new Uri("https://example.com"));
            //foreach (Cookie cookie in cookies)
            //{
            //    Console.WriteLine($"{cookie.Name} = {cookie.Value}");
            //}
        }

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch article from {url}: {response.StatusCode}, content: {content}");
        }
        else if (string.IsNullOrWhiteSpace(content))
        {
            response = await _httpClient.GetAsync(url);
            content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Empty content received from {url}");
        }

        var config = Configuration.Default.WithDefaultLoader();
        var browsingContext = BrowsingContext.New(config);
        var document = await browsingContext.OpenAsync(req => req.Content(content));
        var uri = new Uri(url);
        var hostUrl = $"{uri.Scheme}://{uri.Host}";
        var publishedEl = document.QuerySelector(".kick__article__time")?.TextContent;
        var publishedDate = ParseKickerDate(publishedEl);
        var articleContent = new ArticleContent(
            Title: document.QuerySelector(".kick__article__header h2")?.TextContent ?? document.QuerySelector("h1")?.TextContent,
            MarkdownContent: GetContentAsMarkdown(document, hostUrl),
            FullHtmlContent: document.DocumentElement?.OuterHtml ?? "Unknown",
            PublishedDate: publishedDate
        );
        await Task.Delay(2000); // Delay to avoid hitting the server too hard
        return articleContent;
    }

    private static DateTime ParseKickerDate(string? publishedEl)
    {
        if (string.IsNullOrWhiteSpace(publishedEl))
            return DateTime.UtcNow;

        // Try exact format first
        if (DateTime.TryParseExact(publishedEl, "dd.MM.yy - HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        // Handle relative formats like "vor 5 Stunden", "vor 2 Minuten"
        publishedEl = publishedEl.Trim().ToLowerInvariant();
        if (publishedEl.StartsWith("vor "))
        {
            var parts = publishedEl.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && int.TryParse(parts[1], out int value))
            {
                if (parts[2].StartsWith("stunde")) // "Stunden" or "Stunde"
                    return DateTime.UtcNow.AddHours(-value);
                if (parts[2].StartsWith("minute")) // "Minuten" or "Minute"
                    return DateTime.UtcNow.AddMinutes(-value);
                if (parts[2].StartsWith("sekunde")) // "Sekunden" or "Sekunde"
                    return DateTime.UtcNow.AddSeconds(-value);
                if (parts[2].StartsWith("tag")) // "Tage" or "Tag"
                    return DateTime.UtcNow.AddDays(-value);
            }
        }

        // Fallback
        return DateTime.UtcNow;
    }

    private string? GetContentAsMarkdown(
        IDocument document,
        string hostUrl
    )
    {
        var mainContent = document.QuerySelector("main.kick__article__content");

        if (mainContent == null)
        {
            return null;
        }

        RemoveTag(mainContent, ".kick__article__podcast_embed");
        RemoveTag(mainContent, ".kick__ad-pos_wrapper");
        RemoveTag(mainContent, ".kick__modul kick__modul--news");
        RemoveTag(mainContent, ".kick__bonus-stat__mask");
        RemoveTag(mainContent, ".kick__article__share__end");

        // Serialize the modified HTML document back to a string
        var converter = new Converter(new Config
        {
            UnknownTags = UnknownTagsOption.Drop
        });

        var links = mainContent.QuerySelectorAll("a");
        foreach (var link in links)
        {
            var href = link.GetAttribute("href");
            var isRelativeUrl = !string.IsNullOrEmpty(href) && !href.StartsWith("http");
            var absoluteUrl = isRelativeUrl ? $"{hostUrl}{href}" : href;
            link.SetAttribute("href", absoluteUrl);
        }

        // Also handle images
        var images = mainContent.QuerySelectorAll("img");
        foreach (var img in images)
        {
            var src = img.GetAttribute("src");
            var isRelativeUrl = !string.IsNullOrEmpty(src) && !src.StartsWith("http");
            var absoluteUrl = isRelativeUrl ? $"{hostUrl}{src}" : src;
            img.SetAttribute("src", absoluteUrl);
        }

        // Handle picture elements by extracting img tags and removing picture tags
        var pictures = mainContent.QuerySelectorAll("picture");
        foreach (var picture in pictures)
        {
            var img = picture.QuerySelector("img");
            if (img != null)
            {
                // Insert the img element before the picture element
                picture.Parent?.InsertBefore(img, picture);
            }
            // Remove the picture element and all its content
            picture.Remove();
        }

        var content = mainContent.ToHtml(new AngleSharp.Html.MinifyMarkupFormatter());
        var tagsToReplaceWithDiv = new[]
        {
            "article", "aside", "figure", "header", "footer", "nav", "main"
        };
        foreach (var tag in tagsToReplaceWithDiv)
        {
            content = content.Replace($"<{tag}", $"<div");
            content = content.Replace($"</{tag}>", "</div>");
        }

        string markdown = converter.Convert(content);

        return markdown;
    }

    private void RemoveTag(IElement document, string selector)
    {
        var els = document.QuerySelectorAll(selector).ToList();
        foreach (var el in els)
        {
            el.Remove();
        }
    }
}