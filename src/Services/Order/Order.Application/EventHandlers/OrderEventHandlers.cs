using MediatR;
using Order.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Order.Application.EventHandlers;

public sealed class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} created for customer {CustomerId} with total amount {TotalAmount}",
            notification.OrderId,
            notification.CustomerId,
            notification.TotalAmount);

        // Here we could:
        // - Send welcome email to customer
        // - Update customer statistics
        // - Trigger inventory reservation
        // - Create audit log entry
        // - Publish event to message bus for other services

        return Task.CompletedTask;
    }
}

public sealed class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} status changed from {PreviousStatus} to {NewStatus}. Reason: {Reason}",
            notification.OrderId,
            notification.PreviousStatus,
            notification.NewStatus,
            notification.Reason ?? "Not specified");

        // Here we could:
        // - Send status update email/SMS to customer
        // - Update delivery tracking system
        // - Trigger warehouse operations
        // - Update analytics dashboards
        // - Notify external partners (shipping, payment providers)

        return Task.CompletedTask;
    }
}

public sealed class OrderItemAddedEventHandler : INotificationHandler<OrderItemAddedEvent>
{
    private readonly ILogger<OrderItemAddedEventHandler> _logger;

    public OrderItemAddedEventHandler(ILogger<OrderItemAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderItemAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Item {ProductName} (Qty: {Quantity}) added to order {OrderId}",
            notification.ProductName,
            notification.Quantity,
            notification.OrderId);

        // Here we could:
        // - Update inventory reservations
        // - Recalculate order totals
        // - Update product recommendation engine
        // - Trigger price validation
        // - Update customer purchase history

        return Task.CompletedTask;
    }
}

public sealed class OrderItemRemovedEventHandler : INotificationHandler<OrderItemRemovedEvent>
{
    private readonly ILogger<OrderItemRemovedEventHandler> _logger;

    public OrderItemRemovedEventHandler(ILogger<OrderItemRemovedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderItemRemovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Item with Product ID {ProductId} (Qty: {RemovedQuantity}) removed from order {OrderId}",
            notification.ProductId,
            notification.RemovedQuantity,
            notification.OrderId);

        // Here we could:
        // - Release inventory reservations
        // - Recalculate order totals
        // - Update analytics
        // - Trigger restocking if needed

        return Task.CompletedTask;
    }
}

public sealed class PaymentProcessedEventHandler : INotificationHandler<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public PaymentProcessedEventHandler(ILogger<PaymentProcessedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment {PaymentId} for order {OrderId} processed via {PaymentMethod}. Status: {PaymentStatus}, Amount: {Amount}",
            notification.PaymentId,
            notification.OrderId,
            notification.PaymentMethod,
            notification.PaymentStatus,
            notification.Amount);

        // Here we could:
        // - Send payment confirmation to customer
        // - Update accounting system
        // - Trigger order fulfillment if fully paid
        // - Update fraud detection systems
        // - Generate receipts and invoices

        return Task.CompletedTask;
    }
}