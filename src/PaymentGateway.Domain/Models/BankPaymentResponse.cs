namespace PaymentGateway.Domain.Models;

/// <summary>
/// Response model from bank payment processing
/// </summary>
public class BankPaymentResponse
{
    public bool Authorized { get; set; }
    public string? AuthorizationCode { get; set; }
    public bool BankUnavailable { get; set; }
}
