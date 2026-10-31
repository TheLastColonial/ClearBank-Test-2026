using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Validation
{
    public class FasterPaymentsPaymentValidatorTests
    {
        [Fact]
        public void Validates_the_faster_payments_scheme()
        {
            Assert.Equal(PaymentScheme.FasterPayments, new FasterPaymentsPaymentValidator().Scheme);
        }

        [Theory]
        [InlineData(AllowedPaymentSchemes.FasterPayments)]
        [InlineData(AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Bacs)]
        public void Is_valid_when_the_account_allows_faster_payments_and_has_enough_balance(AllowedPaymentSchemes allowed)
        {
            Assert.True(IsValid(PaymentTestData.AnAccount(allowedPaymentSchemes: allowed, balance: 100m), amount: 10m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(AllowedPaymentSchemes.Bacs)]
        [InlineData(AllowedPaymentSchemes.Chaps)]
        [InlineData(AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.Chaps)]
        public void Is_invalid_when_the_account_does_not_allow_faster_payments(AllowedPaymentSchemes allowed)
        {
            Assert.False(IsValid(PaymentTestData.AnAccount(allowedPaymentSchemes: allowed, balance: 100m), amount: 10m));
        }

        [Theory]
        [InlineData(10, 10, true)]
        [InlineData(10.01, 10, true)]
        [InlineData(9.99, 10, false)]
        [InlineData(0, 10, false)]
        [InlineData(-5, 10, false)]
        [InlineData(0, 0, true)]
        public void Is_valid_only_when_the_balance_covers_the_amount(double balance, double amount, bool expected)
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, balance: (decimal)balance);

            Assert.Equal(expected, IsValid(account, amount: (decimal)amount));
        }

        [Theory]
        [InlineData(AccountStatus.Live)]
        [InlineData(AccountStatus.Disabled)]
        [InlineData(AccountStatus.InboundPaymentsOnly)]
        public void Account_status_does_not_affect_validity(AccountStatus status)
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, status: status);

            Assert.True(IsValid(account, amount: 10m));
        }

        [Fact]
        public void Reports_scheme_not_allowed_when_the_account_does_not_allow_faster_payments()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Bacs, balance: 100m);

            Assert.Equal(new[] { PaymentErrorCodes.SchemeNotAllowed }, ErrorCodes(account));
        }

        [Fact]
        public void Reports_insufficient_balance_when_the_balance_does_not_cover_the_amount()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, balance: 5m);

            Assert.Equal(new[] { PaymentErrorCodes.InsufficientBalance }, ErrorCodes(account, amount: 10m));
        }

        private static IEnumerable<string> ErrorCodes(Account account, decimal amount = 10m) =>
            new FasterPaymentsPaymentValidator()
                .Validate(new PaymentValidationModel(account, PaymentTestData.ARequest(PaymentScheme.FasterPayments, amount)))
                .Errors
                .Select(error => error.ErrorCode);

        private static bool IsValid(Account account, decimal amount) =>
            new FasterPaymentsPaymentValidator()
                .Validate(new PaymentValidationModel(account, PaymentTestData.ARequest(PaymentScheme.FasterPayments, amount)))
                .IsValid;
    }
}
