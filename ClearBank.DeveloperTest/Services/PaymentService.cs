using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Exceptions;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validators;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NodaMoney;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClearBank.DeveloperTest.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IMakePaymentRequestValidator _requestValidator;
    private readonly IPaymentSchemesValidator _schemeValidator;

    public PaymentService(
        ILogger<PaymentService> logger, 
        IAccountRepository accountRepository, 
        IMakePaymentRequestValidator requestValidator,
        IPaymentSchemesValidator schemeValidator
    )
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _requestValidator = requestValidator;
        _schemeValidator = schemeValidator;
    }

    public async Task<MakePaymentResult> MakePayment(MakePaymentRequest request)
    {
        using (_logger.BeginScope(new Dictionary<string, object?>()
        {
            ["request.creditor-account-number"] = request?.CreditorAccountNumber,
            ["request.debtor-account-number"] = request?.DebtorAccountNumber,
            ["request.scheme"] = request?.PaymentScheme,
            // ["request.amount"] = request?.Amount // Useful but might be privacy violation
        }))
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);

                var result = _requestValidator.Validate(request);

                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }

                Account account = await _accountRepository.GetAccount(request.DebtorAccountNumber)
                    ?? throw new AccountNotFoundException(request.DebtorAccountNumber);

                result = _schemeValidator.Validate(new PaymentSchemeValidationModel(request, account));

                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }

                account.Debit(Money.PoundSterling(request.Amount)); // Assumed currency due to request contract
                await _accountRepository.UpdateAccount(account);

                // no credit to target account ?

                return new MakePaymentResult()
                {
                    Success = true
                };
            }
            catch (ValidationException validationException)
            {
                _logger.LogWarning(validationException, "Invalid request for this account");

                return new MakePaymentResult()
                {
                    // don't want to break contracts but would like to inform user of validation errors
                    Success = false
                };
            }
            catch (AccountNotFoundException accountNotFoundException)
            {
                _logger.LogWarning(accountNotFoundException, "Account Not Found");

                return new MakePaymentResult()
                {
                    // don't want to break contracts but would like to include to contact support
                    Success = false
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Payment Failed Unexpectedly");

                return new MakePaymentResult()
                {
                    // don't want to break contracts but would like to include actions to retry
                    Success = false
                };
            }
        }
    }
}
