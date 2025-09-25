using Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Catalog.Tests.Integration
{
    /// <summary>
    /// Custom WebApplicationFactory for integration tests
    /// Professional implementation with proper seeding and error handling
    /// </summary>
    public class CatalogWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;
        private bool _isSeeded = false;
        private readonly object _seedLock = new object();
        
        public CatalogWebApplicationFactory()
        {
            _databaseName = $"CatalogTestDb_{Guid.NewGuid():N}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CatalogDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database with unique name per factory instance
                services.AddDbContext<CatalogDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName: _databaseName);
                    options.EnableSensitiveDataLogging();
                });
            });

            // Configure test environment only
            builder.UseEnvironment("Test");
        }

        /// <summary>
        /// Ensure test data is seeded before tests run
        /// </summary>
        public async Task EnsureSeededAsync()
        {
            if (_isSeeded) return;
            
            lock (_seedLock)
            {
                if (_isSeeded) return;
                
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
                
                SeedTestDataSync(context);
                _isSeeded = true;
            }
        }

        /// <summary>
        /// Get a fresh database context for testing
        /// </summary>
        public CatalogDbContext GetDbContext()
        {
            using var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        }

        private static void SeedTestDataSync(CatalogDbContext context)
        {
            try
            {
                context.Database.EnsureCreated();
                
                // Clear existing data to ensure fresh start
                context.Products.RemoveRange(context.Products);
                context.Categories.RemoveRange(context.Categories);
                context.Brands.RemoveRange(context.Brands);
                context.SaveChanges();

                // Seed test categories
                var electronicsCategory = new Domain.Entities.Category
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                    Slug = "electronics",
                    IsActive = true,
                    SortOrder = 1,
                    ParentCategoryId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var clothingCategory = new Domain.Entities.Category
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    Name = "Clothing",
                    Description = "Fashion and apparel",
                    Slug = "clothing",
                    IsActive = true,
                    SortOrder = 2,
                    ParentCategoryId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Categories.AddRange(electronicsCategory, clothingCategory);

                // Seed test brands
                var appleBrand = new Domain.Entities.Brand
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    Name = "Apple",
                    Description = "Apple Inc. Technology Company",
                    Slug = "apple",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var nikeBrand = new Domain.Entities.Brand
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    Name = "Nike",
                    Description = "Nike Sports Brand",
                    Slug = "nike",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Brands.AddRange(appleBrand, nikeBrand);
                context.SaveChanges();

                // Seed test products
                var testProduct1 = new Domain.Entities.Product
                {
                    Id = new Guid("55555555-5555-5555-5555-555555555555"),
                    Name = "Test iPhone 15 Pro",
                    SKU = "IPHONE-15-PRO-TEST",
                    Description = "Test iPhone 15 Pro for integration testing",
                    Price = 1199.99m,
                    IsActive = true,
                    CategoryId = electronicsCategory.Id,
                    BrandId = appleBrand.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var testProduct2 = new Domain.Entities.Product
                {
                    Id = new Guid("66666666-6666-6666-6666-666666666666"),
                    Name = "Test MacBook Pro M3",
                    SKU = "MACBOOK-M3-TEST",
                    Description = "Test MacBook Pro M3 for integration testing",
                    Price = 2399.99m,
                    IsActive = true,
                    CategoryId = electronicsCategory.Id,
                    BrandId = appleBrand.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var testProduct3 = new Domain.Entities.Product
                {
                    Id = new Guid("77777777-7777-7777-7777-777777777777"),
                    Name = "Test Nike Air Max 2024",
                    SKU = "NIKE-AIRMAX-2024-TEST",
                    Description = "Test Nike Air Max 2024 for integration testing",
                    Price = 159.99m,
                    IsActive = true,
                    CategoryId = clothingCategory.Id,
                    BrandId = nikeBrand.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Products.AddRange(testProduct1, testProduct2, testProduct3);
                context.SaveChanges();
                
                Console.WriteLine($"✅ Test data seeded successfully: {context.Products.Count()} products, {context.Categories.Count()} categories, {context.Brands.Count()} brands");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test data seeding failed: {ex.Message}");
                throw; // Re-throw to make test failures obvious
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    using var scope = Services.CreateScope();
                    var context = scope.ServiceProvider.GetService<CatalogDbContext>();
                    context?.Database.EnsureDeleted();
                }
                catch
                {
                    // Ignore errors during disposal
                }
            }
            
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Professional middleware to handle validation exceptions in tests
    /// Converts ValidationException to proper HTTP 400 responses
    /// </summary>
    public class TestExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TestExceptionHandlerMiddleware> _logger;

        public TestExceptionHandlerMiddleware(RequestDelegate next, ILogger<TestExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationEx)
            {
                _logger.LogWarning("Validation error: {ValidationErrors}", 
                    string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage)));
                
                await HandleValidationExceptionAsync(context, validationEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in test middleware");
                throw; // Let other exceptions bubble up for debugging
            }
        }

        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = "Validation failed",
                errors = exception.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                })
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}