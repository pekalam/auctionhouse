using Auctions.Domain.Services;
using Core.Common.Domain.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationService.CategoryNamesToTreeIdsConversion
{
    using Auctions.Domain;

    internal class CategoryNamesToTreeIdsConversion : ICategoryNamesToTreeIdsConversion
    {
        private readonly CategoryBuilder _categoryBuilder;

        public CategoryNamesToTreeIdsConversion(CategoryBuilder categoryBuilder)
        {
            _categoryBuilder = categoryBuilder;
        }

        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            var category = _categoryBuilder.FromCategoryNamesList(new(categoryNames));
            var categoryIds = new List<CategoryId>();

            Category current = category;
            do
            {
                categoryIds.Add(new CategoryId(current.Id));
                current = current.SubCategory;
            }
            while (current is not null);

            return Task.FromResult(categoryIds.ToArray());
        }
    }

    public static class CategoryNamesToTreeIdsConversionInstaller
    {
        public static void AddCategoryNamesToTreeIdsConversion(this IServiceCollection services)
        {
            services.AddTransient<ICategoryNamesToTreeIdsConversion, CategoryNamesToTreeIdsConversion>();
        }
    }
}