using MediatR;
using PaymentGateway.Application.Commands;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application.Handlers;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IPaymentRepository _repository;

    public ProcessPaymentCommandHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var existingPayment = _repository.Get(request.Id);
        if (existingPayment != null)
        {
            return Task.FromResult(MapToResult(existingPayment));
        }

        var lastFourDigits = int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4));

        var payment = new Payment
        {
            Id = request.Id,
            CardNumberLastFour = lastFourDigits,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Status = PaymentStatus.Authorized
        };

        _repository.Add(payment);

        return Task.FromResult(MapToResult(payment));
    }

    private static ProcessPaymentResult MapToResult(Payment payment)
    {
        return new ProcessPaymentResult
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };
    }
}
