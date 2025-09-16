using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Data
{
    /// <summary>
    /// Catalog database context
    /// </summary>
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
                entity.HasIndex(p => p.SKU).IsUnique();
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.LongDescription).HasMaxLength(2000);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.ComparePrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Weight).HasColumnType("decimal(10,3)");
                entity.Property(p => p.ImageUrl).HasMaxLength(500);
                entity.Property(p => p.ImageGallery).HasMaxLength(2000);

                // Relationships
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Global query filter for soft delete
                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.Property(c => c.Slug).IsRequired().HasMaxLength(200);
                entity.HasIndex(c => c.Slug).IsUnique();
                entity.Property(c => c.ImageUrl).HasMaxLength(300);

                // Self-referencing relationship
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(c => c.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(c => !c.IsDeleted);
            });

            // Brand configuration
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.Description).HasMaxLength(500);
                entity.Property(b => b.Slug).IsRequired().HasMaxLength(200);
                entity.HasIndex(b => b.Slug).IsUnique();
                entity.Property(b => b.LogoUrl).HasMaxLength(300);
                entity.Property(b => b.Website).HasMaxLength(200);

                entity.HasQueryFilter(b => !b.IsDeleted);
            });

            // ProductVariant configuration
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(pv => pv.Id);
                entity.Property(pv => pv.Name).IsRequired().HasMaxLength(100);
                entity.Property(pv => pv.SKU).IsRequired().HasMaxLength(50);
                entity.HasIndex(pv => pv.SKU).IsUnique();
                entity.Property(pv => pv.PriceAdjustment).HasColumnType("decimal(18,2)");
                entity.Property(pv => pv.ImageUrl).HasMaxLength(500);

                entity.HasOne(pv => pv.Product)
                      .WithMany(p => p.Variants)
                      .HasForeignKey(pv => pv.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(pv => !pv.IsDeleted);
            });

            // ProductAttribute configuration
            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.HasKey(pa => pa.Id);
                entity.Property(pa => pa.Name).IsRequired().HasMaxLength(100);
                entity.Property(pa => pa.Value).IsRequired().HasMaxLength(500);

                entity.HasOne(pa => pa.Product)
                      .WithMany(p => p.Attributes)
                      .HasForeignKey(pa => pa.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(pa => !pa.IsDeleted);
            });

            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(r => r.CustomerEmail).HasMaxLength(191); // Max pour index MySQL utf8mb4
                entity.Property(r => r.Title).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Comment).HasMaxLength(2000);
                entity.Property(r => r.ApprovedBy).HasMaxLength(100);

                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(r => !r.IsDeleted);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            var electronicsCategory = new Category
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                Slug = "electronics",
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var clothingCategory = new Category
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Clothing",
                Description = "Fashion and apparel",
                Slug = "clothing",
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<Category>().HasData(electronicsCategory, clothingCategory);

            // Seed Brands
            var appleBrand = new Brand
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Apple",
                Description = "Think different",
                Slug = "apple",
                Website = "https://www.apple.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var nikeBrand = new Brand
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Nike",
                Description = "Just do it",
                Slug = "nike",
                Website = "https://www.nike.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<Brand>().HasData(appleBrand, nikeBrand);
        }
    }
}