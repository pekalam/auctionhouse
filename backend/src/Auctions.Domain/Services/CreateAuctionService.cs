using Auctions.Domain.Repositories;
using Core.DomainFramework;
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
    /// Unit of work in which auction is created.
    /// It is assumed that corresponding AuctionBids aggregate is created and then it's id is assigned in end method.
    /// This class is meant to be used in distributed transaction as well as in monolith scenario.
    /// It implements memento pattern (<see cref="CreateAuctionServiceData"/>)
    /// </summary>
    public class CreateAuctionService
    {
        private Lazy<IAuctionImageRepository> _auctionImages;
        private Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private Lazy<IAuctionRepository> _auctions;
        private Auction? _auction;
        private bool _commited;
        private bool _needsToBeAdded = false;

        public CreateAuctionServiceData? ServiceData { get; private set; }

        public bool Finished => _auction?.Locked == false && _commited;

        public CreateAuctionService(Lazy<IAuctionImageRepository> auctionImages, Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions, CreateAuctionServiceData? serviceData = null)
        {
            _auctionImages = auctionImages;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
            ServiceData = serviceData;
        }

        public async Task<Auction> StartCreate(AuctionCreateSession auctionCreateSession, AuctionArgs auctionArgs)
        {
            if (_needsToBeAdded) throw new InvalidOperationException("Cannot be called twice");

            var auction = _auction = auctionCreateSession.CreateAuction(auctionArgs);

            await _auctionEndScheduler.Value.ScheduleAuctionEnd(auction);
            _needsToBeAdded = true;
            if (auction.BuyNowOnly) // buy now only auctions dont need further actions so transaction is finished
            {
                return auction;
            }

            ServiceData = new(auction.LockIssuer, auction.AggregateId);
            return auction;
        }

        public void EndCreate(AuctionBidsId auctionBidsId)
        {
            if(ServiceData is null)
            {
                throw new InvalidOperationException($"{nameof(ServiceData)} is null. {nameof(StartCreate)} wasn't called.");
            }
            if(_auction is null)
                _auction = _auctions.Value.FindAuction(ServiceData.AuctionId);
            if (!_auction.Locked) throw new InvalidOperationException("Auction is not locked");
            _auction.AddAuctionBids(auctionBidsId);
        }

        public void Commit()
        {
            if (_auction is null) throw new NullReferenceException($"{nameof(_auction)} is null");
            if (_needsToBeAdded)
            {
                _auctions.Value.AddAuction(_auction);

                var imagesToSave = _auction.AuctionImages //TODO move to auction session
                    .Where(image => image != null)
                    .SelectMany(img => new string[3] { img.Size1Id, img.Size2Id, img.Size3Id }.AsEnumerable())
                    .ToArray();
                _auctionImages.Value.UpdateManyMetadata(imagesToSave, new AuctionImageMetadata(null)
                {
                    IsAssignedToAuction = true
                });
            }
            else
            {
                _auctions.Value.UpdateAuction(_auction);
            }
            _commited = true;
        }
    }
}
