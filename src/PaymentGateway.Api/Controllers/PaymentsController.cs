using MediatR;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Commands;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentRepository _paymentsRepository;
    private readonly IMediator _mediator;

    public PaymentsController(IPaymentRepository paymentsRepository, IMediator mediator)
    {
        _paymentsRepository = paymentsRepository;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync(PostPaymentRequest request)
    {
        var command = new ProcessPaymentCommand
        {
            Id = Guid.NewGuid(),
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };

        var result = await _mediator.Send(command);

        var response = new PostPaymentResponse
        {
            Id = result.Id,
            Status = result.Status,
            CardNumberLastFour = result.CardNumberLastFour,
            ExpiryMonth = result.ExpiryMonth,
            ExpiryYear = result.ExpiryYear,
            Currency = result.Currency,
            Amount = result.Amount
        };

        return Ok(response);
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