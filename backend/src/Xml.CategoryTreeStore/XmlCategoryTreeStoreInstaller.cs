using Adapter.XmlCategoryTreeStore;
using Core.Common.Domain.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace XmlCategoryTreeStore
{
    public static class XmlCategoryTreeStoreInstaller
    {
        public static void AddXmlCategoryTreeStore(this IServiceCollection services, XmlCategoryNameStoreSettings settings)
        {
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
