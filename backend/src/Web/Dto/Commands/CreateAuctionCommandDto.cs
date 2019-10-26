using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Common.Domain.Products;

namespace Web.Dto.Commands
{
    public class CreateAuctionCommandDto
    {
        public decimal? BuyNowPrice { get; set; }
        [Required]
        public Product Product { get; set; }
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
    }
}
