using System;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionCreated : Event
    {
        public Guid AuctionId { get; }
        public Product Product { get; }
        public decimal? BuyNowPrice { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public UserIdentity Creator { get; }
        public Category Category { get; }
        public AuctionImage[] AuctionImages { get; }

        public AuctionCreated(Guid auctionId, Product product, decimal? buyNowPrice, 
            DateTime startDate, DateTime endDate, 
            UserIdentity creator, Category category, AuctionImage[] auctionImages) : base(EventsNames.AuctionCreatedEventName)
        {
            AuctionId = auctionId;
            Product = product;
            BuyNowPrice = buyNowPrice;
            StartDate = startDate;
            EndDate = endDate;
            Creator = creator;
            Category = category;
            AuctionImages = auctionImages;
        }
    }
}