using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Query.Queries.User.UserAuctions;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class UserAuctionsQueryDto
    {
        [FromQuery(Name = "page")] public int Page { get; set; } = 0;

        [FromQuery(Name = "show-archived")] public bool ShowArchived { get; set; }

        [FromQuery(Name = "sort")] public UserAuctionsSorting Sorting { get; set; }

        [FromQuery(Name = "dir")] public UserAuctionsSorting SortingDirection { get; set; }
    }
}