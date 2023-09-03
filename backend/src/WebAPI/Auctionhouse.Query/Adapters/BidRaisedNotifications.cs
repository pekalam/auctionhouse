using Microsoft.AspNetCore.SignalR;
using ReadModel.Contracts.Services;

namespace Auctionhouse.Query.Adapters
{
    internal class BidRaisedNotifications : IBidRaisedNotifications
    {
        public const string BidRaisedEventId = "AuctionPriceChanged";
        private readonly IHubContext<ApplicationHub> _hubContext;
        private readonly ILogger<BidRaisedNotifications> _logger;

        public BidRaisedNotifications(IHubContext<ApplicationHub> hubContext, ILogger<BidRaisedNotifications> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyBidRaised(BidRaisedNotificationArgs notificationArgs)
        {
            _logger.LogDebug("Sending websocket notifications about raised bid for an auction {@auctionId}", notificationArgs.auctionId);
            var methodName = BidRaisedEventId + "-" + notificationArgs.auctionId;
            try
            {
                await _hubContext.Clients.All.SendAsync(methodName, new { 
                    newPrice = decimal.Round(notificationArgs.newPrice, 2, MidpointRounding.AwayFromZero).ToString(),
                    notificationArgs.winnerId,
                    notificationArgs.auctionId,
                    notificationArgs.bidId,
                    notificationArgs.dateCreated,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not send notification about raised bid for an auction {@auctionId}", notificationArgs.auctionId);
            }
        }
    }
}
