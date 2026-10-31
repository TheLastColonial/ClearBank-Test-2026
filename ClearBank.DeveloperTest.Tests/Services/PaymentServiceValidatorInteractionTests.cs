using System;
using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services
{
    public class PaymentServiceValidatorInteractionTests
    {
        [Theory]
        [InlineData(PaymentScheme.Bacs)]
        [InlineData(PaymentScheme.FasterPayments)]
        [InlineData(PaymentScheme.Chaps)]
        public void Only_the_validator_for_the_requested_scheme_is_consulted(PaymentScheme scheme)
        {
            var scenario = ScenarioFor(PaymentTestData.AnAccount());

            scenario.Service.MakePayment(PaymentTestData.ARequest(scheme));

            scenario.Validators
                .Where(validator => validator.Scheme != scheme)
                .ToList()
                .ForEach(validator => validator.DidNotReceive().Validate(Arg.Any<PaymentValidationModel>()));
            scenario.ValidatorFor(scheme).Received(1).Validate(Arg.Any<PaymentValidationModel>());
        }

        [Fact]
        public void The_validator_receives_the_loaded_account_and_the_request()
        {
            var account = PaymentTestData.AnAccount();
            var request = PaymentTestData.ARequest(PaymentScheme.Chaps);
            var scenario = ScenarioFor(account);

            scenario.Service.MakePayment(request);

            scenario.ValidatorFor(PaymentScheme.Chaps).Received(1).Validate(
                Arg.Is<PaymentValidationModel>(model => model.Account == account && model.Request == request));
        }

        [Fact]
        public void A_failing_validator_rejects_the_payment_without_touching_the_account()
        {
            var account = PaymentTestData.AnAccount(balance: 100m);
            var scenario = ScenarioFor(account);
            scenario.ValidatorFor(PaymentScheme.Bacs).Validate(Arg.Any<PaymentValidationModel>())
                .Returns(new ValidationResult(new[] { new ValidationFailure("Account", "invalid") { ErrorCode = "Any" } }));

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs, amount: 10m));

            Assert.False(result.Success);
            Assert.Equal(100m, account.Balance);
            scenario.Store.DidNotReceiveWithAnyArgs().UpdateAccount(default);
        }

        [Fact]
        public void A_passing_validator_lets_the_payment_through()
        {
            var account = PaymentTestData.AnAccount(balance: 100m);
            var scenario = ScenarioFor(account);

            var result = scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs, amount: 10m));

            Assert.True(result.Success);
            Assert.Equal(90m, account.Balance);
            scenario.Store.Received(1).UpdateAccount(account);
        }

        [Fact]
        public void Validators_are_not_consulted_when_the_account_does_not_exist()
        {
            var scenario = ScenarioFor(account: null);

            scenario.Service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs));

            scenario.Validators.ToList()
                .ForEach(validator => validator.DidNotReceive().Validate(Arg.Any<PaymentValidationModel>()));
        }

        private static Scenario ScenarioFor(Account account)
        {
            var store = Substitute.For<IAccountDataStore>();
            store.GetAccount(Arg.Any<string>()).Returns(account);

            var factory = Substitute.For<IAccountDataStoreFactory>();
            factory.Create().Returns(store);

            var validators = Enum.GetValues<PaymentScheme>().Select(PassingValidatorFor).ToList();
            var service = new PaymentService(factory, new PaymentSchemeValidatorFactory(validators), NullLogger<PaymentService>.Instance);

            return new Scenario(service, store, validators);
        }

        private static IPaymentSchemeValidator PassingValidatorFor(PaymentScheme scheme)
        {
            var validator = Substitute.For<IPaymentSchemeValidator>();
            validator.Scheme.Returns(scheme);
            validator.Validate(Arg.Any<PaymentValidationModel>()).Returns(new ValidationResult());
            return validator;
        }

        private sealed record Scenario(
            PaymentService Service,
            IAccountDataStore Store,
            IReadOnlyList<IPaymentSchemeValidator> Validators)
        {
            public IPaymentSchemeValidator ValidatorFor(PaymentScheme scheme) =>
                Validators.Single(validator => validator.Scheme == scheme);
        }
    }
}
