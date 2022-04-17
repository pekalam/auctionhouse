using Auctions.Domain;
using Auctions.Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Auctions.Tests.Base.Builders.AuctionTestConstants;

namespace Auctions.Tests.Base.Mocks
{
    internal class TestConvertCategoryNamesToRootToLeafIds : ICategoryNamesToTreeIdsConversion
    {
        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            return Task.FromResult(CATEGORY_IDS.Select(c => new CategoryId(Convert.ToInt32(c))).ToArray());
        }
    }
}
