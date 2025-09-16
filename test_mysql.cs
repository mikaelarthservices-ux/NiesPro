using Microsoft.EntityFrameworkCore;

namespace Test
{
    public class TestDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Test syntax for Pomelo MySQL
            optionsBuilder.UseMySql(
                "Server=localhost;Database=test;Uid=root;Pwd=;",
                ServerVersion.AutoDetect("Server=localhost;Database=test;Uid=root;Pwd=;")
            );
        }
    }
}