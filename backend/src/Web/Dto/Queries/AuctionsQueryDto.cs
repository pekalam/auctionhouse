using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Query.Queries.Auction.Auctions;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class AuctionsQueryDto
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; }
        [FromQuery(Name = "categories")] public string[] Categories { get; set; }

        [FromQuery(Name = "cond")]
        public Condition Condition { get; set; } = Condition.All;
        [FromQuery(Name = "minpr")]
        public decimal MinPrice { get; set; } = 0;
        [FromQuery(Name = "maxpr")]
        public decimal MaxPrice { get; set; } = 0;
        [FromQuery(Name = "type")]
        public AuctionType AuctionType { get; set; } = AuctionType.All;
    }
}
