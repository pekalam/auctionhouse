using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Domain.Services
{
    /// <summary>
    /// Converts names of categories to sequence of category ids coming from tree root to leaf
    /// </summary>
    public interface ICategoryNamesToTreeIdsConversion
    {
        Task<CategoryId[]> ConvertNames(string[] categoryNames);
    }
}
