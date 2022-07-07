using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Domain.Services
{
    public interface IAuctionUnlockScheduler
    {
        void ScheduleAuctionUnlock(AuctionId auctionId, TimeOnly time);
        void Cancel(AuctionId auctionId);
    }
}
