using Auctions.Domain;
using Auctions.Domain.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Auctions.Tests.Base.Domain.ModelBuilders.AuctionTestConstants;

namespace Auctions.Tests.Base.Domain.Services.Fakes
{
    internal class FakeConvertCategoryNamesToRootToLeafIds : ICategoryNamesToTreeIdsConversion
    {
        public Task<CategoryId[]> ConvertNames(string[] categoryNames)
        {
            return Task.FromResult(CATEGORY_IDS.Select(c => new CategoryId(Convert.ToInt32(c))).ToArray());
        }
    }
}
