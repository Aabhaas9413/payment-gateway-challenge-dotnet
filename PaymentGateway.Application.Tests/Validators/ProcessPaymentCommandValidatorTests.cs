using FluentAssertions;
using FluentValidation.TestHelper;
using PaymentGateway.Application.Commands;
using PaymentGateway.Application.Tests.Helpers;
using PaymentGateway.Application.Validators;

namespace PaymentGateway.Application.Tests.Validators;

public class ProcessPaymentCommandValidatorTests
{
    private readonly ProcessPaymentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCardNumber_ShouldPass()
    {
        var command = TestDataBuilder.CreateValidCommand();
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_CardNumberNullOrEmpty_ShouldFail(string? cardNumber)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = cardNumber!;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("*required*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123")]
    public void Validate_CardNumberTooShort_ShouldFail(string cardNumber)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = cardNumber;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Fact]
    public void Validate_CardNumberTooLong_ShouldFail()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = "12345678901234567890";
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData("123456789012345A")]
    [InlineData("1234-5678-9012-3456")]
    [InlineData("1234 5678 9012 3456")]
    public void Validate_CardNumberNonNumeric_ShouldFail(string cardNumber)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = cardNumber;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData("12345678901234")]
    [InlineData("123456789012345")]
    [InlineData("1234567890123456")]
    [InlineData("12345678901234567")]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456789")]
    public void Validate_CardNumberValidLengths_ShouldPass(string cardNumber)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.CardNumber = cardNumber;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Validate_ValidExpiryMonth_ShouldPass(int month)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryMonth = month;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryMonth);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ExpiryMonthLessThan1_ShouldFail(int month)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryMonth = month;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth);
    }

    [Theory]
    [InlineData(13)]
    [InlineData(99)]
    public void Validate_ExpiryMonthGreaterThan12_ShouldFail(int month)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryMonth = month;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth);
    }

    [Fact]
    public void Validate_FutureExpiryYear_ShouldPass()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryYear = DateTime.Now.Year + 2;
        command.ExpiryMonth = 1;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryYear);
    }

    [Fact]
    public void Validate_PastExpiryYear_ShouldFail()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryYear = DateTime.Now.Year - 1;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear);
    }

    [Fact]
    public void Validate_CurrentYearPastMonth_ShouldFail()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryYear = DateTime.Now.Year;
        command.ExpiryMonth = DateTime.Now.Month - 1;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear);
    }

    [Fact]
    public void Validate_CurrentYearFutureMonth_ShouldPass()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.ExpiryYear = DateTime.Now.Year;
        command.ExpiryMonth = DateTime.Now.Month < 12 ? DateTime.Now.Month + 1 : 12;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryYear);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("EUR")]
    public void Validate_ValidCurrency_ShouldPass(string currency)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Currency = currency;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_CurrencyNullOrEmpty_ShouldFail(string? currency)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Currency = currency!;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Validate_CurrencyInvalidLength_ShouldFail(string currency)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Currency = currency;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("ABC")]
    public void Validate_InvalidCurrencyCode_ShouldFail(string currency)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Currency = currency;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Validate_PositiveAmount_ShouldPass(int amount)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Amount = amount;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_ZeroAmount_ShouldFail()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Amount = 0;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_NegativeAmount_ShouldFail()
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Amount = -100;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    public void Validate_ValidCvv_ShouldPass(string cvv)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Cvv = cvv;
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Cvv);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_CvvNullOrEmpty_ShouldFail(string? cvv)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Cvv = cvv!;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Cvv);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    public void Validate_CvvInvalidLength_ShouldFail(string cvv)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Cvv = cvv;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Cvv);
    }

    [Theory]
    [InlineData("12A")]
    [InlineData("1 3")]
    public void Validate_CvvNonNumeric_ShouldFail(string cvv)
    {
        var command = TestDataBuilder.CreateValidCommand();
        command.Cvv = cvv;
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Cvv);
    }
}
