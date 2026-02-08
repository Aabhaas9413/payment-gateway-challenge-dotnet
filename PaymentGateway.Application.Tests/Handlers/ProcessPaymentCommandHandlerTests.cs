using FluentAssertions;
using Moq;
using PaymentGateway.Application.Commands;
using PaymentGateway.Application.Handlers;
using PaymentGateway.Application.Tests.Helpers;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Tests.Handlers;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockRepository;
    private readonly Mock<IBankClient> _mockBankClient;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _mockRepository = new Mock<IPaymentRepository>();
        _mockBankClient = new Mock<IBankClient>();
        _handler = new ProcessPaymentCommandHandler(_mockRepository.Object, _mockBankClient.Object);
        
        // Default bank response for all tests - returns authorized
        SetupBankAuthorized();
    }

    private void SetupBankAuthorized(bool authorized = true)
    {
        _mockBankClient.Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BankPaymentResponse { Authorized = authorized, AuthorizationCode = authorized ? "test-auth-code" : null });
    }

    [Fact]
    public async Task Handle_FirstRequest_ShouldProcessPaymentAndSave()
    {
        var command = TestDataBuilder.CreateValidCommand();
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(command.Id);
        _mockRepository.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateRequest_ShouldReturnExistingPaymentWithoutSaving()
    {
        var command = TestDataBuilder.CreateValidCommand();
        var existingPayment = new Payment
        {
            Id = command.Id,
            CardNumberLastFour = 9012,
            ExpiryMonth = command.ExpiryMonth,
            ExpiryYear = command.ExpiryYear,
            Currency = command.Currency,
            Amount = command.Amount,
            Status = PaymentStatus.Authorized
        };

        _mockRepository.Setup(x => x.Get(command.Id)).Returns(existingPayment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(existingPayment.Id);
        result.CardNumberLastFour.Should().Be(existingPayment.CardNumberLastFour);
        result.Status.Should().Be(existingPayment.Status);
        _mockRepository.Verify(x => x.Add(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleDuplicateRequests_ShouldReturnSameResult()
    {
        var command = TestDataBuilder.CreateValidCommand();
        var existingPayment = TestDataBuilder.CreateValidPayment();
        existingPayment.Id = command.Id;

        _mockRepository.Setup(x => x.Get(command.Id)).Returns(existingPayment);

        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);
        var result3 = await _handler.Handle(command, CancellationToken.None);

        result1.Should().BeEquivalentTo(result2);
        result2.Should().BeEquivalentTo(result3);
        _mockRepository.Verify(x => x.Add(It.IsAny<Payment>()), Times.Never);
    }

    [Theory]
    [InlineData("1234567890123456", 3456)]
    [InlineData("4532123456789012", 9012)]
    [InlineData("37828224631000", 1000)]
    [InlineData("1234567890123456789", 6789)]
    public async Task Handle_ValidCardNumber_ShouldStoreLast4DigitsOnly(string fullCardNumber, int expectedLast4)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = fullCardNumber;
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        Payment? savedPayment = null;
        _mockRepository.Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => savedPayment = p);

        var result = await _handler.Handle(command, CancellationToken.None);

        savedPayment.Should().NotBeNull();
        savedPayment!.CardNumberLastFour.Should().Be(expectedLast4);
        result.CardNumberLastFour.Should().Be(expectedLast4);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldNotStoreFullCardNumber()
    {
        var command = TestDataBuilder.CreateValidCommand();
        var fullCardNumber = command.CardNumber;
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        Payment? savedPayment = null;
        _mockRepository.Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => savedPayment = p);

        await _handler.Handle(command, CancellationToken.None);

        savedPayment.Should().NotBeNull();
        savedPayment!.CardNumberLastFour.ToString().Length.Should().Be(4);
        fullCardNumber.Should().EndWith(savedPayment.CardNumberLastFour.ToString());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldNotReturnFullCardNumber()
    {
        var command = TestDataBuilder.CreateValidCommand();
        var fullCardNumber = command.CardNumber;
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.CardNumberLastFour.ToString().Length.Should().Be(4);
        fullCardNumber.Should().EndWith(result.CardNumberLastFour.ToString());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePaymentWithCorrectData()
    {
        var command = TestDataBuilder.CreateValidCommand();
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        Payment? savedPayment = null;
        _mockRepository.Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => savedPayment = p);

        await _handler.Handle(command, CancellationToken.None);

        savedPayment.Should().NotBeNull();
        savedPayment!.Id.Should().Be(command.Id);
        savedPayment.ExpiryMonth.Should().Be(command.ExpiryMonth);
        savedPayment.ExpiryYear.Should().Be(command.ExpiryYear);
        savedPayment.Currency.Should().Be(command.Currency);
        savedPayment.Amount.Should().Be(command.Amount);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetStatusToAuthorized()
    {
        var command = TestDataBuilder.CreateValidCommand();
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        Payment? savedPayment = null;
        _mockRepository.Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => savedPayment = p);

        await _handler.Handle(command, CancellationToken.None);

        savedPayment.Should().NotBeNull();
        savedPayment!.Status.Should().Be(PaymentStatus.Authorized);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSaveToRepository()
    {
        var command = TestDataBuilder.CreateValidCommand();
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        await _handler.Handle(command, CancellationToken.None);

        _mockRepository.Verify(x => x.Add(It.Is<Payment>(p => 
            p.Id == command.Id &&
            p.Currency == command.Currency &&
            p.Amount == command.Amount
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnResultMatchingSavedPayment()
    {
        var command = TestDataBuilder.CreateValidCommand();
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        Payment? savedPayment = null;
        _mockRepository.Setup(x => x.Add(It.IsAny<Payment>()))
            .Callback<Payment>(p => savedPayment = p);

        var result = await _handler.Handle(command, CancellationToken.None);

        savedPayment.Should().NotBeNull();
        result.Id.Should().Be(savedPayment!.Id);
        result.CardNumberLastFour.Should().Be(savedPayment.CardNumberLastFour);
        result.ExpiryMonth.Should().Be(savedPayment.ExpiryMonth);
        result.ExpiryYear.Should().Be(savedPayment.ExpiryYear);
        result.Currency.Should().Be(savedPayment.Currency);
        result.Amount.Should().Be(savedPayment.Amount);
        result.Status.Should().Be(savedPayment.Status);
    }

    [Fact]
    public async Task Handle_MinimumCardLength_ShouldExtractLast4Correctly()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = "12345678901234";
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.CardNumberLastFour.Should().Be(1234);
    }

    [Fact]
    public async Task Handle_MaximumCardLength_ShouldExtractLast4Correctly()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = "1234567890123456789";
        _mockRepository.Setup(x => x.Get(command.Id)).Returns((Payment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.CardNumberLastFour.Should().Be(6789);
    }
}
