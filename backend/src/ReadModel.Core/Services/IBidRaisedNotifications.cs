namespace ReadModel.Core.Services
{
    public record BidRaisedNotificationArgs(Guid auctionId, Guid bidId, Guid winnerId, decimal newPrice, DateTime dateCreated);

    public interface IBidRaisedNotifications
    {
        Task NotifyBidRaised(BidRaisedNotificationArgs notificationArgs);
    }
}
