using ClearBank.DeveloperTest.Data;
using NSubstitute;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Data
{
    public class AccountDataStoreFactoryTests
    {
        [Fact]
        public void Backup_setting_creates_the_backup_data_store()
        {
            var factory = FactoryWithSetting("Backup");

            Assert.IsType<BackupAccountDataStore>(factory.Create());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Primary")]
        [InlineData("backup")]
        public void Any_other_setting_creates_the_default_data_store(string setting)
        {
            var factory = FactoryWithSetting(setting);

            Assert.IsType<AccountDataStore>(factory.Create());
        }

        [Fact]
        public void Setting_is_read_on_every_call()
        {
            var settings = Substitute.For<IDataStoreSettings>();
            var factory = new AccountDataStoreFactory(settings);

            settings.DataStoreType.Returns("Backup");
            var first = factory.Create();
            settings.DataStoreType.Returns((string)null);
            var second = factory.Create();

            Assert.IsType<BackupAccountDataStore>(first);
            Assert.IsType<AccountDataStore>(second);
        }

        private static AccountDataStoreFactory FactoryWithSetting(string setting)
        {
            var settings = Substitute.For<IDataStoreSettings>();
            settings.DataStoreType.Returns(setting);
            return new AccountDataStoreFactory(settings);
        }
    }
}
