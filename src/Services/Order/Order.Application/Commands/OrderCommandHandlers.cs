using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using Order.Domain.Entities;
using FluentValidation;

namespace Order.Application.Commands;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<CreateOrderCommand> _validator;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IValidator<CreateOrderCommand> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validation
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        // Generate order number
        var orderNumber = await _orderRepository.GenerateOrderNumberAsync(cancellationToken);

        // Create value objects
        var customerInfo = CustomerInfo.Create(
            request.OrderData.CustomerInfo.FirstName,
            request.OrderData.CustomerInfo.LastName,
            request.OrderData.CustomerInfo.Email,
            request.OrderData.CustomerInfo.PhoneNumber);

        var shippingAddress = Address.Create(
            request.OrderData.ShippingAddress.Street,
            request.OrderData.ShippingAddress.City,
            request.OrderData.ShippingAddress.PostalCode,
            request.OrderData.ShippingAddress.Country,
            request.OrderData.ShippingAddress.AddressLine2);

        Address? billingAddress = null;
        if (request.OrderData.BillingAddress != null)
        {
            billingAddress = Address.Create(
                request.OrderData.BillingAddress.Street,
                request.OrderData.BillingAddress.City,
                request.OrderData.BillingAddress.PostalCode,
                request.OrderData.BillingAddress.Country,
                request.OrderData.BillingAddress.AddressLine2);
        }

        // Create order aggregate
        var order = Domain.Entities.Order.Create(
            orderNumber,
            request.OrderData.CustomerId,
            customerInfo,
            shippingAddress,
            billingAddress);

        // Add items
        foreach (var itemDto in request.OrderData.Items)
        {
            var unitPrice = Money.Create(itemDto.UnitPrice, request.OrderData.Currency);
            order.AddItem(
                itemDto.ProductId,
                itemDto.ProductName,
                itemDto.ProductSku,
                itemDto.Quantity,
                unitPrice);
        }

        // Set tax, shipping, and discount
        if (request.OrderData.TaxAmount > 0)
        {
            order.SetTax(Money.Create(request.OrderData.TaxAmount, request.OrderData.Currency));
        }

        if (request.OrderData.ShippingCost > 0)
        {
            order.SetShippingCost(Money.Create(request.OrderData.ShippingCost, request.OrderData.Currency));
        }

        if (request.OrderData.DiscountAmount > 0)
        {
            order.ApplyDiscount(Money.Create(request.OrderData.DiscountAmount, request.OrderData.Currency));
        }

        // Save order
        await _orderRepository.AddAsync(order, cancellationToken);

        return order.Id;
    }
}

public sealed class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public ConfirmOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return false;

        order.Confirm();
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return true;
    }
}

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<UpdateOrderStatusCommand> _validator;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IValidator<UpdateOrderStatusCommand> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(request.StatusData.OrderId, cancellationToken);
        if (order == null)
            return false;

        try
        {
            switch (request.StatusData.NewStatus)
            {
                case Domain.Enums.OrderStatus.Processing:
                    order.StartProcessing();
                    break;
                case Domain.Enums.OrderStatus.Shipped:
                    order.MarkAsShipped(request.StatusData.TrackingNumber);
                    break;
                case Domain.Enums.OrderStatus.Delivered:
                    order.MarkAsDelivered();
                    break;
                case Domain.Enums.OrderStatus.Cancelled:
                    order.Cancel(request.StatusData.Reason ?? "Order cancelled");
                    break;
                default:
                    return false;
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}