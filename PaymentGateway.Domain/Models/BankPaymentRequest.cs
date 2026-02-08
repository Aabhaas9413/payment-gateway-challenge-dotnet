namespace PaymentGateway.Domain.Models;

/// <summary>
/// Request model for bank payment processing
/// </summary>
public class BankPaymentRequest
{
    public required string CardNumber { get; set; }
    public required string ExpiryDate { get; set; }
    public required string Currency { get; set; }
    public int Amount { get; set; }
    public required string Cvv { get; set; }
}
