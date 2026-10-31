using System;
using System.Linq;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using NSubstitute;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Validation
{
    public class PaymentSchemeValidatorFactoryTests
    {
        [Theory]
        [InlineData(PaymentScheme.Bacs)]
        [InlineData(PaymentScheme.FasterPayments)]
        [InlineData(PaymentScheme.Chaps)]
        public void Creates_the_validator_registered_for_the_scheme(PaymentScheme scheme)
        {
            var validators = PaymentTestData.AllSchemeValidators();
            var factory = new PaymentSchemeValidatorFactory(validators);

            var validator = factory.Create(scheme);

            Assert.Same(validators.Single(candidate => candidate.Scheme == scheme), validator);
        }

        [Fact]
        public void A_scheme_with_no_validator_gets_a_validator_that_accepts_everything()
        {
            var factory = new PaymentSchemeValidatorFactory(PaymentTestData.AllSchemeValidators());
            var unrestrictedAccount = PaymentTestData.AnAccount(
                allowedPaymentSchemes: 0, balance: -50m, status: AccountStatus.Disabled);
            var model = new PaymentValidationModel(unrestrictedAccount, PaymentTestData.ARequest((PaymentScheme)99, amount: 1000m));

            var result = factory.Create((PaymentScheme)99).Validate(model);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void A_registered_validator_for_an_undefined_scheme_is_used()
        {
            var customValidator = Substitute.For<IPaymentSchemeValidator>();
            customValidator.Scheme.Returns((PaymentScheme)99);
            var factory = new PaymentSchemeValidatorFactory(PaymentTestData.AllSchemeValidators().Append(customValidator));

            Assert.Same(customValidator, factory.Create((PaymentScheme)99));
        }

        [Fact]
        public void Scheme_validators_are_required()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentSchemeValidatorFactory(null));

            Assert.Equal("schemeValidators", exception.ParamName);
        }

        [Theory]
        [InlineData(PaymentScheme.Bacs)]
        [InlineData(PaymentScheme.FasterPayments)]
        [InlineData(PaymentScheme.Chaps)]
        public void Every_defined_scheme_must_have_a_validator(PaymentScheme missingScheme)
        {
            var validators = PaymentTestData.AllSchemeValidators().Where(validator => validator.Scheme != missingScheme);

            var exception = Assert.Throws<ArgumentException>(() => new PaymentSchemeValidatorFactory(validators));

            Assert.Equal("schemeValidators", exception.ParamName);
            Assert.Contains(missingScheme.ToString(), exception.Message);
        }

        [Fact]
        public void No_defined_scheme_may_have_more_than_one_validator()
        {
            var validators = PaymentTestData.AllSchemeValidators().Append(new BacsPaymentValidator());

            var exception = Assert.Throws<ArgumentException>(() => new PaymentSchemeValidatorFactory(validators));

            Assert.Equal("schemeValidators", exception.ParamName);
            Assert.Contains(PaymentScheme.Bacs.ToString(), exception.Message);
        }

        [Fact]
        public void No_undefined_scheme_may_have_more_than_one_validator()
        {
            var first = Substitute.For<IPaymentSchemeValidator>();
            first.Scheme.Returns((PaymentScheme)99);
            var second = Substitute.For<IPaymentSchemeValidator>();
            second.Scheme.Returns((PaymentScheme)99);
            var validators = PaymentTestData.AllSchemeValidators().Append(first).Append(second);

            var exception = Assert.Throws<ArgumentException>(() => new PaymentSchemeValidatorFactory(validators));

            Assert.Equal("schemeValidators", exception.ParamName);
        }
    }
}
