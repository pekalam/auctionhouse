using Core.Common.Domain.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace Categories.DI;

public class CategoriesInstaller
{
    public CategoriesInstaller(IServiceCollection services)
    {
        Services = services;
        services.AddTransient<CategoryBuilder>();
    }

    public IServiceCollection Services { get; }

    public CategoriesInstaller AddCategoryTreeStore(Func<IServiceProvider, ICategoryTreeStore> factory)
    {
        Services.AddTransient(factory);
        return this;
    }
}