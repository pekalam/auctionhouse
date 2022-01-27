using Auctions.Domain;
using Auctions.Domain.Services;
using Test.Auctions.Base.Builders;

namespace Test.Auctions.Base.ServiceContracts
{
    public record AuctionEndSchedulerScenario(AuctionEndSchedulerScenarioArgs given, AuctionEndSchedulerScenarioController ctrl);

    public record AuctionEndSchedulerScenarioExpected(bool completed);

    public class AuctionEndSchedulerScenarioController
    {
        private readonly Auction _auction;

        public AuctionEndSchedulerScenarioController(Auction auction)
        {
            _auction = auction;
        }

        public async Task<AuctionEndSchedulerScenarioExpected> WaitUntilEndCallIsDue(IAuctionEndScheduler auctionEndScheduler, CancellationToken ct, TimeSpan? timeout = null, Action<string>? logging = null)
        {
            auctionEndScheduler.ScheduleAuctionEnd(_auction).GetAwaiter().GetResult();
            var now = DateTime.UtcNow;
            var end = _auction.EndDate.Value;
            var timeToWait = end - now;
            logging?.Invoke($"Waiting {timeToWait}");
            await Task.Delay(timeToWait);
            timeout = timeout ?? TimeSpan.FromSeconds(20);
            logging?.Invoke($"Waiting additional time {timeout}");
            try
            {
                await Task.Delay(timeout.Value, ct);
            }
            catch (TaskCanceledException)
            {
            }
            return new(true);
        }
    }

    public record AuctionEndSchedulerScenarioArgs(Auction auction);

    public class AuctionEndSchedulerContracts
    {
        public static AuctionEndSchedulerScenario Success
        {
            get
            {
                var auctionArgs = new GivenAuctionArgs()
                    .WithStartDate(DateTime.UtcNow)
                    .WithEndDate(DateTime.UtcNow.AddSeconds(10))
                    .WithBuyNowOnly(false)
                    .Build();
                var auction = new GivenAuction()
                    .WithAuctionArgs(auctionArgs)
                    .WithAssignedAuctionBidsId(new AuctionBidsId(Guid.NewGuid()))
                    .Build();
                return new(new(auction), new(auction));
            }
        }
    }
}
