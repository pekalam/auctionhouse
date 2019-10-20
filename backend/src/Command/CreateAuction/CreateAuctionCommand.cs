using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common.Domain.Products;
using Core.Common.EventBus;
using Core.Common.Interfaces;
using MediatR;

namespace Core.Command.CreateAuction
{
    public class CreateAuctionCommand : IRequest, ICommand
    {
        public decimal? BuyNowPrice { get; set; }
        [Required]
        public Product Product { get; }
        [Required]
        public DateTime StartDate { get; }
        [Required]
        public DateTime EndDate { get; }
        [Required]
        public List<string> Category { get; }
        [Required]
        public CorrelationId CorrelationId { get; }

        public CreateAuctionCommand(decimal? buyNowPrice, Product product, DateTime startDate, DateTime endDate, List<string> category, CorrelationId correlationId)
        {
            BuyNowPrice = buyNowPrice;
            Product = product;
            StartDate = startDate;
            EndDate = endDate;
            Category = category;
            CorrelationId = correlationId;
        }
    }
}
