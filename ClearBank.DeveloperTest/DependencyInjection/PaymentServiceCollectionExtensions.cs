using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClearBank.DeveloperTest.DependencyInjection
{
    public static class PaymentServiceCollectionExtensions
    {
        public static IServiceCollection AddPaymentServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IDataStoreSettings, ConfigurationDataStoreSettings>();
            services.TryAddSingleton<IAccountDataStoreFactory, AccountDataStoreFactory>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPaymentSchemeValidator, BacsPaymentValidator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPaymentSchemeValidator, FasterPaymentsPaymentValidator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPaymentSchemeValidator, ChapsPaymentValidator>());

            services.TryAddSingleton<IPaymentSchemeValidatorFactory, PaymentSchemeValidatorFactory>();

            services.TryAddSingleton<IPaymentService, PaymentService>();

            return services;
        }
    }
}
