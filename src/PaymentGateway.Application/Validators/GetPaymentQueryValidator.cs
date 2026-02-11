using FluentValidation;

namespace PaymentGateway.Application.Queries;

public class GetPaymentQueryValidator : AbstractValidator<GetPaymentQuery>
{
    public GetPaymentQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Payment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Payment ID cannot be empty");
    }
}
