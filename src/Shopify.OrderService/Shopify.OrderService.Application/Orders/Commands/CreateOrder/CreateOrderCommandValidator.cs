

using FluentValidation;

namespace Shopify.OrderService.Application.Orders.Commands.CreateOrder;
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    private const int MaxQuantityPerOrder = 1000;

    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("A valid Product ID must be provided.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(MaxQuantityPerOrder)
            .WithMessage($"Quantity cannot exceed {MaxQuantityPerOrder} per order.");
    }
}