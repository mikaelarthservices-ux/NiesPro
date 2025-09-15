using FluentValidation;
using Order.Application.Commands;
using Order.Domain.Enums;

namespace Order.Application.Validators;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.StatusData.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.StatusData.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid order status");

        RuleFor(x => x.StatusData.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Cancellation reason is required and must be less than 500 characters")
            .When(x => x.StatusData.NewStatus == OrderStatus.Cancelled);

        RuleFor(x => x.StatusData.TrackingNumber)
            .MaximumLength(100)
            .WithMessage("Tracking number must be less than 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.StatusData.TrackingNumber));
    }
}

public sealed class AddOrderItemCommandValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.ItemData.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.ItemData.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ItemData.ProductName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Product name is required and must be less than 100 characters");

        RuleFor(x => x.ItemData.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity must be between 1 and 1000");

        RuleFor(x => x.ItemData.UnitPrice)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000000)
            .WithMessage("Unit price must be between 0.01 and 1,000,000");

        RuleFor(x => x.ItemData.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a valid 3-letter code");
    }
}

public sealed class UpdateOrderItemCommandValidator : AbstractValidator<UpdateOrderItemCommand>
{
    public UpdateOrderItemCommandValidator()
    {
        RuleFor(x => x.ItemData.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.ItemData.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ItemData.NewQuantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity must be between 1 and 1000");
    }
}

public sealed class RemoveOrderItemCommandValidator : AbstractValidator<RemoveOrderItemCommand>
{
    public RemoveOrderItemCommandValidator()
    {
        RuleFor(x => x.ItemData.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.ItemData.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ItemData.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
    }
}

public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentData.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.PaymentData.PaymentMethod)
            .IsInEnum()
            .WithMessage("Invalid payment method");

        RuleFor(x => x.PaymentData.TransactionId)
            .MaximumLength(100)
            .WithMessage("Transaction ID must be less than 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentData.TransactionId));

        RuleFor(x => x.PaymentData.ProviderReference)
            .MaximumLength(100)
            .WithMessage("Provider reference must be less than 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentData.ProviderReference));
    }
}