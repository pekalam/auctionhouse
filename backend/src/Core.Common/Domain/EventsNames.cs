namespace Core.Common.Domain
{
    public sealed class EventsNames
    {
        public const string AuctionCreatedEventName = "auctionCreated";
        public const string AuctionCompletedEventName = "auctionCompleted";
        public const string AuctionBoughtEventName = "auctionBought";
        public const string AuctionRaisedEventName = "auctionRaised";
        public const string AuctionRemovedEventName = "auctionRemoved";
        public const string UserRegisteredEventName = "userRegistered";
        public const string BidCanceledEventName = "bidCanceled";
        public const string UserCreatedAuctionEventName = "userCreatedAuction";
        public const string AuctionImageAddedEventName = "auctionImageAdded";
        public const string AuctionCanceledEventName = "auctionCanceled";

        private EventsNames() { }
    }
}
