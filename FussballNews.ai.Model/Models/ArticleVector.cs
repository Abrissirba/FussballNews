using System.ComponentModel.DataAnnotations.Schema;
using FussballNews.ai.Model.Helpers;

namespace FussballNews.ai.Model.Models;
public class ArticleVector
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int Index { get; set; }
    public required string Content { get; set; }
    public float[]? Embedding { get; set; }
    public DateTime Created { get; set; }

    // Not mapped, used for in-memory access
    //[NotMapped]
    //public ReadOnlyMemory<float>? Embedding
    //{
    //    get => MemoryHelper.ToReadOnlyMemory(EmbeddingAsBytes);
    //    set => EmbeddingAsBytes = MemoryHelper.ToByteArray(value);
    //}

    public Article? Article { get; set; }
}