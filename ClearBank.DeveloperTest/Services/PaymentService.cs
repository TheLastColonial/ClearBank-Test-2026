using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Logging;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStoreFactory _dataStoreFactory;
        private readonly IPaymentSchemeValidatorFactory _validatorFactory;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IAccountDataStoreFactory dataStoreFactory,
            IPaymentSchemeValidatorFactory validatorFactory,
            ILogger<PaymentService> logger)
        {
            ArgumentNullException.ThrowIfNull(dataStoreFactory);
            ArgumentNullException.ThrowIfNull(validatorFactory);
            ArgumentNullException.ThrowIfNull(logger);

            _dataStoreFactory = dataStoreFactory;
            _validatorFactory = validatorFactory;
            _logger = logger;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            using (_logger.BeginScope(ScopeFor(request)))
            {
                try
                {
                    return Process(request);
                }
                catch (Exception exception)
                {
                    _logger.PaymentFailed(exception);
                    return new MakePaymentResult { Success = false };
                }
            }
        }

        private MakePaymentResult Process(MakePaymentRequest request)
        {
            var dataStore = _dataStoreFactory.Create();
            var account = dataStore.GetAccount(request.DebtorAccountNumber);

            if (account == null)
            {
                _logger.AccountNotFound();
                return new MakePaymentResult { Success = false };
            }

            var failures = Validate(account, request);

            if (failures.Any())
            {
                _logger.PaymentRejected(failures.Select(failure => failure.ErrorCode).ToList());
                return new MakePaymentResult { Success = false };
            }

            account.Balance -= request.Amount;
            dataStore.UpdateAccount(account);

            _logger.PaymentProcessed();
            return new MakePaymentResult { Success = true };
        }

        private IList<ValidationFailure> Validate(Account account, MakePaymentRequest request) =>
            _validatorFactory
                .Create(request.PaymentScheme)
                .Validate(new PaymentValidationModel(account, request))
                .Errors;

        private static Dictionary<string, object> ScopeFor(MakePaymentRequest request) =>
            new Dictionary<string, object>
            {
                ["DebtorAccount"] = AccountNumberMask.Mask(request.DebtorAccountNumber),
                ["CreditorAccount"] = AccountNumberMask.Mask(request.CreditorAccountNumber),
                ["PaymentScheme"] = request.PaymentScheme
            };
    }
}
