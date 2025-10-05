using FluentValidation;
using Orders.Application.DTOs;

namespace Orders.Application.Validators
{
    public class AddItemRequestValidator : AbstractValidator<AddItemRequest>
    {
        public AddItemRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Item name is required.")
                .MaximumLength(100).WithMessage("Item name cannot exceed 100 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
        }
    }
}
