using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public class AuctionCreated : Event
    {
        public Guid AuctionId { get; }
        public decimal BuyNowPrice { get; set; } 
        public bool BuyNowOnly { get; set; }
        public string?[] AuctionImagesSize1Id { get; set; } 
        public string?[] AuctionImagesSize2Id { get; set; } 
        public string?[] AuctionImagesSize3Id { get; set; } 
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; } 
        public Guid Owner { get; set; } 
        public string ProductName { get; set; } 
        public string ProductDescription { get; set; } 
        public int ProductCondition { get; set; } 
        public int[] Category { get; set; } 
        public string[] Tags { get; set; } 
        public string Name { get; set; } 

        public AuctionCreated(Guid auctionId) : base(EventNames.AuctionCreated)
        {
            AuctionId = auctionId;
        }
    }
}