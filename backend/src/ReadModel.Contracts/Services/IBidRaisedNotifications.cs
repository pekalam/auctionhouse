using System;
using System.Threading.Tasks;

namespace ReadModel.Contracts.Services
{
    public record BidRaisedNotificationArgs(Guid auctionId, Guid bidId, Guid winnerId, decimal newPrice, DateTime dateCreated);

    public interface IBidRaisedNotifications
    {
        Task NotifyBidRaised(BidRaisedNotificationArgs notificationArgs);
    }
}
