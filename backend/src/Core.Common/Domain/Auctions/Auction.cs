using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Auctions.Events.Update;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;

[assembly: InternalsVisibleTo("Test.IntegrationTests")]
[assembly: InternalsVisibleTo("Test.DomainModelTests")]
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

    public partial class Auction : AggregateRoot<Auction, AuctionUpdateEventGroup>
    {
        public static int MAX_IMAGES => AuctionConstantsFactory.MaxImages;
        public static int MAX_TODAY_MIN_OFFSET => AuctionConstantsFactory.MaxTodayMinOffset;
        public static int MIN_AUCTION_TIME_M => AuctionConstantsFactory.MinAuctionTimeM;
        public static int MIN_TAGS => AuctionConstantsFactory.MinTags;

        private List<AuctionImage> _auctionImages = new List<AuctionImage>(new AuctionImage[MAX_IMAGES]);
        private List<Bid> _bids = new List<Bid>();
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
                var i = 0;
                foreach (var img in auctionArgs.AuctionImages)
                {
                    if (img != null)
                    {
                        _auctionImages[i++] = img;
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


        private void Raise(Bid bid)
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

        public void Raise(User user, decimal newPrice)
        {
            if (!user.IsRegistered)
            {
                throw new DomainException("User is not registered");
            }
            var bid = new Bid(AggregateId, user.UserIdentity, newPrice, DateTime.UtcNow);
            Raise(bid);

            user.WithdrawCredits(newPrice);
        }

        private void CancelBid(Bid bid, bool fromEvents)
        {
            ThrowIfBuyNowOnly();
            ThrowIfCompletedOrCanceled();

            var existingBid = _bids.FirstOrDefault(b => b.BidId.Equals(bid.BidId));
            if (existingBid == null)
            {
                throw new DomainException($"Cannot find bid with BidId: {bid.BidId}");
            }

            if (!fromEvents && DateTime.UtcNow.Subtract(existingBid.DateCreated)
                    .Minutes > 10)
            {
                throw new DomainException($"Bid ({bid.BidId}) was created more than 10 minutes ago");
            }

            _bids.Remove(existingBid);
            Bid newWinningBid = null;
            if (existingBid.Price.Equals(ActualPrice))
            {
                var topBid = Bids.FirstOrDefault(b => b.Price == Bids.Max(mbid => mbid.Price));
                if (topBid != null)
                {
                    ActualPrice = topBid.Price;
                    newWinningBid = topBid;
                }
                else
                {
                    ActualPrice = 0;
                    newWinningBid = existingBid;
                }
            }
            AddEvent(new BidCanceled(existingBid, newWinningBid));
        }

        public void CancelBid(User user, Bid bid)
        {
            if (!bid.UserIdentity.Equals(user.UserIdentity))
            {
                throw new DomainException($"Bid cannot be canceled by user {user.UserIdentity.UserId}");
            }

            CancelBid(bid, false);

            user.ReturnCredits(bid.Price);
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

        private void BuyNow(UserIdentity buyer)
        {
            Buyer = buyer;
            Completed = true;
            AddEvent(new AuctionBought(AggregateId, buyer));
        }

        public void BuyNow(User user)
        {
            if (BuyNowPrice == 0)
            {
                throw new DomainException($"Aution {AggregateId} buy now price has not been set");
            }

            ThrowIfCompletedOrCanceled();

            user.CheckIsRegistered();
            user.WithdrawCredits(BuyNowPrice);
            BuyNow(user.UserIdentity);
        }

        public void AddImage(AuctionImage img)
        {
            if (!_auctionImages.Contains(null))
            {
                throw new DomainException("Cannot add more auction images");
            }

            var ind = _auctionImages.IndexOf(null);
            _auctionImages[ind] = img;
            AddEvent(new AuctionImageAdded(img, ind, AggregateId, Owner));
        }

        public AuctionImage ReplaceImage(AuctionImage img, int imgNum)
        {
            if (imgNum > _auctionImages.Capacity - 1)
            {
                throw new DomainException($"Cannot replace {imgNum} image");
            }

            var replaced = _auctionImages[imgNum];
            _auctionImages[imgNum] = img;
            AddEvent(new AuctionImageReplaced(AggregateId, imgNum, img, Owner));
            return replaced;
        }

        public AuctionImage RemoveImage(int imgNum)
        {
            if (imgNum > _auctionImages.Capacity - 1)
            {
                throw new DomainException($"Cannot remove {imgNum} image");
            }

            var removed = _auctionImages[imgNum];
            _auctionImages[imgNum] = null;
            AddEvent(new AuctionImageRemoved(AggregateId, imgNum, Owner));
            return removed;
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




        public void UpdateBuyNowPrice(BuyNowPrice newPrice)
        {
            if (newPrice == 0m && BuyNowOnly)
            {
                throw new DomainException("Cannot set buy now price to 0 if auction is buyNowOnly");
            }
            if (newPrice.Equals(BuyNowPrice)) { return; }
            BuyNowPrice = newPrice;
            AddUpdateEvent(new AuctionBuyNowPriceChanged(AggregateId, newPrice, Owner));
        }

        public void UpdateTags(Tag[] tags)
        {
            if (tags.Length < MIN_TAGS)
            {
                throw new DomainException("Not enough auction tags");
            }
            if(tags.Length >= Tags.Length && tags.Except(Tags).Count() == 0) { return;}
            Tags = tags;
            AddUpdateEvent(new AuctionTagsChanged(AggregateId, tags));
        }

        public void UpdateEndDate(AuctionDate newEndDate)
        {
            ThrowIfCompletedOrCanceled();
            ValidateDates(StartDate, newEndDate, false);
            if (newEndDate.Value.CompareTo(EndDate.Value) == 0) { return; }
            //TODO
            EndDate = newEndDate;
            AddUpdateEvent(new AuctionEndDateChanged(AggregateId, newEndDate));
        }

        public void UpdateCategory(Category category)
        {
            if(category.Equals(Category)) { return; }
            Category = category;
            AddUpdateEvent(new AuctionCategoryChanged(AggregateId, category));
        }

        public void UpdateName(AuctionName name)
        {
            if (name.Equals(Name)) { return; }
            Name = name;
            AddUpdateEvent(new AuctionNameChanged(AggregateId, name));
        }

        public void UpdateDescription(string description)
        {
            if(Product.Description.Equals(description)) { return; }
            Product.Description = description;
            AddUpdateEvent(new AuctionDescriptionChanged(AggregateId, description));
        }

    }
}