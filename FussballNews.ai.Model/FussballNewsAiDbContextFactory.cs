using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FussballNews.ai.Model;

public class FussballNewsAiDbContextFactory : IDesignTimeDbContextFactory<FussballNewsAiDbContext>
{
    public FussballNewsAiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FussballNewsAiDbContext>();
        // Update the connection string as needed
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=fussballnews_ai_localdev;User Id=sa;Password=T0mteF4r!;TrustServerCertificate=True",
            o => o.UseVectorSearch()
        );
        return new FussballNewsAiDbContext(optionsBuilder.Options);
    }
} 