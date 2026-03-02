namespace ClearBank.DeveloperTest.Tests.Services;

using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validators;
using FluentValidation.Results;
using MELT;
using Microsoft.Extensions.Logging;
using NodaMoney;
using NSubstitute.ExceptionExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PaymentServiceTests
{
    private PaymentService _service;
    private IAccountRepository _accountRepository;
    private IMakePaymentRequestValidator _requestValidator;
    private IPaymentSchemesValidator _schemeValidator;
    private DateTime _fixed;
    private Account _account;
    private MakePaymentRequest _request;

    private ILoggerFactory _factory;
    private ILogger<PaymentService> _logger;

    public PaymentServiceTests()
    {
        _fixed = DateTime.UtcNow;

        _request = new MakePaymentRequest()
        {
            CreditorAccountNumber = "12345",
            DebtorAccountNumber = "56768",
            Amount = 20,
            PaymentDate = _fixed,
            PaymentScheme = PaymentScheme.Bacs
        };

        _account = new Account("12345", Money.PoundSterling(20), AccountStatus.Live, AllowedPaymentSchemes.Bacs);

        _accountRepository = Substitute.For<IAccountRepository>();
        _accountRepository.GetAccount(Arg.Any<string>())
            .Returns(_account);

        _requestValidator = Substitute.For<IMakePaymentRequestValidator>();
        _requestValidator
            .Validate(Arg.Any<MakePaymentRequest>())
            .Returns(new ValidationResult());

        _schemeValidator = Substitute.For<IPaymentSchemesValidator>();
        _schemeValidator
            .Validate(Arg.Any<PaymentSchemeValidationModel>())
            .Returns(new ValidationResult());

        _factory = TestLoggerFactory.Create();
        _logger = _factory.CreateLogger<PaymentService>();

        _service = new PaymentService(_logger, _accountRepository, _requestValidator, _schemeValidator);
    }

    [Fact]
    public async Task GivenPaymentRequestIsNull_WhenMakePayment_ThenBeFailureResult()
    {
        // Given
        _request = null!;

        // When
        var result = await _service.MakePayment(_request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);

        var logs = _factory.GetTestLoggerSink().LogEntries.ToArray();
        Assert.Single(logs);
        Assert.Contains(logs, log =>
            log.Message == "Payment Failed Unexpectedly"
            && log.LogLevel == LogLevel.Error
            && log.Exception != null
        );
    }

    [Fact]
    public async Task GivenRequestValidatorThrows_WhenMakePayment_ThenFailResult()
    {
        // Given
        _requestValidator
            .Validate(Arg.Any<MakePaymentRequest>())
            .Returns(new ValidationResult() { Errors = new List<ValidationFailure>() { new ValidationFailure() } });

        // When
        var result = await _service.MakePayment(_request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);

        var logs = _factory.GetTestLoggerSink().LogEntries.ToArray();
        Assert.Single(logs);
        Assert.Contains(logs, log =>
            log.Message == "Invalid request for this account"
            && log.LogLevel == LogLevel.Warning
            && log.Exception != null
        );
    }

    [Fact]
    public async Task GivenSchemeValidatorThrows_WhenMakePayment_ThenFailResult()
    {
        // Given
        _schemeValidator
            .Validate(Arg.Any<PaymentSchemeValidationModel>())
            .Returns(new ValidationResult() { Errors = new List<ValidationFailure>() { new ValidationFailure() } });

        // When
        var result = await _service.MakePayment(_request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);

        var logs = _factory.GetTestLoggerSink().LogEntries.ToArray();
        Assert.Single(logs);
        Assert.Contains(logs, log =>
            log.Message == "Invalid request for this account"
            && log.LogLevel == LogLevel.Warning
            && log.Exception != null
        );
    }

    [Fact]
    public async Task GivenGetAccountNotFound_WhenMakePayment_ThenReturnFail()
    {
        // Given
        _accountRepository.GetAccount(Arg.Any<string>())
            .Returns((Account)null!);

        // When
        var result = await _service.MakePayment(_request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);

        var logs = _factory.GetTestLoggerSink().LogEntries.ToArray();
        Assert.Single(logs);
        Assert.Contains(logs, log =>
            log.Message == "Account Not Found"
            && log.LogLevel == LogLevel.Warning
            && log.Exception != null
        );
    }

    [Fact]
    public async Task GivenGetAccountThrowsException_WhenMakePayment_ThenReturnFail()
    {
        // Given
        _accountRepository.GetAccount(Arg.Any<string>())
            .Throws(new Exception("Boom!"));

        // When
        var result = await _service.MakePayment(_request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);

        var logs = _factory.GetTestLoggerSink().LogEntries.ToArray();
        Assert.Single(logs);
        Assert.Contains(logs, log =>
            log.Message == "Payment Failed Unexpectedly"
            && log.LogLevel == LogLevel.Error
            && log.Exception != null
        );
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(-200)]
    public async Task GivenAmountIsNegative_WhenMakePayment_ThenBeFailureResult(decimal amount)
    {
        // Given
        MakePaymentRequest request = new MakePaymentRequest()
        {
            CreditorAccountNumber = "123456",
            DebtorAccountNumber = "7890",
            Amount = amount,
            PaymentScheme = PaymentScheme.FasterPayments,
            PaymentDate = _fixed
        };

        // When
        var result = await _service.MakePayment(request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task GivenUpdateAccountThrowsException_WhenMakePayment_ThenReturnFail()
    {
        // Given
        _accountRepository.UpdateAccount(Arg.Any<Account>())
            .Throws(new Exception("Boom!"));

        // When
        var result = await _service.MakePayment(_request);

        // Then
        Assert.NotNull(result);
        Assert.False(result.Success);

        var logs = _factory.GetTestLoggerSink().LogEntries.ToArray();
        Assert.Single(logs);
        Assert.Contains(logs, log =>
            log.Message == "Payment Failed Unexpectedly"
            && log.LogLevel == LogLevel.Error
            && log.Exception != null
        );
    }

    [Theory]
    [InlineData("123456", "7890", 100, PaymentScheme.Bacs)]
    [InlineData("123456", "7890", 100, PaymentScheme.FasterPayments)]
    [InlineData("123456", "7890", 100, PaymentScheme.Chaps)]
    [InlineData("ABCD", "EFGHI", 0.1, PaymentScheme.Bacs)]
    [InlineData("$%^&*(", "&*()", 99999, PaymentScheme.Bacs)]
    public async Task GivenPaymentRequestIsValid_WhenMakePayment_ThenBeSuccessResult(string creditor, string debtor, decimal amount, PaymentScheme scheme)
    {
        // Given
        MakePaymentRequest request = new MakePaymentRequest()
        {
            CreditorAccountNumber = creditor,
            DebtorAccountNumber = debtor,
            Amount = amount,
            PaymentScheme = scheme,
            PaymentDate = _fixed
        };

        // When
        var result = await _service.MakePayment(request);

        // Then
        Assert.NotNull(result);
        Assert.True(result.Success);
    }
}
