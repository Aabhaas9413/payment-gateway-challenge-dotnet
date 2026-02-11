using FluentValidation;
using PaymentGateway.Application.Commands;

namespace PaymentGateway.Application.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    private static readonly string[] ValidCurrencies = { "USD", "GBP", "EUR" };

    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required")
            .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters")
            .Matches("^[0-9]+$").WithMessage("Card number must contain only digits");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12");

        RuleFor(x => x.ExpiryYear)
            .Must((command, expiryYear) => BeValidExpiryDate(command.ExpiryMonth, expiryYear))
            .WithMessage("Card has expired or expiry date is invalid")
            .When(x => x.ExpiryMonth >= 1 && x.ExpiryMonth <= 12);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters")
            .Must(c => ValidCurrencies.Contains(c)).WithMessage("Currency must be USD, GBP, or EUR");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required")
            .Length(3, 4).WithMessage("CVV must be 3 or 4 characters")
            .Matches("^[0-9]+$").WithMessage("CVV must contain only digits");
    }

    private bool BeValidExpiryDate(int month, int year)
    {
        if (month < 1 || month > 12)
            return false;

        try
        {
            var now = DateTime.Now;
            var expiryDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return expiryDate >= now.Date;
        }
        catch
        {
            return false;
        }
    }
}
