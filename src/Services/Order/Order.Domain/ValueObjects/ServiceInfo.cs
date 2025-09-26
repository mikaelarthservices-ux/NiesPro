using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Value Object Enterprise pour informations de service contextuelles
/// Architecture multi-contexte alignée sur NiesPro ERP standards
/// </summary>
public sealed class ServiceInfo : ValueObject
{
    public BusinessContext Context { get; private set; }
    public ServiceType Type { get; private set; }
    public int? TableNumber { get; private set; }
    public Guid? TerminalId { get; private set; }
    public Guid? WaiterId { get; private set; }
    public string? ServiceNotes { get; private set; }
    public DateTime? ReservationTime { get; private set; }
    public TimeSpan? EstimatedDuration { get; private set; }
    public string? DeliveryAddress { get; private set; }
    public CustomerInfo? CustomerInfo { get; private set; }

    private ServiceInfo() { } // EF Constructor

    private ServiceInfo(
        BusinessContext context,
        ServiceType type,
        int? tableNumber = null,
        Guid? terminalId = null,
        Guid? waiterId = null,
        string? serviceNotes = null,
        DateTime? reservationTime = null,
        TimeSpan? estimatedDuration = null,
        string? deliveryAddress = null,
        CustomerInfo? customerInfo = null)
    {
        Context = context;
        Type = type;
        TableNumber = tableNumber;
        TerminalId = terminalId;
        WaiterId = waiterId;
        ServiceNotes = serviceNotes;
        ReservationTime = reservationTime;
        EstimatedDuration = estimatedDuration;
        DeliveryAddress = deliveryAddress;
        CustomerInfo = customerInfo;

        ValidateBusinessRules();
    }

    /// <summary>
    /// Factory pour contexte Restaurant
    /// </summary>
    public static ServiceInfo CreateRestaurant(
        ServiceType type,
        int? tableNumber = null,
        Guid? waiterId = null,
        string? serviceNotes = null,
        DateTime? reservationTime = null,
        TimeSpan? estimatedDuration = null)
    {
        if (type == ServiceType.DineIn && tableNumber == null)
            throw new ArgumentException("Table number is required for dine-in service");

        return new ServiceInfo(
            BusinessContext.Restaurant,
            type,
            tableNumber,
            null,
            waiterId,
            serviceNotes,
            reservationTime,
            estimatedDuration);
    }

    /// <summary>
    /// Factory pour contexte Boutique
    /// </summary>
    public static ServiceInfo CreateBoutique(
        Guid terminalId,
        ServiceType type = ServiceType.InStore,
        string? serviceNotes = null)
    {
        return new ServiceInfo(
            BusinessContext.Boutique,
            type,
            null,
            terminalId,
            null,
            serviceNotes);
    }

    /// <summary>
    /// Factory pour contexte E-Commerce
    /// </summary>
    public static ServiceInfo CreateECommerce(
        string deliveryAddress,
        CustomerInfo customerInfo,
        ServiceType type = ServiceType.Delivery,
        TimeSpan? estimatedDuration = null)
    {
        if (string.IsNullOrWhiteSpace(deliveryAddress))
            throw new ArgumentException("Delivery address is required for e-commerce orders");

        return new ServiceInfo(
            BusinessContext.ECommerce,
            type,
            null,
            null,
            null,
            null,
            null,
            estimatedDuration,
            deliveryAddress,
            customerInfo);
    }

    /// <summary>
    /// Factory pour contexte Wholesale
    /// </summary>
    public static ServiceInfo CreateWholesale(
        CustomerInfo customerInfo,
        string? deliveryAddress = null,
        TimeSpan? estimatedDuration = null)
    {
        return new ServiceInfo(
            BusinessContext.Wholesale,
            ServiceType.Delivery,
            null,
            null,
            null,
            null,
            null,
            estimatedDuration,
            deliveryAddress,
            customerInfo);
    }

    /// <summary>
    /// Validation des règles métier selon le contexte
    /// </summary>
    private void ValidateBusinessRules()
    {
        switch (Context)
        {
            case BusinessContext.Restaurant:
                if (Type == ServiceType.DineIn && TableNumber == null)
                    throw new ArgumentException("Table number required for dine-in service");
                break;

            case BusinessContext.Boutique:
                if (TerminalId == null)
                    throw new ArgumentException("Terminal ID required for boutique service");
                break;

            case BusinessContext.ECommerce:
                if (string.IsNullOrWhiteSpace(DeliveryAddress))
                    throw new ArgumentException("Delivery address required for e-commerce");
                if (CustomerInfo == null)
                    throw new ArgumentException("Customer info required for e-commerce");
                break;

            case BusinessContext.Wholesale:
                if (CustomerInfo == null)
                    throw new ArgumentException("Customer info required for wholesale");
                break;
        }
    }

    /// <summary>
    /// Mise à jour du serveur assigné (Restaurant uniquement)
    /// </summary>
    public ServiceInfo WithWaiter(Guid waiterId)
    {
        if (Context != BusinessContext.Restaurant)
            throw new InvalidOperationException("Waiter assignment is only valid for restaurant context");

        return new ServiceInfo(
            Context, Type, TableNumber, TerminalId, waiterId, 
            ServiceNotes, ReservationTime, EstimatedDuration, 
            DeliveryAddress, CustomerInfo);
    }

    /// <summary>
    /// Mise à jour des notes de service
    /// </summary>
    public ServiceInfo WithNotes(string serviceNotes)
    {
        return new ServiceInfo(
            Context, Type, TableNumber, TerminalId, WaiterId,
            serviceNotes, ReservationTime, EstimatedDuration,
            DeliveryAddress, CustomerInfo);
    }

    /// <summary>
    /// Mise à jour de la durée estimée
    /// </summary>
    public ServiceInfo WithEstimatedDuration(TimeSpan duration)
    {
        return new ServiceInfo(
            Context, Type, TableNumber, TerminalId, WaiterId,
            ServiceNotes, ReservationTime, duration,
            DeliveryAddress, CustomerInfo);
    }

    /// <summary>
    /// Détermine si le service nécessite une réservation
    /// </summary>
    public bool RequiresReservation()
    {
        return Context == BusinessContext.Restaurant && 
               Type == ServiceType.DineIn && 
               ReservationTime.HasValue;
    }

    /// <summary>
    /// Détermine si le service nécessite une préparation en cuisine
    /// </summary>
    public bool RequiresKitchenPreparation()
    {
        return Context == BusinessContext.Restaurant &&
               Type is ServiceType.DineIn or ServiceType.TakeAway;
    }

    /// <summary>
    /// Détermine si le service nécessite un suivi de livraison
    /// </summary>
    public bool RequiresDeliveryTracking()
    {
        return Type == ServiceType.Delivery &&
               Context is BusinessContext.ECommerce or BusinessContext.Wholesale;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Context;
        yield return Type;
        yield return (object?)TableNumber ?? 0;
        yield return (object?)TerminalId ?? Guid.Empty;
        yield return (object?)WaiterId ?? Guid.Empty;
        yield return ServiceNotes ?? string.Empty;
        yield return (object?)ReservationTime ?? DateTime.MinValue;
        yield return (object?)EstimatedDuration ?? TimeSpan.Zero;
        yield return DeliveryAddress ?? string.Empty;
        yield return (object?)CustomerInfo ?? new object();
    }

    public override string ToString()
    {
        var details = Context switch
        {
            BusinessContext.Restaurant => $"Table {TableNumber}, {Type}",
            BusinessContext.Boutique => $"Terminal {TerminalId}, {Type}",
            BusinessContext.ECommerce => $"Delivery to {DeliveryAddress}",
            BusinessContext.Wholesale => $"Wholesale {Type}",
            _ => Context.ToString()
        };

        return $"ServiceInfo: {Context} - {details}";
    }
}

/// <summary>
/// Types de service supportés
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Service sur place (Restaurant)
    /// </summary>
    DineIn = 1,

    /// <summary>
    /// À emporter (Restaurant)  
    /// </summary>
    TakeAway = 2,

    /// <summary>
    /// Livraison (Restaurant/E-Commerce/Wholesale)
    /// </summary>
    Delivery = 3,

    /// <summary>
    /// En magasin (Boutique)
    /// </summary>
    InStore = 4,

    /// <summary>
    /// Click & Collect (Boutique/E-Commerce)
    /// </summary>
    ClickAndCollect = 5,

    /// <summary>
    /// Service de réservation (Restaurant)
    /// </summary>
    Reservation = 6
}