using FluentAssertions;
using Moq;
using PaymentGateway.Application.Handlers;
using PaymentGateway.Application.Queries;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application.Tests.Handlers;

public class GetPaymentQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockRepository;
    private readonly GetPaymentQueryHandler _handler;

    public GetPaymentQueryHandlerTests()
    {
        _mockRepository = new Mock<IPaymentRepository>();
        _handler = new GetPaymentQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ThenReturnsPaymentDetails()
    {
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            CardNumberLastFour = 9012,
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 10000,
            Status = PaymentStatus.Authorized
        };
        _mockRepository.Setup(x => x.Get(paymentId)).Returns(existingPayment);

   
        var query = new GetPaymentQuery { Id = paymentId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(paymentId);
        result.Status.Should().Be(PaymentStatus.Authorized);
        result.Amount.Should().Be(10000);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ThenReturnsOnlyLastFourCardDigits()
    {
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            CardNumberLastFour = 9012,
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 10000,
            Status = PaymentStatus.Authorized
        };
        _mockRepository.Setup(x => x.Get(paymentId)).Returns(existingPayment);

        var query = new GetPaymentQuery { Id = paymentId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.CardNumberLastFour.Should().Be(9012);
    }

    [Fact]
    public async Task Handle_WhenPaymentDoesNotExist_ThenReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.Get(nonExistentId)).Returns((Payment?)null);

        var query = new GetPaymentQuery { Id = nonExistentId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ThenAllPropertiesAreCorrectlyMapped()
    {
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            CardNumberLastFour = 5678,
            ExpiryMonth = 6,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 25000,
            Status = PaymentStatus.Declined
        };
        _mockRepository.Setup(x => x.Get(paymentId)).Returns(existingPayment);

        var query = new GetPaymentQuery { Id = paymentId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(paymentId);
        result.CardNumberLastFour.Should().Be(5678);
        result.ExpiryMonth.Should().Be(6);
        result.ExpiryYear.Should().Be(2026);
        result.Currency.Should().Be("GBP");
        result.Amount.Should().Be(25000);
        result.Status.Should().Be(PaymentStatus.Declined);
    }

    [Fact]
    public async Task Handle_WhenCalledMultipleTimes_ThenRetrievesFromRepositoryEachTime()
    {
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            CardNumberLastFour = 1234,
            ExpiryMonth = 3,
            ExpiryYear = 2025,
            Currency = "EUR",
            Amount = 5000,
            Status = PaymentStatus.Authorized
        };
        _mockRepository.Setup(x => x.Get(paymentId)).Returns(existingPayment);

        var query = new GetPaymentQuery { Id = paymentId };
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        _mockRepository.Verify(x => x.Get(paymentId), Times.Exactly(3));
    }
}
