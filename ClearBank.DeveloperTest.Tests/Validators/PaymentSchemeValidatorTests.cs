using System.Collections.Generic;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validators;
using NodaMoney;

namespace ClearBank.DeveloperTest.Tests.Validators;

public class PaymentSchemeValidatorTests
{
    private PaymentSchemeValidator _validator;
    private Account _account;
    private MakePaymentRequest _request;
    private PaymentSchemeValidationModel _validationModel;

    private DateTime _fixed;

    public PaymentSchemeValidatorTests()
    {
        _fixed = DateTime.UtcNow;

        _account = new Account("123456", Money.PoundSterling(20), AccountStatus.Live, AllowedPaymentSchemes.Bacs);       

        _request = new MakePaymentRequest()
        {
            CreditorAccountNumber = "12345",
            DebtorAccountNumber = "6789",
            Amount = 20,
            PaymentDate = _fixed,
            PaymentScheme = PaymentScheme.Bacs
        };

        _validationModel = new PaymentSchemeValidationModel(_request, _account);

        _validator = new PaymentSchemeValidator(new List<ISchemeValidator>()
        {
            new BacsValidator(),
            new FasterPaymentsValidator(),
            new ChapsValidator(),
        });
    }

    [Fact]
    public void GivenRequestSchemeIsUnknown_WhenValidating_ThenReturnError()
    {
        // Given
        _request.PaymentScheme = PaymentScheme.Unknown;

        // When
        var result = _validator.Validate(_validationModel);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Supported Payment Scheme not provided", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void GivenAccountIsUnknown_WhenValidating_ThenReturnError()
    {
        // Given
        _account = null;
        _validationModel = new PaymentSchemeValidationModel(_request, _account);

        // When
        var result = _validator.Validate(_validationModel);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Debtor Account required to validate payment", result.Errors[0].ErrorMessage);
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs, AllowedPaymentSchemes.FasterPayments, "BACS not enabled for this account")]
    [InlineData(PaymentScheme.Bacs, AllowedPaymentSchemes.Chaps, "BACS not enabled for this account")]
    [InlineData(PaymentScheme.FasterPayments, AllowedPaymentSchemes.Bacs, "Faster Payments not enabled for this account")]
    [InlineData(PaymentScheme.FasterPayments, AllowedPaymentSchemes.Chaps, "Faster Payments not enabled for this account")]
    [InlineData(PaymentScheme.Chaps, AllowedPaymentSchemes.Bacs, "CHAPS not enabled for this account")]
    [InlineData(PaymentScheme.Chaps, AllowedPaymentSchemes.FasterPayments, "CHAPS not enabled for this account")]
    public void GivenAccountAndRequestDoNotMatch_WhenValidating_ThenReturnError(PaymentScheme scheme, AllowedPaymentSchemes allowed, string messgae)
    {
        // Given
        _account = new Account("123456", Money.PoundSterling(20), AccountStatus.Live, allowed);
        _request.PaymentScheme = scheme;
        _validationModel = new PaymentSchemeValidationModel(_request, _account);

        // When
        var result = _validator.Validate(_validationModel);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains(messgae, result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void GivenFasterPaymentHasInsuffientFunds_WhenValidating_ThenReturnError()
    {
        // Given
        _account = new Account("123456", Money.PoundSterling(0), AccountStatus.Live, AllowedPaymentSchemes.FasterPayments);
        _request.PaymentScheme = PaymentScheme.FasterPayments;
        _validationModel = new PaymentSchemeValidationModel(_request, _account);

        // When
        var result = _validator.Validate(_validationModel);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Balance on account is less than requested amount for Faster payments", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void GivenChapsPaymentAccountIsNotLive_WhenValidating_ThenReturnError()
    {
        // Given
        _account = new Account("123456", Money.PoundSterling(20), AccountStatus.InboundPaymentsOnly, AllowedPaymentSchemes.Chaps);
        _request.PaymentScheme = PaymentScheme.Chaps;
        _validationModel = new PaymentSchemeValidationModel(_request, _account);

        // When
        var result = _validator.Validate(_validationModel);

        // Then
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Account must be Live for CHAPS payments", result.Errors[0].ErrorMessage);
    }
}
