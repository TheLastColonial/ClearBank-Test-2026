namespace ClearBank.DeveloperTest.Tests.Validators;

using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validators;
using FluentValidation;

public class MakePaymentRequestValidatorTests
{
    private IValidator<MakePaymentRequest> validator;
    private MakePaymentRequest request;
    private DateTime _fixed;

    public MakePaymentRequestValidatorTests()
    {
        _fixed = DateTime.UtcNow;

        validator = new MakePaymentRequestValidator();
        request = new MakePaymentRequest()
        {
            CreditorAccountNumber = "12345",
            DebtorAccountNumber = "6789",
            Amount = 20,
            PaymentDate = _fixed,
            PaymentScheme = PaymentScheme.Bacs
        };
    }

    [Fact]
    public void GivenValidRequest_WhenValidate_ThenReturnIsValid()
    {
        // Given

        // When
        var result = validator.Validate(request);

        // Then
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenCreditorAccountNumberEmpty_WhenValidating_ThenReturnIsError(string? number)
    {
        // Given
        request.CreditorAccountNumber = number!;

        // When
        var result = validator.Validate(request);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        var error = result.Errors[0];
        Assert.Equal("Creditor Account Number must be provided", error.ErrorMessage);
        Assert.Equal(nameof(MakePaymentRequest.CreditorAccountNumber), error.PropertyName);
    }

    [Fact]
    public void GivenCreditorAccountNumberNull_WhenValidating_ThenReturnIsError()
    {
        // Given
        request.CreditorAccountNumber = null;

        // When
        var result = validator.Validate(request);

        // Then
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        // todo assert both messages
    }

    [Fact]
    public void GivenCreditorAccontNumberLargerThan255_WhenValidating_ThenReturnError()
    {
        // Given
        request.CreditorAccountNumber = new string('A', 256);

        // When
        var result = validator.Validate(request);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        var error = result.Errors[0];
        Assert.Equal("Creditor Account Number must be less than 225", error.ErrorMessage);
        Assert.Equal(nameof(MakePaymentRequest.CreditorAccountNumber), error.PropertyName);
    }

    // todo complete testing of other fields in the same style
}
