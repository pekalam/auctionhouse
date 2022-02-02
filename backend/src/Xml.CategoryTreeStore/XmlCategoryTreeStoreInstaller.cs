using Adapter.XmlCategoryTreeStore;
using Core.Common.Domain.Categories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XmlCategoryTreeStore
{
    public static class XmlCategoryTreeStoreInstaller
    {
        public static void AddXmlCategoryTreeStore(this IServiceCollection services, IConfiguration? configuration = null, XmlCategoryNameStoreSettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(XmlCategoryNameStoreSettings)).Get<XmlCategoryNameStoreSettings>();
            services.AddSingleton(settings);
            services.AddSingleton<ICategoryTreeStore, XmlCategoryTreeStore>();
            services.AddHostedService(provider =>
            {
                return new XmlCategoryTreeInitializer(provider);
            });
        }

        public static void Init(IServiceProvider serviceProvider)
        {
            var store = (XmlCategoryTreeStore)serviceProvider.GetRequiredService<ICategoryTreeStore>();
            store.Init();
        }
    }
}
