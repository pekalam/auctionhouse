using Auctions.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Domain.Services
{
    public class CreateAuctionServiceData
    {
        public Guid LockIssuer { get; }
        public AuctionId AuctionId { get; }

        public CreateAuctionServiceData(Guid lockIssuer, AuctionId auctionId)
        {
            LockIssuer = lockIssuer;
            AuctionId = auctionId;
        }
    }

    /// <summary>
    /// Should be used to create auction when strong consistency across aggregates that use auction is required. Service locks auction and unlocks it. 
    /// Caller should create dependent aggregates between start / end calls and persist auction.
    /// </summary>
    public class CreateAuctionService
    {
        private readonly IAuctionRepository _auctions;
        public CreateAuctionServiceData? ServiceData { get; private set; }

        public CreateAuctionService(IAuctionRepository auctions, CreateAuctionServiceData? serviceData = null)
        {
            ServiceData = serviceData;
            _auctions = auctions;
        }

        public Auction StartCreate(AuctionArgs auctionArgs)
        {
            var auction = new Auction(auctionArgs);
            var lockIssuer = Guid.NewGuid();
            auction.Lock(lockIssuer);
            ServiceData = new(lockIssuer, auction.AggregateId);
            return auction;
        }

        public void EndCreate()
        {
            if(ServiceData is null)
            {
                throw new InvalidOperationException($"{nameof(ServiceData)} is null. {nameof(StartCreate)} wasn't called.");
            }
            var auction = _auctions.FindAuction(ServiceData.AuctionId);
            if (!auction.Locked) throw new InvalidOperationException("Auction is not locked");
            auction.Unlock(ServiceData.LockIssuer);
            _auctions.UpdateAuction(auction);
        }
    }
}
