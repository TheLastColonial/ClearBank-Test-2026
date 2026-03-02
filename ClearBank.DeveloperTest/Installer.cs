namespace ClearBank.DeveloperTest;

using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Installer
{
    private const string BACKUP = "backup";
    private const string DATA_STORE_TYPE = "DataStoreType";

    public static void AddPaymentService(this IServiceCollection services, IConfiguration configuration)
    {
        AddAccountStore(services, configuration);
        AddValidation(services);
    }

    public static void AddAccountStore(this IServiceCollection services, IConfiguration configuration)
    {
        var storeType = configuration[DATA_STORE_TYPE]?.ToLower();

        if (storeType == BACKUP)
        {
            services.AddScoped<IAccountRepository, BackupAccountDataStore>();
        }
        else
        {
            services.AddScoped<IAccountRepository, AccountDataStore>();
        }

        services.AddScoped<IPaymentService, PaymentService>();
    }

    public static void AddValidation(this IServiceCollection services)
    {
        // View Model
        services.AddSingleton<IMakePaymentRequestValidator, MakePaymentRequestValidator>();

        // Paymetn
        services.AddSingleton<ISchemeValidator, BacsValidator>();
        services.AddSingleton<ISchemeValidator, FasterPaymentsValidator>();
        services.AddSingleton<ISchemeValidator, ChapsValidator>();

        services.AddScoped<IPaymentSchemesValidator, PaymentSchemeValidator>();
    }
}
