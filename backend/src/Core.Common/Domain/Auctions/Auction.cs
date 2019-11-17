using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

[assembly: InternalsVisibleTo("IntegrationTests")]
[assembly: InternalsVisibleTo("Core.DomainModelTests")]
namespace Core.Common.Domain.Auctions
{
    internal static class AuctionConstantsFactory
    {
        public const int DEFAULT_MAX_IMAGES = 6;
        public const int DEFAULT_MAX_TODAY_MIN_OFFSET = 15;
        public const int DEFAULT_MIN_AUCTION_TIME_M = 120;
        public const int DEFAULT_MIN_TAGS = 1;

        internal static int MaxImages { get; set; } = DEFAULT_MAX_IMAGES;
        internal static int MaxTodayMinOffset { get; set; } = DEFAULT_MAX_TODAY_MIN_OFFSET;
        internal static int MinAuctionTimeM { get; set; } = DEFAULT_MIN_AUCTION_TIME_M;
        internal static int MinTags { get; set; } = DEFAULT_MIN_TAGS;
    }

    public partial class Auction : AggregateRoot<Auction>
    {
        public static int MAX_IMAGES => AuctionConstantsFactory.MaxImages;
        public static int MAX_TODAY_MIN_OFFSET => AuctionConstantsFactory.MaxTodayMinOffset;
        public static int MIN_AUCTION_TIME_M => AuctionConstantsFactory.MinAuctionTimeM;
        public static int MIN_TAGS => AuctionConstantsFactory.MinTags;

        private List<AuctionImage> _auctionImages = new List<AuctionImage>(new AuctionImage[MAX_IMAGES]);
        private List<Bid> _bids = new List<Bid>();
        private int _imgNum = 0;
        public AuctionName Name { get; private set; }
        public bool BuyNowOnly { get; private set; }
        public BuyNowPrice BuyNowPrice { get; private set; }
        public decimal ActualPrice { get; private set; }
        public IReadOnlyCollection<Bid> Bids => _bids;
        public AuctionDate StartDate { get; private set; }
        public AuctionDate EndDate { get; private set; }
        public UserIdentity Owner { get; private set; }
        public Product Product { get; private set; }
        public UserIdentity Buyer { get; private set; } = UserIdentity.Empty;
        public Category Category { get; private set; }
        public bool Completed { get; private set; }
        public bool Canceled { get; private set; }
        public Tag[] Tags { get; private set; }
        public IReadOnlyList<AuctionImage> AuctionImages => _auctionImages;

        public Auction()
        {
        }

        internal Auction(AuctionArgs auctionArgs)
        {
            Create(auctionArgs, true);
            AddEvent(new AuctionCreated(AggregateId, auctionArgs));
        }

        private void Create(AuctionArgs auctionArgs, bool compareToNow)
        {
            ValidateDates(auctionArgs.StartDate, auctionArgs.EndDate, compareToNow);
            BuyNowPrice = auctionArgs.BuyNowPrice;
            StartDate = auctionArgs.StartDate;
            EndDate = auctionArgs.EndDate;
            Owner = auctionArgs.Creator;
            Product = auctionArgs.Product;
            Category = auctionArgs.Category;
            BuyNowOnly = auctionArgs.BuyNowOnly;
            Name = auctionArgs.Name;
            if (auctionArgs.Tags.Length < MIN_TAGS)
            {
                throw new DomainException("Not enough auction tags");
            }
            else
            {
                Tag[] tagsToCpy = auctionArgs.Tags.Distinct().Select(s => (Tag)s).ToArray();
                Tags = new Tag[tagsToCpy.Length];
                Array.Copy(tagsToCpy, Tags, tagsToCpy.Length);
            }
            if (auctionArgs.AuctionImages != null)
            {
                foreach (var img in auctionArgs.AuctionImages)
                {
                    if (img != null)
                    {
                        _auctionImages[_imgNum++] = img;
                    }
                }
            }
        }

        private void ValidateDates(AuctionDate startDate, AuctionDate endDate, bool compareToNow)
        {
            bool startLessThanOffset = false;
            if (compareToNow)
            {
                startLessThanOffset = (DateTime.UtcNow.Subtract(startDate).TotalMinutes > MAX_TODAY_MIN_OFFSET);
            }

            if (startLessThanOffset)
            {
                throw new DomainException("");
            }

            bool hasMinTime = (endDate.Value.Subtract(startDate)).TotalMinutes >= MIN_AUCTION_TIME_M;

            if (!hasMinTime)
            {
                throw new DomainException("");
            }
            bool startLessThanEnd = startDate.Value.CompareTo(endDate) == -1;

            if (!startLessThanEnd)
            {
                throw new DomainException("Invalid auction date");
            }
        }

        private void ThrowIfCompletedOrCanceled()
        {
            if (Completed)
            {
                throw new DomainException("Auction has been completed");
            }

            if (Canceled)
            {
                throw new DomainException("Auction has been canceled");
            }
        }

        private void ThrowIfBuyNowOnly()
        {
            if (BuyNowOnly)
            {
                throw new DomainException("Auction is buyNow only");
            }
        }




        public void Raise(Bid bid)
        {
            ThrowIfBuyNowOnly();
            ThrowIfCompletedOrCanceled();
            if (bid.AuctionId != AggregateId)
            {
                throw new DomainException("Invalid auction id");
            }
            if (bid.Price <= ActualPrice)
            {
                throw new DomainException("Price is to low");
            }
            if (bid.UserIdentity.UserId == Owner.UserId)
            {
                throw new DomainException("Auction cannot be raised by auction creator");
            }

            _bids.Add(bid);
            ActualPrice = bid.Price;
            AddEvent(new AuctionRaised(bid));
        }

        public void CancelBid(Bid bid)
        {
            ThrowIfBuyNowOnly();
            ThrowIfCompletedOrCanceled();

            var existingBid = _bids.FirstOrDefault(b => b.BidId.Equals(bid.BidId));
            if (existingBid == null)
            {
                throw new DomainException($"Cannot find bid with BidId: {bid.BidId}");
            }

            if (DateTime.Now.Subtract(existingBid.DateCreated)
                    .Minutes > 10)
            {
                throw new DomainException($"Bid ({bid.BidId}) was created more than 10 minutes ago");
            }

            _bids.Remove(existingBid);
            AddEvent(new BidCanceled(existingBid));
            if (existingBid.Price.Equals(ActualPrice))
            {
                var topBid = Bids.First(b => b.Price == Bids.Max(mbid => mbid.Price));
                ActualPrice = topBid.Price;
                AddEvent(new AuctionRaised(topBid));
            }
        }

        public void ChangeEndDate(AuctionDate newEndDate)
        {
            ThrowIfCompletedOrCanceled();
            ValidateDates(StartDate, newEndDate, false);
            EndDate = newEndDate;
        }

        public void CancelAuction()
        {
            ThrowIfCompletedOrCanceled();
            Canceled = true;
            AddEvent(new AuctionCanceled(AggregateId));
        }

        public void EndAuction()
        {
            ThrowIfCompletedOrCanceled();
            Completed = true;
            var winningBid = Bids.FirstOrDefault(b => b.Price == Bids.Max(bid => bid.Price));
            Buyer = winningBid?.UserIdentity;
            AddEvent(new AuctionCompleted(AggregateId, winningBid));
        }

        public void BuyNow(UserIdentity userIdentity)
        {
            if (BuyNowPrice == 0)
            {
                throw new DomainException($"Aution {AggregateId} buy now price has not been set");
            }

            ThrowIfCompletedOrCanceled();

            Buyer = userIdentity ?? throw new DomainException($"Null user identity");
            Completed = true;
            AddEvent(new AuctionBought(AggregateId, Buyer));
        }

        public void AddImage(AuctionImage img)
        {
            if (_imgNum == _auctionImages.Capacity - 1)
            {
                throw new DomainException("Cannot add more auction images");
            }

            _auctionImages[_imgNum] = img;
            AddEvent(new AuctionImageAdded(img, _imgNum++, AggregateId));
        }

        public void RemoveBid(Bid bid)
        {
            var existingBid = _bids.FirstOrDefault(b => b.BidId.Equals(bid.BidId));
            if (existingBid == null)
            {
                throw new DomainException($"Cannot find bid with BidId: {bid.BidId}");
            }

            _bids.Remove(existingBid);

            AddEvent(new BidRemoved(bid));
            if (existingBid.Price.Equals(ActualPrice))
            {
                var topBid = Bids.First(b => b.Price == Bids.Max(mbid => mbid.Price));
                ActualPrice = topBid.Price;
            }

        }
    }
}