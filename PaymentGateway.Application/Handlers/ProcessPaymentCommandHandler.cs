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
        // Check for idempotency - if payment already exists, return it
        var existingPayment = _repository.Get(request.Id);
        if (existingPayment != null)
        {
            return MapToResult(existingPayment);
        }

        // Format expiry date as MM/YYYY for bank (e.g., "04/2025")
        var expiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}";

        // Call bank to process payment
        var bankRequest = new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = expiryDate,
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };

        var bankResponse = await _bankClient.ProcessPaymentAsync(bankRequest, cancellationToken);

        // Extract last 4 digits for storage
        var lastFourDigits = int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4));

        // Create payment entity with status from bank
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
