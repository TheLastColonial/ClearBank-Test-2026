using System;
using System.Linq;
using ClearBank.DeveloperTest.DependencyInjection;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Tests.TestData;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.DependencyInjection
{
    public class PaymentServiceCollectionExtensionsTests
    {
        [Fact]
        public void Payment_service_can_be_resolved_with_every_dependency_registered()
        {
            using var provider = BuildProvider(services => services.AddPaymentServices());

            Assert.IsType<PaymentService>(provider.GetRequiredService<IPaymentService>());
        }

        [Fact]
        public void The_validator_factory_can_be_resolved()
        {
            using var provider = BuildProvider(services => services.AddPaymentServices());

            Assert.NotNull(provider.GetRequiredService<IPaymentSchemeValidatorFactory>());
        }

        [Fact]
        public void Exactly_one_validator_is_registered_for_each_scheme()
        {
            using var provider = BuildProvider(services => services.AddPaymentServices());

            var schemes = provider.GetServices<IPaymentSchemeValidator>().Select(validator => validator.Scheme);

            Assert.Equal(
                Enum.GetValues<PaymentScheme>().OrderBy(scheme => scheme),
                schemes.OrderBy(scheme => scheme));
        }

        [Fact]
        public void Registering_twice_does_not_duplicate_validators()
        {
            using var provider = BuildProvider(services => services.AddPaymentServices().AddPaymentServices());

            Assert.IsType<PaymentService>(provider.GetRequiredService<IPaymentService>());
        }

        [Fact]
        public void Registered_services_apply_the_scheme_rules_end_to_end()
        {
            using var provider = BuildProvider(services => services.AddPaymentServices());
            var service = provider.GetRequiredService<IPaymentService>();

            var bacsResult = service.MakePayment(PaymentTestData.ARequest(PaymentScheme.Bacs));
            var unknownSchemeResult = service.MakePayment(PaymentTestData.ARequest((PaymentScheme)99));

            Assert.False(bacsResult.Success);
            Assert.True(unknownSchemeResult.Success);
        }

        [Fact]
        public void Registration_returns_the_service_collection_for_chaining()
        {
            var services = new ServiceCollection();

            Assert.Same(services, services.AddPaymentServices());
        }

        private static ServiceProvider BuildProvider(Action<IServiceCollection> register)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            register(services);

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }
    }
}
