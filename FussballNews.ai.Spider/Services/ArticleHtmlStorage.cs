using Azure.Storage.Blobs;

namespace FussballNews.ai.Spider.Services;

public interface IArticleHtmlStorage
{
    Task<string> UploadArticleHtmlAsync(string url, string html);
    Task<string?> GetArticleHtmlAsync(string url);
}

public class ArticleHtmlStorage(
    BlobServiceClient blobServiceClient
) : IArticleHtmlStorage
{
    public async Task<string> UploadArticleHtmlAsync(string url, string html)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient("article-html");
        var blobName = $"{Math.Abs(url.GetHashCode())}.html";
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(new BinaryData(html), overwrite: true);
        return blobName;
    }

    public async Task<string?> GetArticleHtmlAsync(string url)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient("article-html");
        var blobName = $"{Math.Abs(url.GetHashCode())}.html";
        var blobClient = containerClient.GetBlobClient(blobName);
        var content = await blobClient.DownloadContentAsync();
        return content.Value.ToString();
    }
}