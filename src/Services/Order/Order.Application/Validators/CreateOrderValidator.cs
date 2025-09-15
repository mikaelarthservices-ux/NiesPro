using FluentValidation;
using Order.Application.Commands;
using Order.Application.DTOs;

namespace Order.Application.Validators;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderData.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.OrderData.CustomerInfo)
            .NotNull()
            .WithMessage("Customer information is required")
            .SetValidator(new CustomerInfoDtoValidator());

        RuleFor(x => x.OrderData.ShippingAddress)
            .NotNull()
            .WithMessage("Shipping address is required")
            .SetValidator(new AddressDtoValidator());

        RuleFor(x => x.OrderData.BillingAddress!)
            .SetValidator(new AddressDtoValidator())
            .When(x => x.OrderData.BillingAddress != null);

        RuleFor(x => x.OrderData.Items)
            .NotEmpty()
            .WithMessage("At least one item is required");

        RuleForEach(x => x.OrderData.Items)
            .SetValidator(new OrderItemDtoValidator());

        RuleFor(x => x.OrderData.TaxAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tax amount cannot be negative");

        RuleFor(x => x.OrderData.ShippingCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Shipping cost cannot be negative");

        RuleFor(x => x.OrderData.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount amount cannot be negative");

        RuleFor(x => x.OrderData.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a valid 3-letter code");
    }
}

public sealed class CustomerInfoDtoValidator : AbstractValidator<CustomerInfoDto>
{
    public CustomerInfoDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("First name is required and must be less than 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Last name is required and must be less than 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100)
            .WithMessage("Valid email address is required");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+\d{1,3}[-.\s]?)?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9}$")
            .WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Street is required and must be less than 100 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("City is required and must be less than 50 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Postal code is required and must be less than 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Country is required and must be less than 50 characters");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(100)
            .WithMessage("Address line 2 must be less than 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));
    }
}

public sealed class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Product name is required and must be less than 100 characters");

        RuleFor(x => x.ProductSku)
            .MaximumLength(50)
            .WithMessage("Product SKU must be less than 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ProductSku));

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity must be between 1 and 1000");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000000)
            .WithMessage("Unit price must be between 0.01 and 1,000,000");
    }
}