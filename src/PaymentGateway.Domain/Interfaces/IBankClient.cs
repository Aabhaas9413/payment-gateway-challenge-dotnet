using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Interfaces;

/// <summary>
/// Interface for communicating with the acquiring bank
/// </summary>
public interface IBankClient
{
    Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}
