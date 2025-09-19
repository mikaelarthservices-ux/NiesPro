using BuildingBlocks.Domain;
using Stock.Domain.Events;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

/// <summary>
/// Entité représentant un fournisseur
/// </summary>
public sealed class Supplier : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public LocationAddress? Address { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? ContactTitle { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? BankAccount { get; private set; }
    public ValidityPeriod? ContractPeriod { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? DeliveryTerms { get; private set; }
    public decimal? CreditLimit { get; private set; }
    public string? Currency { get; private set; }
    public bool IsActive { get; private set; }
    public string? DeactivationReason { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    // Métriques de performance
    public decimal? AverageDeliveryTime { get; private set; } // En jours
    public decimal? QualityRating { get; private set; } // 0-5
    public decimal? OnTimeDeliveryRate { get; private set; } // 0-100%
    public int TotalOrders { get; private set; }
    public DateTime? LastOrderDate { get; private set; }

    private readonly List<SupplierProduct> _products = new();
    public IReadOnlyList<SupplierProduct> Products => _products.AsReadOnly();

    private Supplier() { } // EF Constructor

    public Supplier(
        string name,
        string code,
        string email,
        string phone,
        string? website = null,
        LocationAddress? address = null,
        string? contactPerson = null,
        string? contactTitle = null,
        string? taxNumber = null,
        string? currency = "EUR")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Supplier code cannot be null or empty", nameof(code));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be null or empty", nameof(phone));

        ValidateEmail(email);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Website = website?.Trim();
        Address = address;
        ContactPerson = contactPerson?.Trim();
        ContactTitle = contactTitle?.Trim();
        TaxNumber = taxNumber?.Trim();
        Currency = currency?.Trim().ToUpperInvariant() ?? "EUR";
        IsActive = true;
        TotalOrders = 0;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new SupplierCreatedEvent(
            Id,
            Name,
            Code,
            Email,
            Phone,
            CreatedAt));
    }

    /// <summary>
    /// Mettre à jour les informations du fournisseur
    /// </summary>
    public void UpdateInformation(
        string name,
        string email,
        string phone,
        string? website = null,
        LocationAddress? address = null,
        string? contactPerson = null,
        string? contactTitle = null,
        string? taxNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be null or empty", nameof(phone));

        ValidateEmail(email);

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Website = website?.Trim();
        Address = address;
        ContactPerson = contactPerson?.Trim();
        ContactTitle = contactTitle?.Trim();
        TaxNumber = taxNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SupplierUpdatedEvent(
            Id,
            Name,
            Code,
            Email,
            Phone,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Mettre à jour les conditions commerciales
    /// </summary>
    public void UpdateCommercialTerms(
        ValidityPeriod? contractPeriod = null,
        string? paymentTerms = null,
        string? deliveryTerms = null,
        decimal? creditLimit = null,
        string? currency = null)
    {
        ContractPeriod = contractPeriod;
        PaymentTerms = paymentTerms?.Trim();
        DeliveryTerms = deliveryTerms?.Trim();
        CreditLimit = creditLimit;
        
        if (!string.IsNullOrWhiteSpace(currency))
            Currency = currency.Trim().ToUpperInvariant();
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter un produit au catalogue du fournisseur
    /// </summary>
    public void AddProduct(Guid productId, string supplierProductCode, UnitCost unitCost, int minimumOrderQuantity = 1, int deliveryTimeInDays = 7)
    {
        if (_products.Any(p => p.ProductId == productId))
            throw new InvalidOperationException($"Product {productId} is already in supplier catalog");

        var supplierProduct = new SupplierProduct(Id, productId, supplierProductCode, unitCost, minimumOrderQuantity, deliveryTimeInDays);
        _products.Add(supplierProduct);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Supprimer un produit du catalogue
    /// </summary>
    public void RemoveProduct(Guid productId)
    {
        var product = _products.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            _products.Remove(product);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mettre à jour le prix d'un produit
    /// </summary>
    public void UpdateProductPrice(Guid productId, UnitCost newUnitCost)
    {
        var product = _products.FirstOrDefault(p => p.ProductId == productId);
        if (product == null)
            throw new InvalidOperationException($"Product {productId} not found in supplier catalog");

        product.UpdateUnitCost(newUnitCost);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enregistrer une nouvelle commande
    /// </summary>
    public void RegisterOrder()
    {
        TotalOrders++;
        LastOrderDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour les métriques de performance
    /// </summary>
    public void UpdatePerformanceMetrics(
        decimal? averageDeliveryTime = null,
        decimal? qualityRating = null,
        decimal? onTimeDeliveryRate = null)
    {
        if (averageDeliveryTime.HasValue)
        {
            if (averageDeliveryTime.Value < 0)
                throw new ArgumentException("Average delivery time cannot be negative", nameof(averageDeliveryTime));
            AverageDeliveryTime = averageDeliveryTime.Value;
        }

        if (qualityRating.HasValue)
        {
            if (qualityRating.Value < 0 || qualityRating.Value > 5)
                throw new ArgumentException("Quality rating must be between 0 and 5", nameof(qualityRating));
            QualityRating = qualityRating.Value;
        }

        if (onTimeDeliveryRate.HasValue)
        {
            if (onTimeDeliveryRate.Value < 0 || onTimeDeliveryRate.Value > 100)
                throw new ArgumentException("On-time delivery rate must be between 0 and 100", nameof(onTimeDeliveryRate));
            OnTimeDeliveryRate = onTimeDeliveryRate.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Désactiver le fournisseur
    /// </summary>
    public void Deactivate(string reason)
    {
        if (!IsActive)
            throw new InvalidOperationException("Supplier is already deactivated");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Deactivation reason is required", nameof(reason));

        IsActive = false;
        DeactivationReason = reason.Trim();
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SupplierDeactivatedEvent(
            Id,
            Name,
            DeactivationReason,
            DeactivatedAt.Value));
    }

    /// <summary>
    /// Réactiver le fournisseur
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            throw new InvalidOperationException("Supplier is already active");

        IsActive = true;
        DeactivationReason = null;
        DeactivatedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si le contrat est valide
    /// </summary>
    public bool HasValidContract => ContractPeriod?.IsCurrentlyActive ?? true;

    /// <summary>
    /// Calculer le score de performance global
    /// </summary>
    public decimal CalculatePerformanceScore()
    {
        var scores = new List<decimal>();

        if (QualityRating.HasValue)
            scores.Add(QualityRating.Value);

        if (OnTimeDeliveryRate.HasValue)
            scores.Add(OnTimeDeliveryRate.Value / 20); // Convertir 0-100% en 0-5

        if (AverageDeliveryTime.HasValue)
        {
            // Plus le délai est court, meilleur est le score (max 5 pour ≤ 1 jour, 1 pour ≥ 30 jours)
            var deliveryScore = Math.Max(1, 5 - (AverageDeliveryTime.Value - 1) / 7);
            scores.Add(Math.Min(5, deliveryScore));
        }

        return scores.Any() ? scores.Average() : 0;
    }

    private static void ValidateEmail(string email)
    {
        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));
    }
}

/// <summary>
/// Produit référencé chez un fournisseur
/// </summary>
public sealed class SupplierProduct : Entity
{
    public Guid SupplierId { get; private set; }
    public Guid ProductId { get; private set; }
    public string SupplierProductCode { get; private set; } = string.Empty;
    public UnitCost UnitCost { get; private set; }
    public int MinimumOrderQuantity { get; private set; }
    public int DeliveryTimeInDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastOrderDate { get; private set; }
    public UnitCost? LastOrderUnitCost { get; private set; }

    private SupplierProduct() { } // EF Constructor

    public SupplierProduct(
        Guid supplierId,
        Guid productId,
        string supplierProductCode,
        UnitCost unitCost,
        int minimumOrderQuantity = 1,
        int deliveryTimeInDays = 7)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID cannot be empty", nameof(supplierId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(supplierProductCode))
            throw new ArgumentException("Supplier product code cannot be null or empty", nameof(supplierProductCode));

        if (minimumOrderQuantity <= 0)
            throw new ArgumentException("Minimum order quantity must be positive", nameof(minimumOrderQuantity));

        if (deliveryTimeInDays <= 0)
            throw new ArgumentException("Delivery time must be positive", nameof(deliveryTimeInDays));

        Id = Guid.NewGuid();
        SupplierId = supplierId;
        ProductId = productId;
        SupplierProductCode = supplierProductCode.Trim();
        UnitCost = unitCost ?? throw new ArgumentNullException(nameof(unitCost));
        MinimumOrderQuantity = minimumOrderQuantity;
        DeliveryTimeInDays = deliveryTimeInDays;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateUnitCost(UnitCost newUnitCost)
    {
        LastOrderUnitCost = UnitCost;
        UnitCost = newUnitCost ?? throw new ArgumentNullException(nameof(newUnitCost));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOrderingInfo(int minimumOrderQuantity, int deliveryTimeInDays)
    {
        if (minimumOrderQuantity <= 0)
            throw new ArgumentException("Minimum order quantity must be positive", nameof(minimumOrderQuantity));

        if (deliveryTimeInDays <= 0)
            throw new ArgumentException("Delivery time must be positive", nameof(deliveryTimeInDays));

        MinimumOrderQuantity = minimumOrderQuantity;
        DeliveryTimeInDays = deliveryTimeInDays;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterOrder()
    {
        LastOrderDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}