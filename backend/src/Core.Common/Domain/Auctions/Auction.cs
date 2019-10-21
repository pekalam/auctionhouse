using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions
{
    public partial class Auction : AggregateRoot<Auction>
    {
        public const int MAX_IMAGES = 6;

        public decimal? BuyNowPrice { get; private set; }
        public decimal? ActualPrice { get; private set; }
        public ICollection<Bid> Bids { get; private set; } = new List<Bid>();
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public UserIdentity Owner { get; private set; }
        public bool Completed { get; private set; }
        public Product Product { get; private set; }
        public UserIdentity Buyer { get; private set; } = UserIdentity.Empty;
        public Category Category { get; private set; }
        public bool Canceled { get; private set; } = false;
        private List<AuctionImage> _auctionImages = new List<AuctionImage>(new AuctionImage[MAX_IMAGES]);
        public IReadOnlyList<AuctionImage> AuctionImages => _auctionImages;

        private int _imgNum = 0;

        public Auction()
        {
        }

        internal Auction(decimal? buyNowPrice, DateTime startDate, DateTime endDate, UserIdentity creator,
            Product product, Category category, AuctionImage[] auctionImages = null)
        {
            Create(buyNowPrice, startDate, endDate, creator, product, category, auctionImages);
            AddEvent(new AuctionCreated(AggregateId, product, buyNowPrice, startDate, endDate, creator, category,
                _auctionImages.ToArray()));
        }

        private void Create(decimal? buyNowPrice, DateTime startDate, DateTime endDate, UserIdentity creator,
            Product product, Category category, AuctionImage[] auctionImages, bool compareToNow = true)
        {
            ValidateDates(startDate, endDate, compareToNow);
            BuyNowPrice = buyNowPrice;
            StartDate = startDate;
            EndDate = endDate;
            Owner = creator;
            Product = product;
            Category = category;
            if (auctionImages != null)
            {
                foreach (var img in auctionImages)
                {
                    if (img != null)
                    {
                        _auctionImages[_imgNum++] = img;
                    }
                }
            }
        }

        private void ValidateDates(DateTime startDate, DateTime endDate, bool compareToNow)
        {
            bool startLessThanNow = false;
            if (compareToNow)
            {
                startLessThanNow = startDate.Ticks < DateTime.UtcNow.Ticks;
            }

            bool startLessThanEnd = startDate.CompareTo(endDate) == -1;
            bool startNotEqualMax = !startDate.Equals(DateTime.MaxValue);
            bool startNotEqualMin = !startDate.Equals(DateTime.MinValue);
            bool endNotEqualMax = !endDate.Equals(DateTime.MaxValue);
            bool endNotEqualMin = !endDate.Equals(DateTime.MinValue);

            if (!startLessThanEnd || startLessThanNow || !startNotEqualMax || !startNotEqualMin || !endNotEqualMin ||
                !endNotEqualMax)
            {
                throw new DomainException("Invalid auction date");
            }
        }

        private void CheckAuctionCompletedOrCanceled()
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

        public void Raise(Bid bid)
        {
            CheckAuctionCompletedOrCanceled();
            if (ActualPrice.HasValue && bid.Price <= ActualPrice || bid.Price == 0)
            {
                throw new DomainException("Price is to low");
            }

            if (bid.UserIdentity == Owner)
            {
                throw new DomainException("Auction cannot be raised by auction creator");
            }

            Bids.Add(bid);
            ActualPrice = bid.Price;
            AddEvent(new AuctionRaised(bid));
        }

        public void CancelBid(Bid bid)
        {
            var existingBid = Bids.FirstOrDefault(b => b.BidId.Equals(bid.BidId));
            if (existingBid == null)
            {
                throw new DomainException($"Cannot find bid with BidId: {bid.BidId}");
            }

            if (DateTime.Now.Subtract(existingBid.DateCreated).Minutes > 10)
            {
                throw new DomainException($"Bid ({bid.BidId}) was created more than 10 minutes ago");
            }

            Bids.Remove(existingBid);
            AddEvent(new BidCanceled(existingBid));
            if (existingBid.Price.Equals(ActualPrice))
            {
                var topBid = Bids.First(b => b.Price == Bids.Max(mbid => mbid.Price));
                ActualPrice = topBid.Price;
                AddEvent(new AuctionRaised(topBid));
            }
        }

        public void ChangeEndDate(DateTime newEndDate)
        {
            CheckAuctionCompletedOrCanceled();
            ValidateDates(StartDate, newEndDate, false);
            EndDate = newEndDate;
        }

        public void CancelAuction()
        {
            CheckAuctionCompletedOrCanceled();
            Canceled = true;
            AddEvent(new AuctionCanceled(AggregateId));
        }

        public void EndAuction()
        {
            CheckAuctionCompletedOrCanceled();
            Completed = true;
            var winningBid = Bids.FirstOrDefault(b => b.Price == Bids.Max(bid => bid.Price));
            Buyer = winningBid?.UserIdentity;
            AddEvent(new AuctionCompleted(AggregateId, winningBid));
        }

        public void BuyNow(UserIdentity userIdentity)
        {
            if (!BuyNowPrice.HasValue)
            {
                throw new DomainException($"Aution {AggregateId} buy now price has not been set");
            }

            CheckAuctionCompletedOrCanceled();

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
    }
}