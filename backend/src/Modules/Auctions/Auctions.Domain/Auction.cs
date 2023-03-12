using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Auctions.DomainEvents.Update;
using Core.Common.Domain;
using Core.DomainFramework;

namespace Auctions.Domain
{
    public static class AuctionConstantsFactoryValueProvider
    {
        public static Func<string, object?>? TestValueFactory { get; set; }
    }

    public static class AuctionConstantsFactory
    {

        public const int DEFAULT_MAX_IMAGES = 6;
        public const int DEFAULT_MAX_TODAY_MIN_OFFSET = 15;
        public const int DEFAULT_MIN_AUCTION_TIME_M = 120;
        public const int DEFAULT_MIN_TAGS = 1;
        public const int DEFAULT_BUY_CANCELLATION_TIME = 1000 * 60 * 5;

        public static int MaxImages { get; } 
            = ((int?)AuctionConstantsFactoryValueProvider.TestValueFactory?.Invoke(nameof(MaxImages))) 
            ?? DEFAULT_MAX_IMAGES;
        public static int MaxTodayMinOffset { get; } 
            = ((int?)AuctionConstantsFactoryValueProvider.TestValueFactory?.Invoke(nameof(MaxTodayMinOffset))) 
            ?? DEFAULT_MAX_TODAY_MIN_OFFSET;
        public static int MinAuctionTimeM { get; }
            = ((int?)AuctionConstantsFactoryValueProvider.TestValueFactory?.Invoke(nameof(MinAuctionTimeM)))
            ?? DEFAULT_MIN_AUCTION_TIME_M;
        public static int MinTags { get; } 
            = ((int?)AuctionConstantsFactoryValueProvider.TestValueFactory?.Invoke(nameof(MinTags)))
            ?? DEFAULT_MIN_TAGS;
        public static int BuyCancellationTime { get;  }
            = ((int?)AuctionConstantsFactoryValueProvider.TestValueFactory?.Invoke(nameof(BuyCancellationTime)))
            ?? DEFAULT_BUY_CANCELLATION_TIME;
    }

    public class AuctionId : ValueObject
    {
        public Guid Value { get; }

        public AuctionId(Guid value)
        {
            Value = value;
        }

        public static AuctionId New() => new AuctionId(Guid.NewGuid());

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
        public static implicit operator Guid(AuctionId id) => id.Value;
        public static implicit operator AuctionId(Guid guid) => new AuctionId(guid);
    }

    public partial class Auction : AggregateRoot<Auction, AuctionId, AuctionUpdateEventGroup>
    {
        public static int MAX_IMAGES => AuctionConstantsFactory.MaxImages;
        public static int MAX_TODAY_MIN_OFFSET => AuctionConstantsFactory.MaxTodayMinOffset;
        public static int MIN_AUCTION_TIME_M => AuctionConstantsFactory.MinAuctionTimeM;
        public static int MIN_TAGS => AuctionConstantsFactory.MinTags;

        private AuctionImages _auctionImages = new();
        public AuctionName Name { get; private set; }
        public bool BuyNowOnly { get; private set; }
        public BuyNowPrice? BuyNowPrice { get; private set; }
        public AuctionDate StartDate { get; private set; }
        public AuctionDate EndDate { get; private set; }
        public UserId Owner { get; private set; }
        public Product Product { get; private set; }
        public UserId Buyer { get; private set; } = UserId.Empty;
        public CategoryId[] Categories { get; private set; }
        public bool Completed { get; private set; }
        public bool Canceled { get; private set; }
        public Tag[] Tags { get; private set; }
        public IReadOnlyList<AuctionImage?> AuctionImages => _auctionImages.ToList();

        public AuctionBidsId? AuctionBidsId { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Auction()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Auction(AuctionArgs auctionArgs)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Create(auctionArgs, true);
            AggregateId = AuctionId.New();
            AddEvent(new AuctionCreated()
            {
                AuctionId = AggregateId,
                DateCreated = DateTime.UtcNow,
                Tags = auctionArgs.Tags.Select(t => t.Value).ToArray(),
                AggVersion = Version,
                Name = auctionArgs.Name,
                BuyNowOnly = auctionArgs.BuyNowOnly,
                BuyNowPrice = auctionArgs.BuyNowPrice?.Value,
                Category = auctionArgs.Categories.Select(i => (int)i).ToArray(),
                EndDate = auctionArgs.EndDate,
                Owner = auctionArgs.Owner,
                ProductCondition = (int)auctionArgs.Product.Condition,
                ProductName = auctionArgs.Product.Name,
                ProductDescription = auctionArgs.Product.Description,
                StartDate = auctionArgs.StartDate,
                AuctionImagesSize1Id = auctionArgs.AuctionImages.Size1Ids.ToArray(),
                AuctionImagesSize2Id = auctionArgs.AuctionImages.Size2Ids.ToArray(),
                AuctionImagesSize3Id = auctionArgs.AuctionImages.Size3Ids.ToArray(),
            });
        }

        private void Create(AuctionArgs auctionArgs, bool compareToNow)
        {
            ValidateDates(auctionArgs.StartDate, auctionArgs.EndDate, compareToNow);
            BuyNowPrice = auctionArgs.BuyNowPrice;
            StartDate = auctionArgs.StartDate;
            EndDate = auctionArgs.EndDate;
            Owner = auctionArgs.Owner;
            Product = auctionArgs.Product;
            Categories = auctionArgs.Categories;
            BuyNowOnly = auctionArgs.BuyNowOnly;
            Name = auctionArgs.Name;
            if(Categories.Length != 3)
            {
                throw new DomainException("Auction must have exactly 3 categories assigned");
            }
            if (BuyNowOnly && BuyNowPrice is null)
            {
                throw new DomainException("Cannot create buyNowOnly auction with null buyNowPrice");
            }
            if (BuyNowOnly && BuyNowPrice!.Value == 0)
            {
                throw new DomainException("Cannot create buyNowOnly auction with buyNowPrice 0");
            }
            if (auctionArgs.Tags.Length < MIN_TAGS)
            {
                throw new DomainException("Not enough auction tags");
            }
            else
            {
                if (!(auctionArgs.Tags.Select(tag => tag.Value).Distinct().Count() == auctionArgs.Tags.Length))
                {
                    throw new DomainException("Tags array does not contain unique tags");
                }
                Tag[] tagsToCpy = auctionArgs.Tags;
                Tags = new Tag[tagsToCpy.Length];
                Array.Copy(tagsToCpy, Tags, tagsToCpy.Length);
            }
            if (auctionArgs.AuctionImages != null)
            {
                _auctionImages = auctionArgs.AuctionImages;
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
                throw new DomainException("Auction does not last long enough");
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

        /// <summary>
        /// Sets auction as bought. Next step is to call <see cref="ConfirmBuy(Guid, IAuctionBuyCancellationScheduler)"/> method.
        /// </summary>
        /// <returns>Confirmation Id</returns>
        public async Task Buy(UserId buyerId, string paymentMethod, IAuctionPaymentVerification paymentVerification, IAuctionBuyCancellationScheduler buyCancellationScheduler)
        {
            if (!BuyNowOnly && BuyNowPrice is null)
            {
                throw new DomainException("Cannot buy auction that is not of buy now type");
            }
            if (buyerId == Owner)
            {
                throw new DomainException("Cannot be bought by owner");
            }
            if (Buyer != UserId.Empty && !Completed)
            {
                throw new DomainException("Auction is already bought");
            }
            if (Completed)
            {
                throw new DomainException("Auction cannot be bought if it's completed");
            }

            var paymentPassedVerification = await VerifyPayment(buyerId, paymentMethod, paymentVerification);
            if (!paymentPassedVerification)
            {
                throw new DomainException($"Payment started by buyer {buyerId} didn't pass verification");
            }

            ApplyEvent(AddEvent(new Events.V1.AuctionBought { Price = BuyNowPrice!.Value, AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this), BuyerId = buyerId, PaymentMethodName = paymentMethod, }));

            var cancelTime = TimeOnly.FromTimeSpan(TimeSpan.FromMilliseconds(AuctionConstantsFactory.BuyCancellationTime));
            buyCancellationScheduler.ScheduleAuctionBuyCancellation(AggregateId, cancelTime);
        }

        private async Task<bool> VerifyPayment(UserId buyerId, string paymentMethod, IAuctionPaymentVerification paymentVerification)
        {
            try
            {
                return await paymentVerification.Verification(this, buyerId, paymentMethod);
            }
            catch (Exception)
            {
                //TODO log
                return false;
            }
        }

        private void TryCancelScheduledAuctionBuyCancellation(IAuctionBuyCancellationScheduler auctionBuyCancelScheduler)
        {
            try
            {
                auctionBuyCancelScheduler.Cancel(AggregateId);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Confirms transaction buying for a buyerId.
        /// </summary>
        /// <param name="buyerId"></param>
        /// <exception cref="DomainException"></exception>
        public bool ConfirmBuy(UserId buyerId, IAuctionBuyCancellationScheduler auctionBuyCancelScheduler)
        {
            if(Completed && Buyer == buyerId)
            {
                return true;
            }
            if(Buyer == UserId.Empty || buyerId == UserId.Empty)
            {
                return false;
            }
            if (Buyer != buyerId || Completed)
            {
                ApplyEvent(AddEvent(new Events.V1.AuctionBuyConfirmationFailed { AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this) }));
                return false;
            }

            // if scheduler will throw it doesn't matter anyway because it will not find this auction and additionally won't cancel completed
            TryCancelScheduledAuctionBuyCancellation(auctionBuyCancelScheduler);

            ApplyEvent(AddEvent(new Events.V1.AuctionBuyConfirmed { AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this), 
                BuyerId = Buyer.Value, EndDate = DateTime.UtcNow }));
            return true;
        }

        public bool CancelBuy(UserId buyerId, IAuctionBuyCancellationScheduler auctionBuyCancelScheduler)
        {
            if (Completed)
            {
                return false;
            }
            if (Buyer != buyerId) //when someone bought the auction
            {
                //this event doesn't result in state change
                //TODO: is it required?
                ApplyEvent(AddEvent(new Events.V1.AuctionBuyCanceledConcurrently { AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this) }));
                return false;
            }

            // this cannot fail
            auctionBuyCancelScheduler.Cancel(AggregateId);

            ApplyEvent(AddEvent(new Events.V1.AuctionBuyCanceled { AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this) }));
            return true;
        }

        internal void CancelBuy()
        {
            if(Buyer == UserId.Empty || Completed)
            {
                return;
            }

            ApplyEvent(AddEvent(new Events.V1.AuctionBuyCanceled { AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this) }));
        }


        public void AddAuctionBids(AuctionBidsId auctionBidsId)
        {
            if(AuctionBidsId is not null)
            {
                throw new DomainException("");
            }
            if (BuyNowOnly) throw new DomainException($"Cannot add AuctionBidsId to auction when {nameof(BuyNowOnly)}=true");
            ApplyEvent(AddEvent(new AuctionBidsAdded() { AuctionId = AggregateId, CategoryIds = CategoryIdsFactory.Create(this), AuctionBidsId = auctionBidsId.Value }));
        }


        public void AddImage(AuctionImage img)
        {
            var ind = _auctionImages.AddImage(img);
            AddEvent(new AuctionImageAdded(ind, AggregateId, Owner, img.Size1Id, img.Size2Id, img.Size3Id));
        }

        public AuctionImage? ReplaceImage(AuctionImage img, int imgNum)
        {
            var replaced = _auctionImages[imgNum];
            _auctionImages[imgNum] = img;
            AddEvent(new AuctionImageReplaced(AggregateId, imgNum, Owner, img.Size1Id, img.Size2Id, img.Size3Id));
            return replaced;
        }

        public AuctionImage? RemoveImage(int imgNum)
        {
            if (_auctionImages[imgNum] is null)
            {
                throw new DomainException($"Cannot remove {imgNum} image");
            }

            var removed = _auctionImages[imgNum];
            _auctionImages[imgNum] = null;
            AddEvent(new AuctionImageRemoved(AggregateId, imgNum, Owner));
            return removed;
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
            if (tags.Length >= Tags.Length && tags.Except(Tags).Count() == 0) { return; }
            Tags = tags;
            AddUpdateEvent(new AuctionTagsChanged(AggregateId, tags.Select(t => t.Value).ToArray()));
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

        public void UpdateCategories(CategoryId[] category)
        {
            if (category.Equals(Categories)) { return; }
            Categories = category;
            AddUpdateEvent(new AuctionCategoriesChanged(AggregateId, category.Select(c => (int)c).ToArray()));
        }

        public void UpdateName(AuctionName name)
        {
            if (name.Equals(Name)) { return; }
            Name = name;
            AddUpdateEvent(new AuctionNameChanged(AggregateId, name));
        }

        public void UpdateDescription(string description)
        {
            if (Product.Description.Equals(description)) { return; }
            Product.Description = description;
            AddUpdateEvent(new AuctionDescriptionChanged(AggregateId, description));
        }

        public void EndAuction()
        {
            if (Completed)
            {
                throw new DomainException("Cannot end completed auction");
            }
            if(Buyer != UserId.Empty)
            {
                throw new DomainException("Cannot end bought auction");
            }

            ApplyEvent(AddEvent(new AuctionEnded() { AuctionId = AggregateId }));
        }
    }
}