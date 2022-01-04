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
        }
    }
}
