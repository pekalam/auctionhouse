using Categories.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XmlCategoryTreeStore;

namespace Categories.DI;

public static class InstallationExtensions
{
    public static void AddCategoriesModule(this IServiceCollection services, IConfiguration configuration)
    {
        new CategoriesInstaller(services)
            .AddXmlCategoryTreeStoreAdapter(configuration);
    }
}
