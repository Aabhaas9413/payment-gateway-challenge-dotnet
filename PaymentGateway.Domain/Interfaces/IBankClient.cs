using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Interfaces;

/// <summary>
/// Interface for communicating with the acquiring bank
/// </summary>
public interface IBankClient
{
    /// <summary>
    /// Processes a payment through the bank
    /// </summary>
    /// <param name="request">Payment details to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bank response indicating authorization status</returns>
    Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}
