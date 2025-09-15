# Architecture technique détaillée - NiesPro ERP

## 🏗️ Vue d'ensemble architecturale

### Principes architecturaux
- **Microservices** : Services autonomes et faiblement couplés
- **Domain-Driven Design (DDD)** : Organisation par domaines métier
- **CQRS + Event Sourcing** : Séparation lecture/écriture avec événements
- **API-First** : APIs REST comme contrats principaux
- **Cloud-Native** : Conçu pour le cloud avec containers
- **Security by Design** : Sécurité intégrée dès la conception

### Stack technologique complète

#### Backend Services (.NET 6+)
```
├── ASP.NET Core 6.0+ (Web APIs)
├── Entity Framework Core 6.0+ (ORM)
├── MediatR (CQRS pattern)
├── FluentValidation (Validation)
├── AutoMapper (Object mapping)
├── Serilog (Logging structuré)
├── Polly (Resilience patterns)
└── Carter (Minimal APIs)
```

#### Frontend Applications
```
├── WPF (.NET 6) - Desktop POS
├── .NET MAUI - Mobile/Tablet
├── Blazor Server - Web Admin
└── Material Design Components
```

#### Infrastructure
```
├── Docker + Kubernetes
├── NGINX (Reverse proxy)
├── MySQL 8.0 (Primary DB)
├── Redis (Cache + Sessions)
├── RabbitMQ (Message broker)
├── Elasticsearch (Logs + Search)
└── Grafana + Prometheus (Monitoring
```

## 🎯 Architecture microservices

### Services de domaine

#### 1. Auth Service (Authentification/Autorisation)
```csharp
// Port: 5001
// Database: AuthDB
// Responsabilités:
- Authentification JWT + Device Keys
- Gestion des rôles et permissions (RBAC)
- Validation des tokens
- Audit des connexions
- Sessions utilisateurs

// APIs principales:
POST /auth/login
POST /auth/refresh-token
POST /auth/device/register
GET  /auth/permissions/{userId}
POST /auth/logout
```

#### 2. Product Service (Catalogue produits)
```csharp
// Port: 5002  
// Database: ProductDB
// Responsabilités:
- CRUD produits avec variantes
- Gestion codes-barres/QR codes
- Catégories et tags
- Prix et promotions
- Import/Export catalogue

// APIs principales:
GET    /products
POST   /products
PUT    /products/{id}
DELETE /products/{id}
GET    /products/search?q={query}
POST   /products/import
GET    /products/{id}/barcode
```

#### 3. Stock Service (Gestion inventaire)
```csharp
// Port: 5003
// Database: StockDB  
// Responsabilités:
- Mouvements de stock (entrées/sorties)
- Inventaires physiques
- Alertes de rupture
- Valorisation (FIFO/LIFO/CMP)
- Réservations

// APIs principales:
GET  /stock/products/{productId}
POST /stock/movements
POST /stock/inventory
GET  /stock/alerts
PUT  /stock/reserve/{productId}
GET  /stock/valuation
```

#### 4. Order Service (Commandes)
```csharp
// Port: 5004
// Database: OrderDB
// Responsabilités:
- Commandes boutique/restaurant  
- Calculs prix/taxes/remises
- Statuts et workflow
- Facturation
- Annulations/retours

// APIs principales:
POST /orders
GET  /orders/{id}
PUT  /orders/{id}/status
POST /orders/{id}/cancel
GET  /orders/restaurant/table/{tableId}
POST /orders/{id}/invoice
```

#### 5. Payment Service (Paiements)
```csharp
// Port: 5005
// Database: PaymentDB
// Responsabilités:
- Multi-moyens de paiement
- Intégrations TPE
- Multi-devises
- Avoirs et remboursements
- Conformité PCI DSS

// APIs principales:
POST /payments/process
GET  /payments/{transactionId}
POST /payments/refund
GET  /payments/methods
POST /payments/currencies/rates
```

#### 6. Customer Service (Clients)
```csharp
// Port: 5006
// Database: CustomerDB  
// Responsabilités:
- Gestion clients RGPD
- Programme fidélité
- Historique achats
- Préférences
- Segmentation

// APIs principales:
POST /customers
GET  /customers/{id}
PUT  /customers/{id}
GET  /customers/{id}/history
POST /customers/{id}/loyalty-points
GET  /customers/segments
```

#### 7. Restaurant Service (Spécificités restaurant)
```csharp
// Port: 5007
// Database: RestaurantDB
// Responsabilités:
- Menus et cartes
- Tables et plans de salle
- Modifications plats
- Temps de préparation
- Livraison/à emporter

// APIs principales:
GET  /restaurant/menus
POST /restaurant/menus
GET  /restaurant/tables
PUT  /restaurant/tables/{id}/status
GET  /restaurant/orders/kitchen
POST /restaurant/orders/{id}/ready
```

### Services transversaux

#### 8. Notification Service
```csharp
// Port: 5008
// Responsabilités:
- Notifications temps réel (SignalR)
- SMS/WhatsApp marketing
- Emails transactionnels
- Push notifications mobiles
- Templates configurables

// APIs principales:
POST /notifications/send
POST /notifications/sms
POST /notifications/email
GET  /notifications/templates
POST /notifications/broadcast
```

#### 9. File Service (Gestion fichiers)
```csharp
// Port: 5009
// Responsabilités:
- Stockage sécurisé fichiers
- Images produits
- Factures PDF
- Documents légaux
- Versioning

// APIs principales:
POST /files/upload
GET  /files/{id}
DELETE /files/{id}
GET  /files/{id}/versions
POST /files/generate-pdf
```

#### 10. Report Service (Reporting)
```csharp
// Port: 5010
// Database: ReportDB (read-only replicas)
// Responsabilités:
- Dashboards temps réel
- Rapports périodiques
- KPIs métier
- Exports Excel/PDF
- Analytics prédictives

// APIs principales:
GET  /reports/dashboard
GET  /reports/sales/{period}
GET  /reports/stock/movements
POST /reports/custom/generate
GET  /reports/kpis/realtime
```

#### 11. Log Service (Audit et logs)
```csharp
// Port: 5011
// Database: LogDB + Elasticsearch
// Responsabilités:
- Logs applicatifs centralisés
- Audit trail sécurisé
- Métriques performance
- Alertes anomalies
- Conformité réglementaire

// APIs principales:
POST /logs/write
GET  /logs/search
GET  /logs/audit/{userId}
GET  /logs/performance/metrics
POST /logs/alerts/configure
```

## 🗄️ Architecture base de données

### Stratégie de données
- **Database per Service** : Chaque microservice a sa propre DB
- **MySQL 8.0** : Base principale pour données transactionnelles
- **Redis** : Cache distribué et sessions
- **Elasticsearch** : Logs et recherche full-text
- **Read Replicas** : Pour le reporting et analytics

### Schéma principal (MySQL)

#### AuthDB
```sql
-- Tables Auth Service
Users (Id, Username, PasswordHash, Email, IsActive, CreatedAt)
Roles (Id, Name, Description, Permissions)
UserRoles (UserId, RoleId)
Devices (Id, DeviceKey, UserId, Name, LastLogin)
Sessions (Id, UserId, DeviceId, Token, ExpiresAt)
AuditLogs (Id, UserId, Action, EntityType, EntityId, Timestamp)
```

#### ProductDB  
```sql
-- Tables Product Service
Products (Id, Name, Description, SKU, CategoryId, IsActive)
ProductVariants (Id, ProductId, Name, SKU, Price, Stock)
Categories (Id, Name, ParentId, SortOrder)
Barcodes (Id, ProductVariantId, Code, Type)
PriceRules (Id, ProductId, StartDate, EndDate, DiscountPercent)
```

#### OrderDB
```sql
-- Tables Order Service  
Orders (Id, CustomerId, Type, Status, TotalAmount, CreatedAt)
OrderItems (Id, OrderId, ProductVariantId, Quantity, UnitPrice)
Invoices (Id, OrderId, InvoiceNumber, Amount, IssuedAt)
Payments (Id, OrderId, Method, Amount, TransactionId, Status)
Returns (Id, OrderId, Reason, Amount, ProcessedAt)
```

### Patterns de données

#### CQRS (Command Query Responsibility Segregation)
```csharp
// Commands (Write side)
public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

// Queries (Read side)  
public class GetProductByIdQuery : IRequest<ProductDto>
{
    public int ProductId { get; set; }
}

// Handlers
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
```

#### Event Sourcing (pour audit critique)
```csharp
// Events
public class ProductCreatedEvent : DomainEvent
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Event Store
EventStore (Id, AggregateId, EventType, EventData, Version, Timestamp)
```

## 🔗 Communication inter-services

### Patterns de communication

#### 1. Synchrone (REST APIs)
```csharp
// Service-to-service direct calls
public class OrderService
{
    private readonly IProductServiceClient _productService;
    private readonly IStockServiceClient _stockService;
    
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        // Valider produits
        var products = await _productService.GetProductsAsync(request.ProductIds);
        
        // Vérifier stock
        var availability = await _stockService.CheckAvailabilityAsync(request.Items);
        
        // Créer commande...
    }
}
```

#### 2. Asynchrone (Events via RabbitMQ)
```csharp
// Publisher
public class OrderService
{
    public async Task CompleteOrderAsync(int orderId)
    {
        // Logique métier...
        
        // Publier événement
        await _eventBus.PublishAsync(new OrderCompletedEvent 
        { 
            OrderId = orderId,
            Items = orderItems 
        });
    }
}

// Subscriber
public class StockService : IEventHandler<OrderCompletedEvent>
{
    public async Task HandleAsync(OrderCompletedEvent @event)
    {
        // Décrémenter stock automatiquement
        foreach(var item in @event.Items)
        {
            await DecrementStockAsync(item.ProductId, item.Quantity);
        }
    }
}
```

### API Gateway Pattern
```yaml
# NGINX Configuration
upstream auth_service {
    server auth-service:5001;
}

upstream product_service {
    server product-service:5002;
}

server {
    listen 80;
    
    # Authentification
    location /api/auth/ {
        proxy_pass http://auth_service/;
    }
    
    # Produits  
    location /api/products/ {
        proxy_pass http://product_service/;
        # Require auth
        auth_request /auth;
    }
}
```

## 🛡️ Sécurité architecture

### Authentification/Autorisation
```csharp
// JWT Token Structure
{
  "sub": "user123",
  "device": "device456", 
  "roles": ["cashier", "stock_manager"],
  "permissions": ["orders.create", "stock.read"],
  "exp": 1640995200,
  "iat": 1640908800
}

// Device Key Validation
public class DeviceAuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var deviceKey = context.Request.Headers["X-Device-Key"];
        var isValidDevice = await _deviceService.ValidateDeviceAsync(deviceKey);
        
        if (!isValidDevice)
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        await next(context);
    }
}
```

### Chiffrement des données
```csharp
// Chiffrement données sensibles
public class EncryptedEntity
{
    [Encrypted]
    public string CreditCardNumber { get; set; }
    
    [Encrypted]  
    public string CustomerEmail { get; set; }
}

// Configuration EF Core
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>()
        .Property(e => e.Email)
        .HasConversion(
            v => _encryptionService.Encrypt(v),
            v => _encryptionService.Decrypt(v)
        );
}
```

## 📊 Monitoring et observabilité

### Logging structuré (Serilog)
```csharp
// Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200")))
    .CreateLogger();

// Usage
_logger.Information("Order {OrderId} created by user {UserId} with total {Total}", 
    orderId, userId, total);
```

### Métriques (Prometheus)
```csharp
// Custom metrics
private static readonly Counter OrdersCreated = Metrics
    .CreateCounter("orders_created_total", "Total orders created");

private static readonly Histogram OrderProcessingTime = Metrics
    .CreateHistogram("order_processing_duration_seconds", "Order processing time");

// Usage
public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
{
    using (OrderProcessingTime.NewTimer())
    {
        var order = await ProcessOrderAsync(request);
        OrdersCreated.Inc();
        return order;
    }
}
```

### Health Checks
```csharp
// Startup configuration
services.AddHealthChecks()
    .AddMySql(connectionString)
    .AddRedis(redisConnectionString)
    .AddRabbitMQ(rabbitConnectionString)
    .AddCheck<CustomHealthCheck>("business-rules");

// Endpoints
GET /health           // Overall health
GET /health/ready     // Readiness probe
GET /health/live      // Liveness probe
```

## 🚀 Déploiement et infrastructure

### Docker Compose (Développement)
```yaml
version: '3.8'
services:
  auth-service:
    build: ./src/Services/Auth
    ports:
      - "5001:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=mysql;Database=AuthDB;
    depends_on:
      - mysql
      - redis

  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: yourpassword
    volumes:
      - mysql_data:/var/lib/mysql

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
```

### Kubernetes (Production)
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auth-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: auth-service
  template:
    metadata:
      labels:
        app: auth-service
    spec:
      containers:
      - name: auth-service
        image: niespro/auth-service:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

Cette architecture technique fournit une base solide et évolutive pour le développement du projet NiesPro ERP, avec tous les patterns et bonnes pratiques nécessaires pour un système de classe entreprise.

---

**Architecture validée le :** [Date]  
**Architecte :** [Nom]  
**Tech Lead :** [Nom]
