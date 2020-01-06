using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Query.Queries.User.UserAuctions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Web.Dto.Queries
{
    public class UserBoughtAuctionsQueryDto
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;

        [FromQuery(Name = "sort")] public UserAuctionsSorting Sorting { get; set; }

        [FromQuery(Name = "dir")] public UserAuctionsSorting SortingDirection { get; set; }
    }
}
