using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentRepository _paymentsRepository;

    public PaymentsController(IPaymentRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment == null)
        {
            return NotFound();
        }

        var response = new PostPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };

        return Ok(response);
    }
}