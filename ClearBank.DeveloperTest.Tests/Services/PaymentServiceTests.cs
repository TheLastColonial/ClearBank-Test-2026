using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services
{
    public class PaymentServiceTests
    {
        [Fact]
        public void Payment_is_rejected_when_the_account_does_not_exist()
        {
            var scenario = ScenarioFor(account: null);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs));

            Assert.False(result.Success);
            scenario.Store.DidNotReceiveWithAnyArgs().UpdateAccount(default);
        }

        [Fact]
        public void Payment_is_rejected_when_the_account_does_not_exist_for_a_scheme_with_no_rules()
        {
            var scenario = ScenarioFor(account: null);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest((PaymentScheme)99));

            Assert.False(result.Success);
            scenario.Store.DidNotReceiveWithAnyArgs().UpdateAccount(default);
        }

        [Theory]
        [InlineData(PaymentScheme.Bacs)]
        [InlineData(PaymentScheme.FasterPayments)]
        [InlineData(PaymentScheme.Chaps)]
        public void Payment_is_rejected_when_the_account_allows_no_payment_schemes(PaymentScheme scheme)
        {
            var account = PaymentTestData.AnAccount(allowedPaymentSchemes: 0);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(scheme));

            Assert.False(result.Success);
            Assert.Equal(100m, account.Balance);
            scenario.Store.DidNotReceiveWithAnyArgs().UpdateAccount(default);
        }

        [Fact]
        public void Account_is_looked_up_by_the_debtor_account_number()
        {
            var scenario = ScenarioFor(PaymentTestData.AnAccount());

            scenario.Service.MakePayment(PaymentTestData.ARequest());

            scenario.Store.Received(1).GetAccount("12345678");
        }

        [Fact]
        public void Bacs_payment_debits_the_account_and_updates_the_store()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Bacs, balance: 100m);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs, amount: 30m));

            Assert.True(result.Success);
            Assert.Equal(70m, account.Balance);
            scenario.Store.Received(1).UpdateAccount(account);
        }

        [Fact]
        public void Bacs_payment_can_take_the_balance_below_zero()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Bacs, balance: 5m, status: AccountStatus.Disabled);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs, amount: 30m));

            Assert.True(result.Success);
            Assert.Equal(-25m, account.Balance);
            scenario.Store.Received(1).UpdateAccount(account);
        }

        [Theory]
        [InlineData(100, 100, true, 0)]
        [InlineData(100, 99.99, true, 0.01)]
        [InlineData(99.99, 100, false, 99.99)]
        public void FasterPayments_payment_requires_the_balance_to_cover_the_amount(
            double balance, double amount, bool expectedSuccess, double expectedBalance)
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, balance: (decimal)balance);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(
                PaymentTestData.ARequest(PaymentScheme.FasterPayments, (decimal)amount));

            Assert.Equal(expectedSuccess, result.Success);
            Assert.Equal((decimal)expectedBalance, account.Balance);
            AssertUpdated(scenario.Store, account, expectedSuccess);
        }

        [Theory]
        [InlineData(AccountStatus.Live, true, 90)]
        [InlineData(AccountStatus.Disabled, false, 100)]
        [InlineData(AccountStatus.InboundPaymentsOnly, false, 100)]
        public void Chaps_payment_requires_a_live_account(AccountStatus status, bool expectedSuccess, double expectedBalance)
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Chaps, balance: 100m, status: status);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Chaps, amount: 10m));

            Assert.Equal(expectedSuccess, result.Success);
            Assert.Equal((decimal)expectedBalance, account.Balance);
            AssertUpdated(scenario.Store, account, expectedSuccess);
        }

        [Fact]
        public void Payment_for_a_scheme_with_no_rules_is_accepted_and_debited()
        {
            var account = PaymentTestData.AnAccount(allowedPaymentSchemes: 0, balance: 100m);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest((PaymentScheme)99, amount: 40m));

            Assert.True(result.Success);
            Assert.Equal(60m, account.Balance);
            scenario.Store.Received(1).UpdateAccount(account);
        }

        [Fact]
        public void Only_the_rules_for_the_requested_scheme_are_applied()
        {
            var account = PaymentTestData.AnAccount(
                AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
                balance: 0m,
                status: AccountStatus.Disabled);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs, amount: 10m));

            Assert.True(result.Success);
        }

        [Fact]
        public void Payment_fails_without_throwing_when_the_data_store_cannot_be_created()
        {
            var scenario = ScenarioFor(PaymentTestData.AnAccount());
            scenario.Factory.Create().Throws(new InvalidOperationException());

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest());

            Assert.False(result.Success);
        }

        [Fact]
        public void Payment_fails_without_throwing_when_the_account_cannot_be_read()
        {
            var scenario = ScenarioFor(PaymentTestData.AnAccount());
            scenario.Store.GetAccount(Arg.Any<string>()).Throws(new InvalidOperationException());

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest());

            Assert.False(result.Success);
        }

        [Fact]
        public void Payment_fails_without_throwing_when_the_account_cannot_be_updated()
        {
            var scenario = ScenarioFor(PaymentTestData.AnAccount());
            scenario.Store.When(store => store.UpdateAccount(Arg.Any<Account>())).Do(_ => throw new InvalidOperationException());

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest());

            Assert.False(result.Success);
        }

        private static void AssertUpdated(IAccountDataStore store, Account account, bool expected)
        {
            if (expected)
            {
                store.Received(1).UpdateAccount(account);
                return;
            }

            store.DidNotReceiveWithAnyArgs().UpdateAccount(default);
        }

        private static Scenario ScenarioFor(Account account)
        {
            var store = Substitute.For<IAccountDataStore>();
            store.GetAccount(Arg.Any<string>()).Returns(account);

            var factory = Substitute.For<IAccountDataStoreFactory>();
            factory.Create().Returns(store);

            var service = new PaymentService(factory, PaymentTestData.AValidatorFactory(), NullLogger<PaymentService>.Instance);

            return new Scenario(service, store, factory);
        }

        private sealed record Scenario(PaymentService Service, IAccountDataStore Store, IAccountDataStoreFactory Factory);
    }
}
