using Auctions.Domain;
using Auctions.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Tests.Base.Domain.Services.Fakes
{
    public class FakeAuctionUnlockScheduler : IAuctionUnlockScheduler
    {
        public static FakeAuctionUnlockScheduler Instance => new FakeAuctionUnlockScheduler();

        public void Cancel(AuctionId auctionId)
        {
        }

        public void ScheduleAuctionUnlock(AuctionId auctionId, TimeOnly time)
        {
        }
    }
}
