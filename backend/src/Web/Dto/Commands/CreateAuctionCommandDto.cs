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
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public Condition Condition { get; set; }
    }

    public class CreateAuctionCommandDto
    {
        public decimal? BuyNowPrice { get; set; }
        [Required]
        public ProductDto Product { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public List<string> Category { get; set; }
        [Required]
        public string CorrelationId { get; set; }
        [Required]
        public string[] Tags { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
