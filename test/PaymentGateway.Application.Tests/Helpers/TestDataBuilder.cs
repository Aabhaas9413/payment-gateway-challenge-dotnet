using PaymentGateway.Application.Commands;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Tests.Helpers;

/// <summary>
/// Test data builder for creating valid test objects
/// </summary>
public static class TestDataBuilder
{
    public static ProcessPaymentCommand CreateValidCommand()
    {
        return new ProcessPaymentCommand
        {
            Id = Guid.NewGuid(),
            CardNumber = "4532123456789012", // Valid 16-digit card
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1, // Next year
            Currency = "USD",
            Amount = 10000, // $100.00 in cents
            Cvv = "123"
        };
    }

    public static Payment CreateValidPayment()
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            CardNumberLastFour = 9012,
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Currency = "USD",
            Amount = 10000,
            Status = Domain.Enums.PaymentStatus.Authorized
        };
    }
}
