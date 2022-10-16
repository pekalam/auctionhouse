using Auctions.Domain;
using System;
using System.Threading.Tasks;
using Auctions.Domain.Services;
using System.Linq;

namespace FunctionalTests.Mocks
{
    public class ConvertCategoryNamesToRootToLeafIdsMock : ICategoryNamesToTreeIdsConversion
    {
        public static ConvertCategoryNamesToRootToLeafIdsMock Instance { get; } = new ConvertCategoryNamesToRootToLeafIdsMock();

        public static ConvertCategoryNamesToRootToLeafIdsMock Create() => new ConvertCategoryNamesToRootToLeafIdsMock();

        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            return Task.FromResult(categoryNames.Select(c => new CategoryId(Convert.ToInt32(c))).ToArray());
        }
    }
}