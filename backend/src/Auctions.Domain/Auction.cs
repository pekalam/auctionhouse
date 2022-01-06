using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Auctions.DomainEvents.Update;
using Core.Common.Domain;
using Core.DomainFramework;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.IntegrationTests")]
[assembly: InternalsVisibleTo("Test.DomainModelTests")]
namespace Auctions.Domain
{

    public static class AuctionConstantsFactory
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

    public class UserId : ValueObject
    {
        public static readonly UserId Empty = new UserId(Guid.Empty);

        public Guid Value { get; }

        public UserId(Guid value)
        {
            Value = value;
        }

        public static UserId New() => new UserId(Guid.NewGuid());

        public override string ToString() => Value.ToString();
        public static implicit operator Guid(UserId id) => id.Value;
        public static implicit operator UserId(Guid guid) => new UserId(guid);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class CategoryId : ValueObject
    {
        public int Value { get; }

        public CategoryId(int value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
        public static implicit operator CategoryId(int value) => new CategoryId(value);
        public static implicit operator int(CategoryId id) => id.Value;
    }

    public class AuctionBidsId : GuidId<AuctionBidsId>
    {
        public AuctionBidsId(Guid value) : base(value)
        {
        }
    }

    public partial class Auction : AggregateRoot<Auction, AuctionId, AuctionUpdateEventGroup>
    {
        public static int MAX_IMAGES => AuctionConstantsFactory.MaxImages;
        public static int MAX_TODAY_MIN_OFFSET => AuctionConstantsFactory.MaxTodayMinOffset;
        public static int MIN_AUCTION_TIME_M => AuctionConstantsFactory.MinAuctionTimeM;
        public static int MIN_TAGS => AuctionConstantsFactory.MinTags;

        private readonly List<AuctionImage?> _auctionImages = new(new AuctionImage[MAX_IMAGES]);
        public AuctionName Name { get; private set; }
        public bool BuyNowOnly { get; private set; }
        public BuyNowPrice BuyNowPrice { get; private set; }
        public decimal ActualPrice { get; private set; }
        public AuctionDate StartDate { get; private set; }
        public AuctionDate EndDate { get; private set; }
        public UserId Owner { get; private set; }
        public Product Product { get; private set; }
        public UserId Buyer { get; private set; } = UserId.Empty;
        public CategoryId[] Categories { get; private set; }
        public bool Completed { get; private set; }
        public bool Canceled { get; private set; }
        public Tag[] Tags { get; private set; }
        public IReadOnlyList<AuctionImage?> AuctionImages => _auctionImages;

        public AuctionBidsId? AuctionBidsId { get; private set; }

        /// <summary>
        /// Locked auction shouldn't be visible in list. It can be only viewed in read-only mode by lock issuer.
        /// </summary>
        public bool Locked { get; private set; }
        internal UserId LockIssuer { get; private set; } = UserId.Empty;
        private Guid? TransactionId { get; set; }

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
            AddEvent(new AuctionCreated(AggregateId)
            {
                Tags = auctionArgs.Tags.Select(t => t.Value).ToArray(),
                AggVersion = Version,
                Name = auctionArgs.Name,
                BuyNowOnly = auctionArgs.BuyNowOnly,
                BuyNowPrice = auctionArgs.BuyNowPrice,
                Category = auctionArgs.Categories.Select(i => (int)i).ToArray(),
                EndDate = auctionArgs.EndDate,
                Owner = auctionArgs.Owner,
                ProductCondition = (int)auctionArgs.Product.Condition,
                ProductName = auctionArgs.Product.Name,
                ProductDescription = auctionArgs.Product.Description,
                StartDate = auctionArgs.StartDate,
                AuctionImagesSize1Id = auctionArgs.AuctionImages.Select(i => i?.Size1Id).ToArray(),
                AuctionImagesSize2Id = auctionArgs.AuctionImages.Select(i => i?.Size2Id).ToArray(),
                AuctionImagesSize3Id = auctionArgs.AuctionImages.Select(i => i?.Size3Id).ToArray(),
            });
            if (!BuyNowOnly)
            {
                Lock(UserId.New());
            }
        }

        private void Create(AuctionArgs auctionArgs, bool compareToNow)
        {
            ValidateDates(auctionArgs.StartDate, auctionArgs.EndDate, compareToNow);
            if (BuyNowOnly && BuyNowPrice.Value == 0)
            {
                throw new DomainException("Cannot create buyNowOnly auction with buyNowPrice 0");
            }
            BuyNowPrice = auctionArgs.BuyNowPrice;
            StartDate = auctionArgs.StartDate;
            EndDate = auctionArgs.EndDate;
            Owner = auctionArgs.Owner;
            Product = auctionArgs.Product;
            Categories = auctionArgs.Categories;
            BuyNowOnly = auctionArgs.BuyNowOnly;
            Name = auctionArgs.Name;
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
                var i = 0;
                foreach (var img in auctionArgs.AuctionImages)
                {
                    if (img is not null)
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

        public async Task Buy(UserId buyerId, string paymentMethod, IAuctionPaymentVerification paymentVerification)
        {
            if (!BuyNowOnly && BuyNowPrice is null)
            {
                throw new DomainException("Cannot buy auction that is not of buy now type");
            }
            if (buyerId == Owner)
            {
                throw new DomainException("Cannot be bought by owner");
            }

            var paymentPassedVerification = await VerifyPayment(buyerId, paymentMethod, paymentVerification);
            if (!paymentPassedVerification)
            {
                throw new DomainException($"Payment started by buyer {buyerId} didn't pass verification");
            }

            StartBuyNowTransaction(buyerId, paymentMethod);
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

        private void StartBuyNowTransaction(UserId lockIssuerId, string paymentMethod)
        {
            Lock(lockIssuerId);
            TransactionId = Guid.NewGuid();

            AddEvent(new Events.V1.BuyNowTXStarted { TransactionId = TransactionId.Value, Price = BuyNowPrice.Value, AuctionId = AggregateId, BuyerId = lockIssuerId, PaymentMethod = paymentMethod, });
        }

        /// <summary>
        /// Confirms buy as a result of buy transaction commit
        /// </summary>
        /// <param name="transactionId"></param>
        /// <exception cref="DomainException"></exception>
        internal bool ConfirmBuy(Guid transactionId)
        {
            //transaction id was supplied in transaction event as a result of distributed transaction. Owner of
            //transaction id (subscriber of event) should supply valid transaction id and be able to commit it.
            //In other case failed event should be emitted.
            if (TransactionId != transactionId)
            {
                //transaction failed - auction unlocked and locked by another transaction
                AddEvent(new Events.V1.BuyNowTXFailed { TransactionId = transactionId, AuctionId = AggregateId });
                return false;
            }

            Buyer = LockIssuer;
            Unlock(LockIssuer);
            EndDate = DateTime.UtcNow;
            TransactionId = null;
            Completed = true;
            AddEvent(new Events.V1.BuyNowTXSuccess { TransactionId = transactionId, AuctionId = AggregateId, BuyerId = Buyer });
            return true;
        }

        internal void Lock(UserId lockIssuerId)
        {
            if (Locked)
            {
                return;
                throw new DomainException("Auction is already locked");
            }
            ApplyEvent(AddEvent(new AuctionLocked() { AuctionId = AggregateId, LockIssuer = lockIssuerId }));
        }


        internal void Unlock(UserId lockIssuerId)
        {
            if (!Locked) throw new DomainException("Auction is not locked");
            if (LockIssuer != lockIssuerId) throw new DomainException($"Invalid {nameof(lockIssuerId)}");
            ApplyEvent(AddEvent(new AuctionUnlocked() { AuctionId = AggregateId }));
        }

        internal void AddAuctionBids(AuctionBidsId auctionBidsId)
        {
            if (BuyNowOnly) throw new DomainException($"Cannot add AuctionBidsId to auction when {nameof(BuyNowOnly)}=true");
            if (Locked) Unlock(LockIssuer);
            ApplyEvent(AddEvent(new AuctionBidsAdded() { AuctionId = AggregateId, AuctionBidsId = auctionBidsId.Value }));
        }


        public void AddImage(AuctionImage img)
        {
            if (!_auctionImages.Contains(null))
            {
                throw new DomainException("Cannot add more auction images");
            }

            var ind = _auctionImages.IndexOf(null);
            _auctionImages[ind] = img;
            AddEvent(new AuctionImageAdded(ind, AggregateId, Owner, img.Size1Id, img.Size2Id, img.Size3Id));
        }

        public AuctionImage? ReplaceImage(AuctionImage img, int imgNum)
        {
            if (imgNum > _auctionImages.Capacity - 1)
            {
                throw new DomainException($"Cannot replace {imgNum} image");
            }

            var replaced = _auctionImages[imgNum];
            _auctionImages[imgNum] = img;
            AddEvent(new AuctionImageReplaced(AggregateId, imgNum, Owner, img.Size1Id, img.Size2Id, img.Size3Id));
            return replaced;
        }

        public AuctionImage? RemoveImage(int imgNum)
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

    }
}