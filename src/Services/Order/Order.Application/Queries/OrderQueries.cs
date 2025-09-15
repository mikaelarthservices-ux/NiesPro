using MediatR;
using Order.Application.DTOs;
using Order.Domain.Enums;
using NiesPro.Contracts.Common;

namespace Order.Application.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

public sealed record GetOrderByNumberQuery(string OrderNumber) : IRequest<OrderDto?>;

public sealed record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<IEnumerable<OrderDto>>;

public sealed record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<IEnumerable<OrderDto>>;

public sealed record GetOrdersPagedQuery(
    int Page = 1,
    int PageSize = 10,
    OrderStatus? Status = null,
    Guid? CustomerId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<PaginatedResult<OrderDto>>;

public sealed record GetOrderHistoryQuery(Guid OrderId) : IRequest<IEnumerable<OrderEventDto>>;

public sealed record GetOrderStatsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<OrderStatsDto>;

public sealed record OrderEventDto
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public string Description { get; init; } = string.Empty;
    public object? EventData { get; init; }
}

public sealed record OrderStatsDto
{
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int ConfirmedOrders { get; init; }
    public int ProcessingOrders { get; init; }
    public int ShippedOrders { get; init; }
    public int DeliveredOrders { get; init; }
    public int CancelledOrders { get; init; }
    public MoneyDto TotalRevenue { get; init; } = null!;
    public MoneyDto AverageOrderValue { get; init; } = null!;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}