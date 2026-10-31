namespace ClearBank.DeveloperTest.Data
{
    internal sealed class AccountDataStoreFactory : IAccountDataStoreFactory
    {
        private const string BackupDataStoreType = "Backup";

        private readonly IDataStoreSettings _settings;

        public AccountDataStoreFactory(IDataStoreSettings settings)
        {
            _settings = settings;
        }

        public IAccountDataStore Create() =>
            _settings.DataStoreType == BackupDataStoreType
                ? new BackupAccountDataStore()
                : new AccountDataStore();
    }
}
