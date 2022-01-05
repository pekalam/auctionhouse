using Auctions.Domain.Services;
using Core.Common.Domain.Categories;

namespace Auctionhouse.Command.Adapters
{
    using Auctions.Domain;

    internal class ConvertCategoryNamesToRootToLeafIds : IConvertCategoryNamesToRootToLeafIds
    {
        private readonly CategoryBuilder _categoryBuilder;

        public ConvertCategoryNamesToRootToLeafIds(CategoryBuilder categoryBuilder)
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
}
