using Microsoft.AspNetCore.SignalR;
using ReadModel.Core;
using ReadModel.Core.Services;

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
            catch (Exception)
            {
                _logger.LogWarning("Could not send notification about raised bid for an auction {@auctionId}", notificationArgs.auctionId);
            }
        }
    }

    internal static class BidRaisedNotificationsInstaller
    {
        public static ReadModelInstaller AddBidRaisedNotificationsAdapter(this ReadModelInstaller installer)
        {
            installer.Services.AddTransient<BidRaisedNotifications>();
            installer.AddBidRaisedNotifications((prov) => prov.GetRequiredService<BidRaisedNotifications>());
            return installer;
        }
    }
}
