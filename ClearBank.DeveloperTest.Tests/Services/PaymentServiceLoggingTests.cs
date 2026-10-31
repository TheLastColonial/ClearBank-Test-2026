using System;
using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Logging;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using Microsoft.Extensions.Logging;
using MELT;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services
{
    public class PaymentServiceLoggingTests
    {
        [Fact]
        public void Processed_payment_is_logged_as_information()
        {
            var entries = Run(PaymentTestData.AnAccount(AllowedPaymentSchemes.Bacs), PaymentTestData.ARequest(PaymentScheme.Bacs));

            var entry = Assert.Single(entries);
            Assert.Equal(LogLevel.Information, entry.LogLevel);
            Assert.Equal(PaymentServiceLog.PaymentProcessedId, entry.EventId.Id);
            Assert.Equal("PaymentProcessed", entry.EventId.Name);
        }

        [Fact]
        public void Missing_account_is_logged_as_a_warning()
        {
            var entries = Run(account: null, PaymentTestData.ARequest(PaymentScheme.Bacs));

            var entry = Assert.Single(entries);
            Assert.Equal(LogLevel.Warning, entry.LogLevel);
            Assert.Equal(PaymentServiceLog.AccountNotFoundId, entry.EventId.Id);
            Assert.Equal("AccountNotFound", entry.EventId.Name);
        }

        [Fact]
        public void Payment_for_a_scheme_with_no_rules_and_no_account_is_logged_as_a_missing_account()
        {
            var entries = Run(account: null, PaymentTestData.ARequest((PaymentScheme)99));

            var entry = Assert.Single(entries);
            Assert.Equal(PaymentServiceLog.AccountNotFoundId, entry.EventId.Id);
        }

        [Fact]
        public void Payment_rejected_for_a_scheme_the_account_does_not_allow_names_the_failed_rule()
        {
            var account = PaymentTestData.AnAccount(allowedPaymentSchemes: 0);

            var entry = Assert.Single(Run(account, PaymentTestData.ARequest(PaymentScheme.Bacs)));

            AssertRejectedWithFailedRule(entry, PaymentErrorCodes.SchemeNotAllowed);
        }

        [Fact]
        public void Faster_payment_rejected_for_insufficient_balance_names_the_failed_rule()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, balance: 5m);

            var entry = Assert.Single(Run(account, PaymentTestData.ARequest(PaymentScheme.FasterPayments, amount: 10m)));

            AssertRejectedWithFailedRule(entry, PaymentErrorCodes.InsufficientBalance);
        }

        [Fact]
        public void Chaps_payment_rejected_for_a_non_live_account_names_the_failed_rule()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Chaps, status: AccountStatus.Disabled);

            var entry = Assert.Single(Run(account, PaymentTestData.ARequest(PaymentScheme.Chaps)));

            AssertRejectedWithFailedRule(entry, PaymentErrorCodes.AccountNotLive);
        }

        [Fact]
        public void Every_failed_rule_is_named()
        {
            var account = PaymentTestData.AnAccount(allowedPaymentSchemes: 0, status: AccountStatus.Disabled);

            var entry = Assert.Single(Run(account, PaymentTestData.ARequest(PaymentScheme.Chaps)));

            var failedRules = FailedRules(entry);
            Assert.Contains(PaymentErrorCodes.SchemeNotAllowed, failedRules);
            Assert.Contains(PaymentErrorCodes.AccountNotLive, failedRules);
        }

        [Theory]
        [InlineData(PaymentScheme.Bacs)]
        [InlineData(PaymentScheme.FasterPayments)]
        [InlineData(PaymentScheme.Chaps)]
        public void Log_entries_carry_the_masked_accounts_and_scheme_in_scope(PaymentScheme scheme)
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.Bacs);

            var entry = Assert.Single(Run(account, PaymentTestData.ARequest(scheme)));

            var scope = ScopeValues(entry);
            Assert.Equal("****5678", scope["DebtorAccount"]);
            Assert.Equal("****4321", scope["CreditorAccount"]);
            Assert.Equal(scheme, scope["PaymentScheme"]);
        }

        [Fact]
        public void Full_account_numbers_amounts_and_balances_are_never_logged()
        {
            var account = PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, balance: 987.65m);
            var request = PaymentTestData.ARequest(PaymentScheme.FasterPayments, amount: 123.45m);
            var rejected = Run(PaymentTestData.AnAccount(AllowedPaymentSchemes.FasterPayments, balance: 1.11m), request);
            var processed = Run(account, request);

            var everythingLogged = rejected.Concat(processed).SelectMany(AllLoggedText).ToList();

            Assert.NotEmpty(everythingLogged);
            Assert.DoesNotContain(everythingLogged, text => text.Contains("12345678"));
            Assert.DoesNotContain(everythingLogged, text => text.Contains("87654321"));
            Assert.DoesNotContain(everythingLogged, text => text.Contains("123.45"));
            Assert.DoesNotContain(everythingLogged, text => text.Contains("987.65"));
            Assert.DoesNotContain(everythingLogged, text => text.Contains("1.11"));
        }

        [Fact]
        public void Failure_reading_the_account_is_logged_as_an_error_with_the_exception()
        {
            var failure = new InvalidOperationException("database unavailable");
            var store = Substitute.For<IAccountDataStore>();
            store.GetAccount(Arg.Any<string>()).Throws(failure);

            var entry = Assert.Single(Run(store, PaymentTestData.ARequest()));

            AssertFailedWith(entry, failure);
        }

        [Fact]
        public void Failure_updating_the_account_is_logged_as_an_error_with_the_exception()
        {
            var failure = new InvalidOperationException("database unavailable");
            var store = Substitute.For<IAccountDataStore>();
            store.GetAccount(Arg.Any<string>()).Returns(PaymentTestData.AnAccount());
            store.When(target => target.UpdateAccount(Arg.Any<Account>())).Do(_ => throw failure);

            var entry = Assert.Single(Run(store, PaymentTestData.ARequest()));

            AssertFailedWith(entry, failure);
        }

        private static void AssertFailedWith(LogEntry entry, Exception failure)
        {
            Assert.Equal(LogLevel.Error, entry.LogLevel);
            Assert.Equal(PaymentServiceLog.PaymentFailedId, entry.EventId.Id);
            Assert.Equal("PaymentFailed", entry.EventId.Name);
            Assert.Same(failure, entry.Exception);
            Assert.Equal("****5678", ScopeValues(entry)["DebtorAccount"]);
        }

        private static void AssertRejectedWithFailedRule(LogEntry entry, string expectedErrorCode)
        {
            Assert.Equal(LogLevel.Warning, entry.LogLevel);
            Assert.Equal(PaymentServiceLog.PaymentRejectedId, entry.EventId.Id);
            Assert.Equal("PaymentRejected", entry.EventId.Name);
            Assert.Contains(expectedErrorCode, FailedRules(entry));
        }

        private static IReadOnlyCollection<string> FailedRules(LogEntry entry) =>
            (IReadOnlyCollection<string>)entry.Properties.Single(property => property.Key == "FailedRules").Value;

        private static Dictionary<string, object> ScopeValues(LogEntry entry) =>
            entry.Scopes
                .SelectMany(scope => scope.Properties)
                .ToDictionary(property => property.Key, property => property.Value);

        private static IEnumerable<string> AllLoggedText(LogEntry entry) =>
            entry.Properties
                .Concat(entry.Scopes.SelectMany(scope => scope.Properties))
                .Select(property => property.Value?.ToString() ?? string.Empty)
                .Append(entry.Message ?? string.Empty);

        private static IReadOnlyList<LogEntry> Run(Account account, MakePaymentRequest request)
        {
            var store = Substitute.For<IAccountDataStore>();
            store.GetAccount(Arg.Any<string>()).Returns(account);

            return Run(store, request);
        }

        private static IReadOnlyList<LogEntry> Run(IAccountDataStore store, MakePaymentRequest request)
        {
            var loggerFactory = TestLoggerFactory.Create();

            var dataStoreFactory = Substitute.For<IAccountDataStoreFactory>();
            dataStoreFactory.Create().Returns(store);

            var service = new PaymentService(
                dataStoreFactory,
                PaymentTestData.AValidatorFactory(),
                loggerFactory.CreateLogger<PaymentService>());

            service.MakePayment(request);

            return loggerFactory.Sink.LogEntries.ToList();
        }
    }
}
