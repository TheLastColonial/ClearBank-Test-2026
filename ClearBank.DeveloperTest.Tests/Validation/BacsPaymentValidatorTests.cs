using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Validation
{
    public class BacsPaymentValidatorTests
    {
        [Fact]
        public void Validates_the_bacs_scheme()
        {
            Assert.Equal(PaymentScheme.Bacs, new BacsPaymentValidator().Scheme);
        }

        [Theory]
        [InlineData(AllowedPaymentSchemes.Bacs)]
        [InlineData(AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.Chaps)]
        [InlineData(AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps)]
        public void Is_valid_when_the_account_allows_bacs(AllowedPaymentSchemes allowed)
        {
            Assert.True(IsValid(PaymentTestData.AnAccount(allowedPaymentSchemes: allowed)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(AllowedPaymentSchemes.FasterPayments)]
        [InlineData(AllowedPaymentSchemes.Chaps)]
        [InlineData(AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps)]
        public void Is_invalid_when_the_account_does_not_allow_bacs(AllowedPaymentSchemes allowed)
        {
            Assert.False(IsValid(PaymentTestData.AnAccount(allowedPaymentSchemes: allowed)));
        }

        [Theory]
        [InlineData(AccountStatus.Live)]
        [InlineData(AccountStatus.Disabled)]
        [InlineData(AccountStatus.InboundPaymentsOnly)]
        public void Account_status_does_not_affect_validity(AccountStatus status)
        {
            Assert.True(IsValid(PaymentTestData.AnAccount(status: status)));
        }

        [Theory]
        [InlineData(-50)]
        [InlineData(0)]
        [InlineData(5)]
        public void Account_balance_does_not_affect_validity(int balance)
        {
            Assert.True(IsValid(PaymentTestData.AnAccount(balance: balance)));
        }

        [Fact]
        public void Reports_scheme_not_allowed_when_the_account_does_not_allow_bacs()
        {
            var account = PaymentTestData.AnAccount(allowedPaymentSchemes: AllowedPaymentSchemes.Chaps);

            Assert.Equal(new[] { PaymentErrorCodes.SchemeNotAllowed }, ErrorCodes(account));
        }

        private static IEnumerable<string> ErrorCodes(Account account, decimal amount = 10m) =>
            new BacsPaymentValidator()
                .Validate(new PaymentValidationModel(account, PaymentTestData.ARequest(PaymentScheme.Bacs, amount)))
                .Errors
                .Select(error => error.ErrorCode);

        private static bool IsValid(Account account) =>
            new BacsPaymentValidator()
                .Validate(new PaymentValidationModel(account, PaymentTestData.ARequest(amount: 10m)))
                .IsValid;
    }
}
