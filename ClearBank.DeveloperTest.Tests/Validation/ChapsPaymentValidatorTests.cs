using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Validation
{
    public class ChapsPaymentValidatorTests
    {
        [Fact]
        public void Validates_the_chaps_scheme()
        {
            Assert.Equal(PaymentScheme.Chaps, new ChapsPaymentValidator().Scheme);
        }

        [Theory]
        [InlineData(AllowedPaymentSchemes.Chaps)]
        [InlineData(AllowedPaymentSchemes.Chaps | AllowedPaymentSchemes.Bacs)]
        public void Is_valid_when_the_account_allows_chaps_and_is_live(AllowedPaymentSchemes allowed)
        {
            Assert.True(IsValid(PaymentTestData.AnAccount(allowedPaymentSchemes: allowed, status: AccountStatus.Live)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(AllowedPaymentSchemes.Bacs)]
        [InlineData(AllowedPaymentSchemes.FasterPayments)]
        [InlineData(AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments)]
        public void Is_invalid_when_the_account_does_not_allow_chaps(AllowedPaymentSchemes allowed)
        {
            Assert.False(IsValid(PaymentTestData.AnAccount(allowedPaymentSchemes: allowed, status: AccountStatus.Live)));
        }

        [Theory]
        [InlineData(AccountStatus.Disabled)]
        [InlineData(AccountStatus.InboundPaymentsOnly)]
        public void Is_invalid_when_the_account_is_not_live(AccountStatus status)
        {
            Assert.False(IsValid(PaymentTestData.AnAccount(AllowedPaymentSchemes.Chaps, status: status)));
        }

        [Theory]
        [InlineData(-50)]
        [InlineData(0)]
        [InlineData(5)]
        public void Account_balance_does_not_affect_validity(int balance)
        {
            Assert.True(IsValid(PaymentTestData.AnAccount(AllowedPaymentSchemes.Chaps, balance: balance)));
        }

        [Fact]
        public void Reports_scheme_not_allowed_when_the_account_does_not_allow_chaps()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Bacs, status: AccountStatus.Live);

            Assert.Equal(new[] { PaymentErrorCodes.SchemeNotAllowed }, ErrorCodes(account));
        }

        [Fact]
        public void Reports_account_not_live_when_the_account_is_not_live()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Chaps, status: AccountStatus.Disabled);

            Assert.Equal(new[] { PaymentErrorCodes.AccountNotLive }, ErrorCodes(account));
        }

        [Fact]
        public void Reports_every_failed_rule()
        {
            var account = PaymentTestData.AnAccount(allowedPaymentSchemes: 0, status: AccountStatus.Disabled);

            var errorCodes = ErrorCodes(account).ToList();

            Assert.Contains(PaymentErrorCodes.SchemeNotAllowed, errorCodes);
            Assert.Contains(PaymentErrorCodes.AccountNotLive, errorCodes);
        }

        private static IEnumerable<string> ErrorCodes(Account account, decimal amount = 10m) =>
            new ChapsPaymentValidator()
                .Validate(new PaymentValidationModel(account, PaymentTestData.ARequest(PaymentScheme.Chaps, amount)))
                .Errors
                .Select(error => error.ErrorCode);

        private static bool IsValid(Account account) =>
            new ChapsPaymentValidator()
                .Validate(new PaymentValidationModel(account, PaymentTestData.ARequest(PaymentScheme.Chaps, amount: 10m)))
                .IsValid;
    }
}
