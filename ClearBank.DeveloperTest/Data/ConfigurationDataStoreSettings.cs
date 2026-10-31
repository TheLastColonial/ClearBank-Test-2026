using System.Configuration;

namespace ClearBank.DeveloperTest.Data
{
    internal sealed class ConfigurationDataStoreSettings : IDataStoreSettings
    {
        public string DataStoreType => ConfigurationManager.AppSettings["DataStoreType"];
    }
}
