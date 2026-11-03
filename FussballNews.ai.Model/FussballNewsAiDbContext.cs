using Microsoft.EntityFrameworkCore;
using FussballNews.ai.Model.Models;

namespace FussballNews.ai.Model;

public class FussballNewsAiDbContext : DbContext
{
    public FussballNewsAiDbContext(DbContextOptions<FussballNewsAiDbContext> options) : base(options) { }

    public DbSet<Article> Articles { get; set; }
    public DbSet<Source> Sources { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ArticleVector> ArticleVectors { get; set; }
    public DbSet<RssFeed> RssFeeds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Markdown);
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.Language);
            entity.Property(e => e.Created).IsRequired();

            entity.HasOne(e => e.Source)
                  .WithMany()
                  .HasForeignKey(e => e.SourceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Tags)
                  .WithMany();

            entity.HasMany(e => e.ArticleVectors)
                  .WithOne(e => e.Article)
                  .HasForeignKey(e => e.ArticleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Source>(entity => 
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.ParserType).IsRequired();
            entity.Property(e => e.Language).IsRequired();

            entity.HasMany(e => e.RssFeeds)
                  .WithOne(e => e.Source)
                  .HasForeignKey(e => e.SourceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Created).IsRequired();
        });

        modelBuilder.Entity<ArticleVector>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.Index).IsRequired();
            entity.Property(e => e.ArticleId).IsRequired();
            entity.Property(p => p.Embedding).HasColumnType("vector(1536)");

            entity.HasOne(e => e.Article)
                  .WithMany(e => e.ArticleVectors)
                  .HasForeignKey(e => e.ArticleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RssFeed>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.LastFetchTriggered);
            entity.Property(e => e.LastFetchedCompleted);

            entity.HasOne(e => e.Source)
                  .WithMany(s => s.RssFeeds)
                  .HasForeignKey(e => e.SourceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 