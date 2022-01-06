using Auctions.Domain;
using Auctions.Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Test.Auctions.Base.Builders.AuctionTestConstants;

namespace Test.Auctions.Base.Mocks
{
    internal class TestConvertCategoryNamesToRootToLeafIds : IConvertCategoryNamesToRootToLeafIds
    {
        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            return Task.FromResult(CATEGORY_IDS.Select(c => new CategoryId(Convert.ToInt32(c))).ToArray());
        }
    }
}
