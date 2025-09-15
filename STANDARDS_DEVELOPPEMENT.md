# Standards de développement - NiesPro ERP

## 🎯 Principes généraux

### Philosophie de développement
- **Clean Code** : Code lisible, maintenable et auto-documenté
- **SOLID Principles** : Respect des 5 principes SOLID
- **DRY (Don't Repeat Yourself)** : Éviter la duplication de code
- **YAGNI (You Aren't Gonna Need It)** : Ne pas sur-ingéniérer
- **Boy Scout Rule** : Laisser le code plus propre qu'on l'a trouvé

### Standards de qualité
- **Couverture de tests** : Minimum 80% pour le code critique
- **Complexité cyclomatique** : Maximum 10 par méthode
- **Taille des méthodes** : Maximum 50 lignes
- **Taille des classes** : Maximum 500 lignes
- **Dépendances** : Injection de dépendance obligatoire

## 📝 Conventions de codage C#

### Naming conventions

#### Classes et interfaces
```csharp
// ✅ Correct
public class ProductService { }
public interface IProductRepository { }
public abstract class BaseEntity { }
public enum OrderStatus { }

// ❌ Incorrect
public class productService { }
public interface ProductRepository { }
public class baseEntity { }
```

#### Méthodes et propriétés
```csharp
// ✅ Correct
public async Task<Product> GetProductByIdAsync(int productId)
{
    var product = await _repository.FindAsync(productId);
    return product;
}

public string ProductName { get; set; }
public bool IsActive { get; private set; }

// ❌ Incorrect
public async Task<Product> getProduct(int id) { }
public string productName { get; set; }
```

#### Variables et paramètres
```csharp
// ✅ Correct
public void ProcessOrder(Order customerOrder)
{
    var totalAmount = customerOrder.Items.Sum(i => i.Price);
    var discountAmount = CalculateDiscount(totalAmount);
    const int MAX_RETRY_COUNT = 3;
}

// ❌ Incorrect  
public void ProcessOrder(Order o)
{
    var total = o.Items.Sum(i => i.Price);
    var discount = CalculateDiscount(total);
    int maxRetry = 3;
}
```

### Structure des fichiers

#### Organisation des usings
```csharp
// 1. System namespaces
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 2. Microsoft namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// 3. Third-party namespaces
using AutoMapper;
using FluentValidation;

// 4. Application namespaces
using NiesPro.Common.Models;
using NiesPro.Services.Product.Domain;

namespace NiesPro.Services.Product.API.Controllers
{
    // Code...
}
```

#### Structure de classe
```csharp
namespace NiesPro.Services.Product.Domain.Entities
{
    /// <summary>
    /// Represents a product in the catalog
    /// </summary>
    public class Product : BaseEntity
    {
        #region Constants
        public const int MAX_NAME_LENGTH = 200;
        #endregion

        #region Properties
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; }
        #endregion

        #region Constructors
        private Product() { } // EF Constructor

        public Product(string name, string description, decimal price)
        {
            ValidateParameters(name, description, price);
            
            Name = name;
            Description = description;
            Price = price;
            IsActive = true;
        }
        #endregion

        #region Public Methods
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("Price must be positive", nameof(newPrice));
                
            Price = newPrice;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
        #endregion

        #region Private Methods
        private void ValidateParameters(string name, string description, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
                
            if (name.Length > MAX_NAME_LENGTH)
                throw new ArgumentException($"Name cannot exceed {MAX_NAME_LENGTH} characters", nameof(name));
                
            if (price <= 0)
                throw new ArgumentException("Price must be positive", nameof(price));
        }
        #endregion
    }
}
```

## 🏗️ Patterns architecturaux

### Repository Pattern
```csharp
// Interface
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

// Implémentation
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;
    
    public ProductRepository(ProductDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    // Autres méthodes...
}
```

### CQRS Pattern avec MediatR
```csharp
// Command
public class CreateProductCommand : IRequest<CreateProductResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Command Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    
    public CreateProductCommandHandler(
        IProductRepository repository,
        IMapper mapper,
        ILogger<CreateProductCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with name: {ProductName}", request.Name);
        
        var product = new Product(request.Name, request.Description, request.Price);
        var createdProduct = await _repository.AddAsync(product, cancellationToken);
        
        _logger.LogInformation("Product created with ID: {ProductId}", createdProduct.Id);
        
        return _mapper.Map<CreateProductResponse>(createdProduct);
    }
}

// Query
public class GetProductQuery : IRequest<ProductDto>
{
    public int ProductId { get; set; }
}

// Query Handler
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    
    public GetProductQueryHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        
        if (product == null)
            throw new NotFoundException($"Product with ID {request.ProductId} not found");
            
        return _mapper.Map<ProductDto>(product);
    }
}
```

### Controller Standards
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;
    
    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductDto>> GetProduct(
        int id, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetProductQuery { ProductId = id };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(
                nameof(GetProduct), 
                new { id = result.Id }, 
                result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
```

## ✅ Validation et gestion d'erreurs

### FluentValidation
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");
            
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
            
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(1000000).WithMessage("Price cannot exceed 1,000,000");
    }
}
```

### Custom Exceptions
```csharp
public class BusinessException : Exception
{
    public string Code { get; }
    
    public BusinessException(string code, string message) : base(message)
    {
        Code = code;
    }
    
    public BusinessException(string code, string message, Exception innerException) 
        : base(message, innerException)
    {
        Code = code;
    }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base("NOT_FOUND", message) { }
}

public class ValidationException : BusinessException
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException(IDictionary<string, string[]> errors) 
        : base("VALIDATION_FAILED", "One or more validation errors occurred")
    {
        Errors = errors;
    }
}
```

### Global Exception Handler
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {ErrorMessage}", exception.Message);
        
        var response = context.Response;
        response.ContentType = "application/json";
        
        var errorResponse = new ErrorResponse();
        
        switch (exception)
        {
            case NotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                errorResponse.Message = exception.Message;
                break;
                
            case ValidationException validationEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Message = "Validation failed";
                errorResponse.Errors = validationEx.Errors;
                break;
                
            case BusinessException businessEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Message = businessEx.Message;
                errorResponse.Code = businessEx.Code;
                break;
                
            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Message = "An internal server error occurred";
                break;
        }
        
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(jsonResponse);
    }
}
```

## 📝 Logging et monitoring

### Logging structuré avec Serilog
```csharp
// Configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Product.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "niespro-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

// Usage dans les services
public class ProductService
{
    private readonly ILogger<ProductService> _logger;
    
    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductName"] = request.Name,
            ["UserId"] = _currentUser.Id,
            ["CorrelationId"] = Guid.NewGuid()
        });
        
        _logger.LogInformation("Creating product {ProductName} for user {UserId}", 
            request.Name, _currentUser.Id);
        
        try
        {
            var product = new Product(request.Name, request.Description, request.Price);
            var result = await _repository.AddAsync(product);
            
            _logger.LogInformation("Product {ProductId} created successfully", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product {ProductName}", request.Name);
            throw;
        }
    }
}
```

## 🧪 Standards de tests

### Tests unitaires
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut;
    
    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _sut = new ProductService(_repositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateProductAsync_WithValidData_ShouldReturnProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.99m
        };
        
        var expectedProduct = new Product(request.Name, request.Description, request.Price)
        {
            Id = 1
        };
        
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(expectedProduct);
        
        // Act
        var result = await _sut.CreateProductAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Price.Should().Be(request.Price);
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }
    
    [Theory]
    [InlineData("", "Description", 10.99)]
    [InlineData(null, "Description", 10.99)]
    [InlineData("Name", "Description", 0)]
    [InlineData("Name", "Description", -1)]
    public async Task CreateProductAsync_WithInvalidData_ShouldThrowException(
        string name, string description, decimal price)
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = name,
            Description = description,
            Price = price
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateProductAsync(request));
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}
```

### Tests d'intégration
```csharp
public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = new CreateProductCommand
        {
            Name = "Integration Test Product",
            Description = "Test Description",
            Price = 19.99m
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/products", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateProductResponse>(responseContent);
        
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(request.Name);
    }
}
```

## 🔄 Processus de développement

### Git Flow
1. **Feature branches** : `feature/product-management`
2. **Bug fix branches** : `bugfix/fix-price-calculation`
3. **Release branches** : `release/v1.2.0`
4. **Hotfix branches** : `hotfix/critical-security-fix`

### Commit messages
```
feat(product): add product creation functionality

- Implement CreateProductCommand and handler
- Add validation for product data
- Include unit tests for happy path
- Update API documentation

Closes #123
```

### Pull Request process
1. **Branch à jour** avec develop/main
2. **Tests passent** (unitaires + intégration)
3. **Code review** par 2 développeurs minimum
4. **Documentation** mise à jour si nécessaire
5. **Merge** après approbation

### Code Review checklist
- [ ] Code respecte les conventions de nommage
- [ ] Logique métier est correcte
- [ ] Tests unitaires couvrent les cas critiques
- [ ] Gestion d'erreurs appropriée
- [ ] Performance acceptable
- [ ] Sécurité respectée
- [ ] Documentation à jour

---

**Standards approuvés le :** [Date]  
**Tech Lead :** [Nom]  
**Équipe de développement :** [Noms]

Ces standards doivent être respectés par tous les développeurs et seront régulièrement révisés pour s'adapter aux évolutions du projet.
