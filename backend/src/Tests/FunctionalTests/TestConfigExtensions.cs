using Adapter.EfCore.ReadModelNotifications;
using Microsoft.Extensions.Configuration;
using RabbitMq.EventBus;
using ReadModel.Core.Model;
using XmlCategoryTreeStore;

namespace FunctionalTests
{
    internal static class TestConfigExtensions
    {
        private const string SectionName = "FunctionalTests";

        public static RabbitMqSettings GetRabbitMqSettings(this IConfigurationRoot configuration)
        {
            return new RabbitMqSettings
            {
                ConnectionString = configuration.GetSection(SectionName)["RabbitMq:ConnectionString"]
            };
        }

        public static MongoDbSettings GetReadModelSettings(this IConfigurationRoot configuration)
        {
            return new MongoDbSettings
            {
                ConnectionString = configuration.GetSection(SectionName).GetSection("ReadModelSettings")["ConnectionString"],
                DatabaseName = configuration.GetSection(SectionName).GetSection("ReadModelSettings")["DatabaseName"]
            };
        }

        public static XmlCategoryNameStoreSettings GetXmlStoreSettings(this IConfigurationRoot configuration)
        {
            return new XmlCategoryNameStoreSettings
            {
                CategoriesFilePath = configuration.GetSection(SectionName).GetSection("XmlCategoryTreeStore")["CategoriesFilePath"],
                SchemaFilePath = configuration.GetSection(SectionName).GetSection("XmlCategoryTreeStore")["SchemaFilePath"],
            };
        }

        public static string GetChronicleSQLServerStorageConnectionString(this IConfigurationRoot configuration)
        {
            return configuration.GetSection(SectionName).GetSection("ChronicleSQLServerStorage")["ConnectionString"];
        }

        public static EfCoreReadModelNotificaitonsOptions GetEfCoreReadModelNotificaitonsOptions(this IConfigurationRoot configuration)
        {
            return configuration.GetSection(SectionName).GetSection("ReadModelNotificaitons").Get<EfCoreReadModelNotificaitonsOptions>();
        }
    }
}
