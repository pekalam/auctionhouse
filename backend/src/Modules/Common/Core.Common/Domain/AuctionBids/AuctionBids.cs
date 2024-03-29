﻿using Core.Common.Exceptions;
using Core.DomainFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Common.Domain.AuctionBids
{
    internal static class AuctionBidsExtensions
    {
        public static Bid WithMaximumPrice(this IEnumerable<Bid> bids) => bids.First(b => b.Price == bids.Max(m => m.Price));
    }

    public class AuctionBids : AggregateRoot<AuctionBids, AuctionBidsId>
    {
        private List<Bid> _bids;
        public AuctionId AuctionId { get; private set; }
        public UserId OwnerId { get; private set; }
        public UserId WinnerId { get; private set; }
        public BidId WinnerBidId { get; private set; }
        public decimal CurrentPrice { get; private set; }
        public IReadOnlyList<Bid> Bids => _bids;

        /// <summary>
        /// Temporary solution
        /// </summary>
        public bool Cancelled { get; private set; }
        public bool Completed { get; private set; }

        public AuctionBids() { }

        public AuctionBids(AuctionBidsId aggregateId, Bid[] bids, AuctionId auctionId, UserId ownerId, UserId winnerId, decimal currentPrice, bool cancelled, bool completed)
        {
            AggregateId = new AuctionBidsId(auctionId.Value); //temporary solution TODO
            _bids = bids.ToList();
            AuctionId = auctionId;
            OwnerId = ownerId;
            WinnerId = winnerId;
            CurrentPrice = currentPrice;
            Cancelled = cancelled;
            Completed = completed;
            AddEvent(new Events.V1.AuctionBidsCreated { AuctionBidsId = AggregateId, AuctionId = AuctionId, OwnerId = OwnerId });//temporary solution TODO
        }

        public static AuctionBids CreateNew(AuctionId auctionId, UserId ownerId)
        {
            return new AuctionBids(AuctionBidsId.New(), new Bid[0], auctionId, ownerId, null, 0m, false, false);//TODO null
        }

        public Bid TryRaise(UserId userId, decimal price)
        {
            if (userId == OwnerId)
            {
                throw new InvalidUserIdException("Auction cannot be raised by owner");
            }

            Bid bid;
            if (price <= CurrentPrice)
            {
                bid = Bid.CreateNotAccepted(this, userId, price);
            }
            else
            {
                bid = Bid.CreateAccepted(this, userId, price);
            }

            _bids.Add(bid);
            if (bid.Accepted)
            {
                CurrentPrice = price;
                WinnerId = userId;
                WinnerBidId = bid.Id;
                AddEvent(new AuctionPriceRised { AuctionBidsId = AggregateId, BidId = bid.Id, AuctionId = AuctionId, Date = bid.Date, CurrentPrice = CurrentPrice, WinnerId = userId });
            }
            else
            {
                AddEvent(new AuctionBidNotAccepted { AuctionBidsId = AggregateId, BidId=bid.Id, AuctionId = AuctionId, Date = bid.Date, Price = price, UserId = userId });
            }

            return bid;
        }

        public void CancelBid(BidId bidId)
        {
            var bid = _bids.FirstOrDefault(b => b.Id == bidId);
            if(bid == null)
            {
                throw new DomainException($"Could not find bid {bidId}");
            }

            bid.Cancel();
            _bids.Remove(bid);
            if(_bids.Count == 0)
            {
                WinnerBidId = null;
                WinnerId = null;
                CurrentPrice = default;
                AddEvent(new AuctionHaveNoParticipants { AuctionBidsId = AggregateId, AuctionId = AuctionId, Price = CurrentPrice });
            }
            else if(bid.Id == WinnerBidId)
            {
                var maxBid = _bids.WithMaximumPrice();
                WinnerBidId = maxBid.Id;
                WinnerId = maxBid.UserId;
                CurrentPrice = maxBid.Price;
                AddEvent(new AuctionPriceRised { AuctionBidsId = AggregateId, AuctionId = AuctionId, Date = maxBid.Date, CurrentPrice = maxBid.Price, WinnerId = maxBid.UserId });
            }
        }

        public void Complete()
        {
            if (Cancelled)
            {
                throw new DomainException("Auction is cancelled");
            }

            Completed = true;
            AddEvent(new AuctionCompleted { AuctionBidsId = AggregateId, AuctionId = AuctionId, CurrentPrice = CurrentPrice, WinnerId = WinnerId });
        }

        protected override void Apply(Event @event)
        {
            switch (@event)
            {
                case Events.V1.AuctionBidsCreated e:
                    AggregateId = new AuctionBidsId(e.AuctionBidsId);
                    AuctionId = new AuctionId(e.AuctionId);
                    OwnerId = new UserId(e.OwnerId);
                    _bids = new List<Bid>();
                    break;
                case AuctionPriceRised e:
                    {
                        WinnerBidId = new BidId(e.BidId);
                        CurrentPrice = e.CurrentPrice;
                        WinnerId = new UserId(e.WinnerId);
                        var bid = new Bid(new BidId(e.BidId), new UserId(e.WinnerId), e.CurrentPrice, e.Date, true, false, this);
                        _bids.Add(bid);
                    }
                    break;
                case AuctionBidNotAccepted e:
                    {
                        var bid = new Bid(new BidId(e.BidId), new UserId(e.UserId), e.Price, e.Date, false, false, this);
                        _bids.Add(bid);
                    }
                    break;

                //bids
                case BidCancelled e:
                    _bids.First(b => b.Id == e.BidId).ApplyInternal(e);
                    break;
                default:
                    break;
            }
        }
    }

    public sealed class AuctionBidsId : GuidId<AuctionBidsId>
    {
        public AuctionBidsId(Guid value) : base(value) { }
        public static AuctionBidsId New() => new AuctionBidsId(Guid.NewGuid());
    }

    public class InvalidUserIdException : DomainException
    {
        public InvalidUserIdException(string message) : base(message)
        {
        }

        public InvalidUserIdException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
