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
    /// Creates an auction and coordinates calls to services required to create the auction. Does not persist created auction.
    /// It is assumed that corresponding AuctionBids aggregate is created and then it's id is assigned in <see cref="EndCreate(AuctionBidsId)"/> method.
    /// This class is meant to be used in distributed transaction as well as in monolith scenario.
    /// It implements memento pattern (<see cref="CreateAuctionServiceData"/>)
    /// </summary>
    public class CreateAuctionService
    {
        private Lazy<IAuctionImageRepository> _auctionImages;
        private Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private Lazy<IAuctionRepository> _auctions;
        private Auction? _auction;
        private bool _startSuccess;

        public CreateAuctionServiceData? ServiceData { get; private set; }

        /// <summary>
        /// Returns true if created auction can be used and there is no need to call <see cref="EndCreate(AuctionBidsId)"/>
        /// </summary>
        public bool Finished => _auction is not null && (_auction.BuyNowOnly || _auction.Locked == false) && _startSuccess;

        public CreateAuctionService(Lazy<IAuctionImageRepository> auctionImages, Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions, CreateAuctionServiceData? serviceData = null)
        {
            _auctionImages = auctionImages;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
            ServiceData = serviceData;
            // if service data is provided it means that start was called
            if (serviceData != null) _startSuccess = true;
        }

        /// <summary>
        /// Creates auction in locked state and schedules it's end.
        /// </summary>
        /// <param name="auctionCreateSession">Session containing images provided by user</param>
        /// <param name="auctionArgs">Properties of created auction</param>
        /// <returns>Created auction in locked state</returns>
        public async Task<Auction> StartCreate(AuctionCreateSession auctionCreateSession, AuctionArgs auctionArgs)
        {
            if (_startSuccess) throw new InvalidOperationException("Start cannot be called twice");
            var auction = _auction = auctionCreateSession.CreateAuction(auctionArgs);

            // NOTE: there is no need to perform this action is system transaction:

            // * actions performed after this call can fail because scheduler call can be ignored when auction
            // is created in invalid state or not persisted
            await _auctionEndScheduler.Value.ScheduleAuctionEnd(auction);

            var imagesToSave = _auction.AuctionImages //TODO move to auction session
                .Where(image => image != null)
                .SelectMany(img => new string[3] { img.Size1Id, img.Size2Id, img.Size3Id }.AsEnumerable())
                .ToArray();

            // * actions performed after this call can fail because some service called periodically
            // may delete images assigned to not existing auctions
            _auctionImages.Value.UpdateManyMetadata(imagesToSave, new AuctionImageMetadata(null)
            {
                IsAssignedToAuction = true
            });

            _startSuccess = true;
            if (auction.BuyNowOnly) // buy now only auctions dont need further actions so Finished==true
            {
                return auction;
            }

            ServiceData = new(auction.LockIssuer, auction.AggregateId);
            return auction;
        }

        public Auction EndCreate(AuctionBidsId auctionBidsId)
        {
            if(ServiceData is null)
            {
                throw new InvalidOperationException($"{nameof(ServiceData)} is null. {nameof(StartCreate)} wasn't called.");
            }
            if(_auction is null)
                _auction = _auctions.Value.FindAuction(ServiceData.AuctionId);
            if (!_auction.Locked) throw new InvalidOperationException("Auction is not locked");
            _auction.AddAuctionBids(auctionBidsId);
            return _auction;
        }
    }
}
