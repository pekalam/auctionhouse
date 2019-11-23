using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Common.Common;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Products;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class ProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Condition Condition { get; set; }
    }

    public class CreateAuctionCommandDto
    {
        public decimal? BuyNowPrice { get; set; }
        public ProductDto Product { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Category { get; set; }
        public string[] Tags { get; set; }
        public string Name { get; set; }
        public bool? BuyNowOnly { get; set; }
    }
}
