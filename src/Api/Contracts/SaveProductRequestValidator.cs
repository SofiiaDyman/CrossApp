using Core.Domain;
using FluentValidation;

namespace Api.Contracts;

public sealed class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().WithMessage("Артикул обов'язковий.")
            .MaximumLength(50).WithMessage("Артикул — до 50 символів.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200)
            .WithMessage("Назва обов'язкова, до 200 символів.");

        RuleFor(x => x.Category).NotEmpty().WithMessage("Категорія обов'язкова.")
            .Must(c => Product.AllowedCategories.Contains(c.Trim().ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Category))
            .WithMessage($"Невідома категорія. Допустимі: {string.Join(", ", Product.AllowedCategories)}.");

        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20)
            .WithMessage("Одиниця виміру обов'язкова, до 20 символів.");

        RuleFor(x => x.Quantity).InclusiveBetween(0, 1_000_000)
            .WithMessage("Кількість має бути від 0 до 1 000 000.");

        RuleFor(x => x.Note).MaximumLength(200)
            .WithMessage("Примітка — до 200 символів.");
    }
}
