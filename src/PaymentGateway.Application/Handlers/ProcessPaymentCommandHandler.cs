using MediatR;
using PaymentGateway.Application.Commands;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Handlers;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IPaymentRepository _repository;
    private readonly IBankClient _bankClient;

    public ProcessPaymentCommandHandler(IPaymentRepository repository, IBankClient bankClient)
    {
        _repository = repository;
        _bankClient = bankClient;
    }

    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var existingPayment = _repository.Get(request.Id);
        if (existingPayment != null)
        {
            return MapToResult(existingPayment);
        }

        var expiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}";

        var bankRequest = new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = expiryDate,
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };

        var bankResponse = await _bankClient.ProcessPaymentAsync(bankRequest, cancellationToken);

        var lastFourDigits = int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4));

        var payment = new Payment
        {
            Id = request.Id,
            CardNumberLastFour = lastFourDigits,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
        };

        _repository.Add(payment);

        return MapToResult(payment);
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
