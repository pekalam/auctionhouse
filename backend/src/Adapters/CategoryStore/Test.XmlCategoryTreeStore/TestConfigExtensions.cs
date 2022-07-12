using Microsoft.Extensions.Configuration;
using XmlCategoryTreeStore;

namespace Test.XmlCategoryTreeStore
{
    internal static class TestConfigExtensions
    {
        private const string SectionName = "XmlCategoryTreeStoreTests";

        public static XmlCategoryNameStoreSettings GetStoreSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection(SectionName).Get<XmlCategoryNameStoreSettings>();
        }

        public static string GetSchemaFilePath(this IConfigurationRoot configuration)
        {
            return configuration[$"{SectionName}:{nameof(XmlCategoryNameStoreSettings.SchemaFilePath)}"];
        }
    }
}
