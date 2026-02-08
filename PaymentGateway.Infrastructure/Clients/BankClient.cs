using System.Net.Http.Json;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Infrastructure.Clients;

/// <summary>
/// HTTP client for communicating with the bank simulator
/// </summary>
public class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;

    public BankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Map domain request to bank API format
        var bankRequest = new
        {
            card_number = request.CardNumber,
            expiry_date = request.ExpiryDate,
            currency = request.Currency,
            amount = request.Amount,
            cvv = request.Cvv
        };

        var response = await _httpClient.PostAsJsonAsync("/payments", bankRequest, cancellationToken);

        // If bank returns error (400/503), throw exception
        response.EnsureSuccessStatusCode();

        var bankResponse = await response.Content.ReadFromJsonAsync<BankApiResponse>(cancellationToken);

        if (bankResponse == null)
        {
            throw new InvalidOperationException("Bank returned empty response");
        }

        return new BankPaymentResponse
        {
            Authorized = bankResponse.Authorized,
            AuthorizationCode = bankResponse.AuthorizationCode
        };
    }

    /// <summary>
    /// Internal model matching bank API response format (snake_case)
    /// </summary>
    private class BankApiResponse
    {
        public bool Authorized { get; set; }
        public string? Authorization_Code { get; set; }

        // Map to PascalCase property
        public string? AuthorizationCode => Authorization_Code;
    }
}
