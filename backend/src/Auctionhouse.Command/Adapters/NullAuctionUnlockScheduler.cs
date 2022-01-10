using Auctions.Domain;
using Auctions.Domain.Services;
using UserPayments.Domain;
using UserPayments.Domain.Repositories;

namespace Auctionhouse.Command.Adapters
{
    public class NullAuctionUnlockScheduler : IAuctionUnlockScheduler
    {
        public void Cancel(AuctionId auctionId)
        {
        }

        public void ScheduleAuctionUnlock(AuctionId auctionId, TimeOnly time)
        {
        }
    }
}
