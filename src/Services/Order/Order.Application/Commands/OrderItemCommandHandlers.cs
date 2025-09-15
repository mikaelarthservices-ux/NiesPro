using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using FluentValidation;

namespace Order.Application.Commands;

public sealed class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<AddOrderItemCommand> _validator;

    public AddOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IValidator<AddOrderItemCommand> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public async Task<bool> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(request.ItemData.OrderId, cancellationToken);
        if (order == null)
            return false;

        try
        {
            var unitPrice = Money.Create(request.ItemData.UnitPrice, request.ItemData.Currency);
            order.AddItem(
                request.ItemData.ProductId,
                request.ItemData.ProductName,
                request.ItemData.ProductSku,
                request.ItemData.Quantity,
                unitPrice);

            await _orderRepository.UpdateAsync(order, cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

public sealed class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<UpdateOrderItemCommand> _validator;

    public UpdateOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IValidator<UpdateOrderItemCommand> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateOrderItemCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(request.ItemData.OrderId, cancellationToken);
        if (order == null)
            return false;

        try
        {
            order.UpdateItemQuantity(request.ItemData.ProductId, request.ItemData.NewQuantity);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

public sealed class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<RemoveOrderItemCommand> _validator;

    public RemoveOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IValidator<RemoveOrderItemCommand> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public async Task<bool> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(request.ItemData.OrderId, cancellationToken);
        if (order == null)
            return false;

        try
        {
            order.RemoveItem(request.ItemData.ProductId, request.ItemData.Quantity);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return false;

        try
        {
            order.Cancel(request.Reason);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IValidator<ProcessPaymentCommand> _validator;

    public ProcessPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IValidator<ProcessPaymentCommand> validator)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _validator = validator;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(request.PaymentData.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {request.PaymentData.OrderId} not found");

        var payment = order.AddPayment(request.PaymentData.PaymentMethod);

        // Mark as processing with transaction ID
        payment.MarkAsProcessing(request.PaymentData.TransactionId);

        // Simulate payment processing completion
        payment.MarkAsCompleted(request.PaymentData.ProviderReference);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return payment.Id;
    }
}