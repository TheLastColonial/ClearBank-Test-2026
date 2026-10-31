using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services
{
    public class PaymentServiceCompositionTests
    {
        [Fact]
        public void A_data_store_factory_is_required()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PaymentService(null, PaymentTestData.AValidatorFactory(), NullLogger<PaymentService>.Instance));

            Assert.Equal("dataStoreFactory", exception.ParamName);
        }

        [Fact]
        public void A_validator_factory_is_required()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PaymentService(Substitute.For<IAccountDataStoreFactory>(), null, NullLogger<PaymentService>.Instance));

            Assert.Equal("validatorFactory", exception.ParamName);
        }

        [Fact]
        public void A_logger_is_required()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PaymentService(Substitute.For<IAccountDataStoreFactory>(), PaymentTestData.AValidatorFactory(), null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void A_request_is_required()
        {
            var service = ServiceWith(PaymentTestData.AValidatorFactory());

            var exception = Assert.Throws<ArgumentNullException>(() => service.MakePayment(null));

            Assert.Equal("request", exception.ParamName);
        }

        private static PaymentService ServiceWith(IPaymentSchemeValidatorFactory validatorFactory) =>
            new PaymentService(Substitute.For<IAccountDataStoreFactory>(), validatorFactory, NullLogger<PaymentService>.Instance);
    }
}
