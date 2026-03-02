using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearBank.DeveloperTest.Tests;

public class InstallerTests
{
    [Fact]
    public void InstallerAddsExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
            { "DataStoreType", "Primary" }
            })
            .Build();

        services.AddLogging();

        // Act
        services.AddPaymentService(config);
        var provider = services.BuildServiceProvider();

        // Assert
        using var scope = provider.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
        Assert.IsType<AccountDataStore>(repo);

        var payementService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        Assert.IsType<PaymentService>(payementService);

        var validators = provider.GetServices<ISchemeValidator>().ToList();

        Assert.Contains(validators, validator => validator is BacsValidator);
        Assert.Contains(validators, validator => validator is FasterPaymentsValidator);
        Assert.Contains(validators, validator => validator is ChapsValidator);
    }

    [Fact]
    public void AssertBackupDataStoreIsAddedWhenConfigured()
    {
        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
            { "DataStoreType", "Backup" }
            })
            .Build();

        services.AddPaymentService(config);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

        Assert.IsType<BackupAccountDataStore>(repo);
    }
}
