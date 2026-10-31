using System;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;

namespace ClearBank.DeveloperTest.Tests.TestData
{
    internal static class PaymentTestData
    {
        public static IPaymentSchemeValidator[] AllSchemeValidators() =>
            new IPaymentSchemeValidator[]
            {
                new BacsPaymentValidator(),
                new FasterPaymentsPaymentValidator(),
                new ChapsPaymentValidator()
            };

        public static IPaymentSchemeValidatorFactory AValidatorFactory() =>
            new PaymentSchemeValidatorFactory(AllSchemeValidators());

        public static Account AnAccount(
            AllowedPaymentSchemes allowedPaymentSchemes = AllowedPaymentSchemes.Bacs,
            decimal balance = 100m,
            AccountStatus status = AccountStatus.Live) =>
            new Account
            {
                AccountNumber = "12345678",
                AllowedPaymentSchemes = allowedPaymentSchemes,
                Balance = balance,
                Status = status
            };

        public static MakePaymentRequest ARequest(
            PaymentScheme paymentScheme = PaymentScheme.Bacs,
            decimal amount = 10m) =>
            new MakePaymentRequest
            {
                DebtorAccountNumber = "12345678",
                CreditorAccountNumber = "87654321",
                Amount = amount,
                PaymentDate = new DateTime(2026, 1, 1),
                PaymentScheme = paymentScheme
            };
    }
}
