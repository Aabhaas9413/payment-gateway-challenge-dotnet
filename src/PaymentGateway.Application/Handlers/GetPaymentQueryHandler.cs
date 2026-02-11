using MediatR;
using PaymentGateway.Application.Queries;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application.Handlers;

public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, GetPaymentQueryResult?>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentQueryHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public Task<GetPaymentQueryResult?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = _repository.Get(request.Id);

        if (payment == null)
        {
            return Task.FromResult<GetPaymentQueryResult?>(null);
        }

        var result = new GetPaymentQueryResult
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };

        return Task.FromResult<GetPaymentQueryResult?>(result);
    }
}
