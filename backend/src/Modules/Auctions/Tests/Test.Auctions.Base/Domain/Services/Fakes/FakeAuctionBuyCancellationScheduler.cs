using Auctions.Domain;
using Auctions.Domain.Services;

namespace Auctions.Tests.Base.Domain.Services.Fakes
{
    public class FakeAuctionBuyCancellationScheduler : IAuctionBuyCancellationScheduler
    {
        public static FakeAuctionBuyCancellationScheduler Instance => new FakeAuctionBuyCancellationScheduler();

        public void Cancel(AuctionId auctionId)
        {
        }

        public void ScheduleAuctionBuyCancellation(AuctionId auctionId, TimeOnly time)
        {
        }
    }
}
