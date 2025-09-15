using FluentValidation;

namespace Catalog.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Validator for CreateProductCommand
    /// </summary>
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
                .Matches("^[A-Za-z0-9-_]+$").WithMessage("SKU can only contain letters, numbers, hyphens and underscores");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.LongDescription)
                .MaximumLength(2000).WithMessage("Long description cannot exceed 2000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.ComparePrice)
                .GreaterThan(x => x.Price).WithMessage("Compare price must be greater than regular price")
                .When(x => x.ComparePrice.HasValue);

            RuleFor(x => x.CostPrice)
                .GreaterThan(0).WithMessage("Cost price must be greater than 0")
                .When(x => x.CostPrice.HasValue);

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");

            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold cannot be negative")
                .When(x => x.LowStockThreshold.HasValue);

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0")
                .When(x => x.Weight.HasValue);

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
                .Must(BeAValidUrl).WithMessage("Image URL must be a valid URL")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required");
        }

        private static bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var result) 
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}