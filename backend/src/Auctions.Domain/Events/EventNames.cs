namespace Auctions.Domain.Events
{
    public sealed class EventNames
    {
        public const string AuctionCreated = "auctionCreated";
        public const string AuctionCompleted = "auctionCompleted";
        public const string AuctionBought = "auctionBought";
        public const string AuctionRaised = "auctionRaised";
        public const string AuctionRemoved = "auctionRemoved";
        public const string UserRegistered = "userRegistered";
        public const string BidCanceled = "bidCanceled";
        public const string AuctionImageAddedEventName = "auctionImageAdded";
        public const string AuctionCanceled = "auctionCanceled";
        public const string BidRemoved = "bidRemoved";
        public const string AuctionImageRemoved = "auctionImageRemoved";
        public const string AuctionImageReplaced = "auctionImageReplaced";
        public const string AuctionBuyNowPriceChanged = "auctionBuyNowPriceChanged";
        public const string AuctionTagsChanged = "auctionTagsChanged";
        public const string AuctionEndDateChanged = "auctionEndDateChanged";
        public const string AuctionCategoryChanged = "auctionCategoryChanged";
        public const string AuctionNameChanged = "auctionNameChanged";
        public const string AuctionUpdated = "auctionUpdateEventGroup";
        public const string AuctionDescriptionChanged = "auctionDescriptionChanged";


        private EventNames() { }
    }
}
