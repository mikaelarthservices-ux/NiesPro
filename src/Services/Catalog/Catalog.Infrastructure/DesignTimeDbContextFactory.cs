using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure;

/// <summary>
/// Design-time DbContext factory for EF migrations
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        
        // Configuration MySQL pour migrations
        optionsBuilder.UseMySql(
            "Server=localhost;Port=3306;Database=NiesPro_Catalog;Uid=root;Pwd=;",
            new MySqlServerVersion(new Version(8, 0, 21)));

        return new CatalogDbContext(optionsBuilder.Options);
    }
}