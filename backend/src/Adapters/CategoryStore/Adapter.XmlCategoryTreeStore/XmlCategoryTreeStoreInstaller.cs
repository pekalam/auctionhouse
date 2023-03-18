using Adapter.XmlCategoryTreeStore;
using Categories.Domain;
using Core.Common.Domain.Categories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XmlCategoryTreeStore
{
    public static class XmlCategoryTreeStoreInstaller
    {
        public static CategoriesInstaller AddXmlCategoryTreeStoreAdapter(this CategoriesInstaller installer,
            IConfiguration? configuration = null,
            XmlCategoryNameStoreSettings? settings = null)
        {
            installer.Services.AddSingleton<XmlCategoryTreeStore>();
            installer.AddXmlCategoryTreeStoreAdapter((prov) => prov.GetRequiredService<XmlCategoryTreeStore>(), configuration, settings);
            return installer;
        }

        public static CategoriesInstaller AddXmlCategoryTreeStoreAdapter(this CategoriesInstaller installer,
            Func<IServiceProvider, ICategoryTreeStore> categoryTreeStoreFactory,
            IConfiguration? configuration = null, 
            XmlCategoryNameStoreSettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(XmlCategoryNameStoreSettings)).Get<XmlCategoryNameStoreSettings>();
            installer.Services.AddSingleton(settings);
            installer.Services.AddHostedService(provider =>
            {
                return new XmlCategoryTreeInitializer(provider);
            });

            installer.AddCategoryTreeStore(categoryTreeStoreFactory);
            return installer;
        }

        public static void Init(IServiceProvider serviceProvider)
        {
            var store = (XmlCategoryTreeStore)serviceProvider.GetRequiredService<ICategoryTreeStore>();
            store.Init();
        }
    }
}
