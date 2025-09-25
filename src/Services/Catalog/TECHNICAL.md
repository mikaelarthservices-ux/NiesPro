# Documentation Technique - Catalog.API

## 🏗️ Architecture détaillée

### Structure du projet
```
src/Services/Catalog/
├── Catalog.API/                    # Couche présentation
│   ├── Controllers/
│   │   └── V1/
│   │       ├── CategoriesController.cs
│   │       └── ProductsController.cs
│   ├── Middleware/                 # Middlewares personnalisés
│   ├── Program.cs                  # Point d'entrée + DI
│   └── appsettings.json           # Configuration
├── Catalog.Application/            # Logique métier
│   ├── Commands/                  # CQRS Commands
│   ├── Queries/                   # CQRS Queries  
│   ├── Handlers/                  # Command/Query Handlers
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Mappings/                  # AutoMapper profiles
│   ├── Validators/                # FluentValidation
│   └── Common/                    # Behaviours, Interfaces
├── Catalog.Domain/                # Entités métier
│   ├── Entities/                  # Agrégats DDD
│   ├── ValueObjects/              # Value Objects
│   ├── Events/                    # Domain Events
│   └── Interfaces/                # Contrats domaine
└── Catalog.Infrastructure/         # Accès données
    ├── Data/
    │   ├── CatalogDbContext.cs    # EF DbContext
    │   ├── Configurations/        # Entity configurations
    │   └── Migrations/            # EF Migrations
    ├── Repositories/              # Pattern Repository
    └── Services/                  # Services externes
```

### Patterns implémentés

#### 1. Clean Architecture
- **Séparation des responsabilités** claire
- **Inversion de dépendances** respectée
- **Testabilité** optimisée par injection de dépendances

#### 2. CQRS (Command Query Responsibility Segregation)
- **Commands** : Modification des données (Create, Update, Delete)
- **Queries** : Lecture des données (Get, List, Search)
- **MediatR** : Orchestration des messages

#### 3. Repository Pattern
- **Abstraction** de l'accès aux données
- **Testabilité** avec mocks
- **Cohérence** des opérations CRUD

#### 4. Domain-Driven Design (DDD)
- **Entités** avec identité métier
- **Value Objects** pour concepts sans identité
- **Domain Events** pour communication asynchrone

## 🗃️ Modèle de données

### Schéma relationnel

```sql
-- Table principale des catégories
CREATE TABLE categories (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    Slug VARCHAR(255) UNIQUE NOT NULL,
    ImageUrl VARCHAR(500),
    SortOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE,
    ParentCategoryId CHAR(36),
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (ParentCategoryId) REFERENCES categories(Id)
);

-- Table des marques
CREATE TABLE brands (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    LogoUrl VARCHAR(500),
    WebsiteUrl VARCHAR(500),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6)
);

-- Table principale des produits
CREATE TABLE products (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Sku VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL,
    ComparePrice DECIMAL(18,2),
    TrackQuantity BOOLEAN DEFAULT TRUE,
    Quantity INT DEFAULT 0,
    Weight DECIMAL(8,2),
    ImageUrl VARCHAR(500),
    IsActive BOOLEAN DEFAULT TRUE,
    IsFeatured BOOLEAN DEFAULT FALSE,
    PublishedAt DATETIME(6),
    CategoryId CHAR(36) NOT NULL,
    BrandId CHAR(36),
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    FOREIGN KEY (CategoryId) REFERENCES categories(Id),
    FOREIGN KEY (BrandId) REFERENCES brands(Id)
);

-- Table des variants de produits
CREATE TABLE productvariants (
    Id CHAR(36) PRIMARY KEY,
    ProductId CHAR(36) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Sku VARCHAR(100) UNIQUE NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT DEFAULT 0,
    ImageUrl VARCHAR(500),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES products(Id)
);

-- Table des attributs produits (couleur, taille, etc.)
CREATE TABLE productattributes (
    Id CHAR(36) PRIMARY KEY,
    ProductId CHAR(36) NOT NULL,
    AttributeName VARCHAR(100) NOT NULL,
    AttributeValue VARCHAR(255) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES products(Id)
);

-- Table des avis clients
CREATE TABLE reviews (
    Id CHAR(36) PRIMARY KEY,
    ProductId CHAR(36) NOT NULL,
    CustomerName VARCHAR(255) NOT NULL,
    CustomerEmail VARCHAR(255),
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Title VARCHAR(255),
    Comment TEXT,
    IsApproved BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME(6) NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES products(Id)
);
```

### Entités Domain

#### Product (Agrégat racine)
```csharp
public class Product : BaseEntity
{
    public string Name { get; private set; }
    public string Sku { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }        // Value Object
    public Money? ComparePrice { get; private set; } // Value Object
    public bool TrackQuantity { get; private set; }
    public int Quantity { get; private set; }
    public Weight? Weight { get; private set; }      // Value Object
    public string ImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // Navigation properties
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }
    public Guid? BrandId { get; private set; }
    public Brand Brand { get; private set; }
    
    // Collections
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyList<ProductAttribute> Attributes => _attributes.AsReadOnly();
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();
    
    // Business logic
    public void UpdatePrice(Money newPrice) { /* validation + event */ }
    public void AddVariant(ProductVariant variant) { /* validation */ }
    public void SetFeatured(bool featured) { /* business rules */ }
    public decimal GetAverageRating() { /* calculation */ }
    public bool IsInStock() { return !TrackQuantity || Quantity > 0; }
}
```

#### Category
```csharp
public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Slug Slug { get; private set; }           // Value Object
    public string ImageUrl { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    
    // Hiérarchie
    public Guid? ParentCategoryId { get; private set; }
    public Category ParentCategory { get; private set; }
    public IReadOnlyList<Category> SubCategories => _subCategories.AsReadOnly();
    
    // Navigation
    public IReadOnlyList<Product> Products => _products.AsReadOnly();
    
    // Business logic
    public bool HasSubCategories() { return _subCategories.Any(); }
    public int GetProductCount() { return _products.Count(p => p.IsActive); }
    public void AddSubCategory(Category subCategory) { /* validation */ }
}
```

### Value Objects

#### Money
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency = "EUR")
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        Amount = amount;
        Currency = currency ?? "EUR";
    }
    
    public static Money operator +(Money a, Money b) { /* validation + calculation */ }
    public static Money operator -(Money a, Money b) { /* validation + calculation */ }
    public static bool operator >(Money a, Money b) { /* comparison */ }
}
```

#### Slug
```csharp
public class Slug : ValueObject
{
    public string Value { get; }
    
    public Slug(string value)
    {
        Value = GenerateSlug(value);
    }
    
    private static string GenerateSlug(string input)
    {
        // Conversion en minuscules, suppression accents, remplacement espaces par tirets
        return input.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Trim('-');
    }
}
```

## 🔄 CQRS Implementation

### Commands (Write operations)

#### CreateProductCommand
```csharp
public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; }
    public int Stock { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Specifications { get; set; } = new();
}

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly ICatalogRepository _repository;
    private readonly IMapper _mapper;
    
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Validation métier
        await ValidateCategory(request.CategoryId);
        await ValidateSkuUniqueness(request.Sku);
        
        // 2. Création entité
        var product = Product.Create(
            request.Name,
            request.Description,
            new Money(request.Price),
            request.CategoryId,
            request.Sku);
            
        // 3. Ajout attributs
        foreach (var spec in request.Specifications)
        {
            product.AddAttribute(spec.Key, spec.Value);
        }
        
        // 4. Persistance
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        
        // 5. Mapping DTO
        return _mapper.Map<ProductDto>(product);
    }
}
```

### Queries (Read operations)

#### GetProductsQuery
```csharp
public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string SearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActive { get; set; } = true;
    public ProductSortBy SortBy { get; set; } = ProductSortBy.CreatedDate;
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;
}

public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        // 1. Construction requête avec filtres
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();
            
        // 2. Application filtres
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId);
            
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                                   p.Description.Contains(request.SearchTerm));
                                   
        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);
            
        // 3. Tri
        query = request.SortBy switch
        {
            ProductSortBy.Name => request.SortOrder == SortOrder.Ascending 
                ? query.OrderBy(p => p.Name) 
                : query.OrderByDescending(p => p.Name),
            ProductSortBy.Price => request.SortOrder == SortOrder.Ascending 
                ? query.OrderBy(p => p.Price) 
                : query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
        
        // 4. Pagination
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category.Name,
                BrandName = p.Brand.Name,
                IsInStock = !p.TrackQuantity || p.Quantity > 0
            })
            .ToListAsync(ct);
            
        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
```

## 📊 Performance et optimisations

### Indexation base de données
```sql
-- Index sur recherche produits
CREATE INDEX IX_Products_Search ON products(Name, Description);
CREATE INDEX IX_Products_Category ON products(CategoryId, IsActive);
CREATE INDEX IX_Products_Price ON products(Price);
CREATE INDEX IX_Products_Sku ON products(Sku);

-- Index sur catégories
CREATE INDEX IX_Categories_Slug ON categories(Slug);
CREATE INDEX IX_Categories_Parent ON categories(ParentCategoryId);
CREATE INDEX IX_Categories_Active ON categories(IsActive, SortOrder);
```

### Caching strategy (à implémenter)
```csharp
public class CachedProductService
{
    private readonly IMemoryCache _cache;
    private readonly IProductService _productService;
    
    public async Task<ProductDto> GetProductAsync(Guid id)
    {
        var cacheKey = $"product_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out ProductDto product))
        {
            product = await _productService.GetByIdAsync(id);
            _cache.Set(cacheKey, product, TimeSpan.FromMinutes(15));
        }
        
        return product;
    }
}
```

### Optimisations requêtes
- **Projection** directe en DTO pour éviter le mapping lourd
- **Include** sélectif des relations nécessaires
- **Pagination** avec Skip/Take optimisé
- **Filtres** appliqués avant le COUNT pour performance

## 🔐 Sécurité

### Validation stricte
```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");
            
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(1000000).WithMessage("Price must be realistic");
            
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .Matches(@"^[A-Z0-9]+$").WithMessage("SKU must contain only letters and numbers");
            
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required")
            .MustAsync(CategoryExists).WithMessage("Category does not exist");
    }
}
```

### Sanitisation
- **HTML Encoding** pour descriptions
- **SQL Injection** protection via EF parameterization
- **XSS Protection** sur les champs texte

## 🧪 Tests recommandés

### Tests unitaires
```csharp
[Test]
public async Task CreateProduct_WithValidData_ShouldReturnProductDto()
{
    // Arrange
    var command = new CreateProductCommand 
    { 
        Name = "Test Product",
        Price = 99.99m,
        CategoryId = Guid.NewGuid(),
        Sku = "TEST001"
    };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Test Product");
    result.Price.Should().Be(99.99m);
}
```

### Tests d'intégration
```csharp
[Test]
public async Task GetProducts_WithFilters_ShouldReturnFilteredResults()
{
    // Arrange
    await SeedTestData();
    
    // Act
    var response = await _client.GetAsync("/api/v1/Products?categoryId=xxx&minPrice=50");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductDto>>>(content);
    result.Data.Items.Should().HaveCount(2);
}
```

---

**Documentation Technique Catalog.API** - Architecture de référence 🏗️📚