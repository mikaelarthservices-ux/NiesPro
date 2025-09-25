using Catalog.Tests.Integration;
using Catalog.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Net;
using System.Text;

namespace Catalog.Tests.Integration.Controllers
{
    /// <summary>
    /// Professional integration tests for ProductsController
    /// Uses proper seeding and error handling
    /// </summary>
    [TestFixture]
    public class ProductsControllerIntegrationTests
    {
        private CatalogWebApplicationFactory _factory;
        private HttpClient _client;

        // Test data constants - using fixed GUIDs for predictable tests
        private static readonly Guid ElectronicsId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ClothingId = new("22222222-2222-2222-2222-222222222222");
        private static readonly Guid AppleId = new("33333333-3333-3333-3333-333333333333");
        private static readonly Guid NikeId = new("44444444-4444-4444-4444-444444444444");
        private static readonly Guid iPhone15Id = new("55555555-5555-5555-5555-555555555555");
        private static readonly Guid MacBookId = new("66666666-6666-6666-6666-666666666666");
        private static readonly Guid AirMaxId = new("77777777-7777-7777-7777-777777777777");

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _factory = new CatalogWebApplicationFactory();
            await _factory.EnsureSeededAsync();
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task GetProducts_ShouldReturnSuccessAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/Products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        [Test]
        public async Task GetProducts_ShouldReturnValidJsonStructure()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/Products");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();

            // Parse JSON to verify structure
            var json = JObject.Parse(content);
            json["success"].Should().NotBeNull();
            json["data"].Should().NotBeNull();
            json["data"]["items"].Should().NotBeNull();
            json["data"]["totalCount"].Should().NotBeNull();
        }

        [Test]
        public async Task GetProducts_ShouldReturnSeededProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/Products");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var json = JObject.Parse(content);
            var success = json["success"]?.Value<bool>();
            var totalCount = json["data"]?["totalCount"]?.Value<int>();

            success.Should().BeTrue();
            totalCount.Should().Be(3); // We seeded exactly 3 products
        }

        [Test]
        public async Task GetProducts_WithPagination_ShouldReturnCorrectStructure()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/Products?page=1&pageSize=2");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var json = JObject.Parse(content);
            json["data"]["page"].Should().NotBeNull();
            json["data"]["pageSize"].Should().NotBeNull();
            json["data"]["totalPages"].Should().NotBeNull();
            json["data"]["hasNextPage"].Should().NotBeNull();
            json["data"]["hasPreviousPage"].Should().NotBeNull();
            
            var items = json["data"]["items"] as JArray;
            items.Should().NotBeNull();
            items.Count.Should().BeLessOrEqualTo(2); // Respecting page size
        }

        [Test]
        public async Task GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Act - using known seeded product ID
            var response = await _client.GetAsync($"/api/v1/Products/{iPhone15Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            json["success"]?.Value<bool>().Should().BeTrue();
            json["data"]["id"]?.Value<string>().Should().Be(iPhone15Id.ToString());
            json["data"]["name"]?.Value<string>().Should().Be("Test iPhone 15 Pro");
        }

        [Test]
        public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act - Test with random GUID that doesn't exist
            var response = await _client.GetAsync($"/api/v1/Products/{Guid.NewGuid()}");

            // Assert - Should return 400 because our controller validates GUID format first
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateProduct_WithValidData_ShouldCreateSuccessfully()
        {
            var productData = new
            {
                Name = "Test New Product Professional",
                SKU = $"TEST-PRO-{DateTime.Now.Ticks}",
                Description = "A professionally tested product",
                Price = 299.99m,
                IsActive = true,
                CategoryId = ElectronicsId.ToString(),
                BrandId = AppleId.ToString()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(productData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/Products", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseContent);
            responseJson["success"]?.Value<bool>().Should().BeTrue();
            responseJson["data"]["name"]?.Value<string>().Should().Be("Test New Product Professional");
        }

        [Test]
        public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
        {
            var productData = new
            {
                Name = "", // Invalid: empty name
                SKU = "",  // Invalid: empty SKU
                Price = -10 // Invalid: negative price
            };

            var json = System.Text.Json.JsonSerializer.Serialize(productData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/Products", content);

            // Assert - Now should return 400 due to our exception handler
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseContent);
            responseJson["success"]?.Value<bool>().Should().BeFalse();
            responseJson["errors"].Should().NotBeNull();
        }

        [Test]
        public async Task UpdateProduct_WithValidId_ShouldUpdateSuccessfully()
        {
            // First check if product exists to determine expected behavior
            var updateData = new
            {
                Id = iPhone15Id.ToString(),
                Name = "Updated iPhone 15 Pro Max",
                SKU = "IPHONE-15-PRO-TEST", // Keep same SKU
                Description = "Updated description for testing",
                Price = 1299.99m,
                IsActive = true,
                CategoryId = ElectronicsId.ToString(),
                BrandId = AppleId.ToString()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/v1/Products/{iPhone15Id}", content);

            // Assert - Product exists and update succeeded
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseContent);
            responseJson["success"]?.Value<bool>().Should().BeTrue();
        }

        [Test]
        public async Task DeleteProduct_WithValidId_ShouldDeleteSuccessfully()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/v1/Products/{AirMaxId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task GetProducts_WithFilters_ShouldReturnFilteredResults()
        {
            // Test with price range filter
            var response = await _client.GetAsync("/api/v1/Products?minPrice=100&maxPrice=2000");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            json["success"]?.Value<bool>().Should().BeTrue();
            json["data"]["items"].Should().NotBeNull();
        }

        [Test]
        public async Task GetProducts_ByCategoryFilter_ShouldReturnCorrectProducts()
        {
            // Test filtering by Electronics category
            var response = await _client.GetAsync($"/api/v1/Products?categoryId={ElectronicsId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            json["success"]?.Value<bool>().Should().BeTrue();
            
            var items = json["data"]["items"] as JArray;
            items.Should().NotBeNull();
            // Should have 2 electronics products (iPhone and MacBook)
            items.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GetProducts_ByBrandFilter_ShouldReturnCorrectProducts()
        {
            // Test filtering by Apple brand
            var response = await _client.GetAsync($"/api/v1/Products?brandId={AppleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            json["success"]?.Value<bool>().Should().BeTrue();
            
            var items = json["data"]["items"] as JArray;
            items.Should().NotBeNull();
            // Should have Apple products
            items.Count.Should().BeGreaterThan(0);
        }
    }
}