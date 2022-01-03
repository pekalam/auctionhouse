using Auctions.Domain;
using System;
using System.Threading.Tasks;
using Auctions.Domain.Services;
using System.Linq;

namespace FunctionalTests.Mocks
{
    public class ConvertCategoryNamesToRootToLeafIdsMock : IConvertCategoryNamesToRootToLeafIds
    {
        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            return Task.FromResult(categoryNames.Select(c => new CategoryId(Convert.ToInt32(c))).ToArray());
        }
    }
}