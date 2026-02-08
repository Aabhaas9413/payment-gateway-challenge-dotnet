using FluentAssertions;
using FluentValidation.TestHelper;
using PaymentGateway.Application.Queries;

namespace PaymentGateway.Application.Tests.Validators;

public class GetPaymentQueryValidatorTests
{
    private readonly GetPaymentQueryValidator _validator;

    public GetPaymentQueryValidatorTests()
    {
        _validator = new GetPaymentQueryValidator();
    }

    [Fact]
    public void Validate_WhenValidGuidProvided_ThenValidationSucceeds()
    {
        var query = new GetPaymentQuery { Id = Guid.NewGuid() };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenEmptyGuidProvided_ThenValidationFails()
    {
        var query = new GetPaymentQuery { Id = Guid.Empty };

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Payment ID cannot be empty");
    }

    [Fact]
    public void Validate_WhenEmptyGuid_ThenContainsSpecificErrorMessage()
    { 
        var query = new GetPaymentQuery { Id = Guid.Empty };
        var result = _validator.TestValidate(query);
        var errors = result.Errors.Select(e => e.ErrorMessage).ToList();

        errors.Should().Contain("Payment ID cannot be empty");
    }
}
