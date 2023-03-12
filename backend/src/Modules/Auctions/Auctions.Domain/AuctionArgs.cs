using Auctions.Domain.Services;
using Core.DomainFramework;
using System.Runtime.CompilerServices;

namespace Auctions.Domain
{
    public class AuctionArgs
    {
        public BuyNowPrice? BuyNowPrice { get; set; }
        public bool BuyNowOnly { get; set; }
        public AuctionImages AuctionImages { get; set; } = new();
        public AuctionDate StartDate { get; set; } = null!;
        public AuctionDate EndDate { get; set; } = null!;
        public UserId Owner { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public CategoryId[] Categories { get; set; } = null!;
        public Tag[] Tags { get; set; } = null!;
        public AuctionName Name { get; set; } = null!;

        public class Builder
        {
            private AuctionArgs _args = new AuctionArgs();

            private static void ThrowIfNull(string memberName, object? obj)
            {
                if(obj is null)
                {
                    throw new DomainException($"{memberName} cannot be null");
                }
            }

            public Builder From(AuctionArgs args)
            {
                _args = args;
                ThrowIfNull(nameof(_args), _args);
                return this;
            }

            public Builder SetTags(string[] tags)
            {
                _args.Tags = tags.Select(s => (Tag)s).ToArray();
                ThrowIfNull(nameof(_args.Tags), _args.Tags);
                return this;
            }
            public Builder SetTags(Tag[] tags)
            {
                _args.Tags = tags;
                ThrowIfNull(nameof(_args.Tags), _args.Tags);
                return this;
            }
            public Builder SetBuyNow(BuyNowPrice? buyNowPrice)
            {
                _args.BuyNowPrice = buyNowPrice;
                return this;
            }

            public Builder SetBuyNowOnly(bool buyNowOnly)
            {
                _args.BuyNowOnly = buyNowOnly;
                return this;
            }

            public Builder SetStartDate(AuctionDate startDate)
            {
                _args.StartDate = startDate;
                ThrowIfNull(nameof(_args.StartDate), _args.StartDate);
                return this;
            }

            public Builder SetEndDate(AuctionDate endDate)
            {
                _args.EndDate = endDate;
                ThrowIfNull(nameof(_args.EndDate), _args.EndDate);
                return this;
            }

            public Builder SetOwner(UserId owner)
            {
                _args.Owner = owner;
                ThrowIfNull(nameof(_args.Owner), _args.Owner);
                return this;
            }

            public Builder SetProduct(Product product)
            {
                _args.Product = product;
                ThrowIfNull(nameof(_args.Product), _args.Product);
                return this;
            }

            public async Task<Builder> SetCategories(string[] categoryNames, ICategoryNamesToTreeIdsConversion convertCategoryNamesToIds)
            {
                _args.Categories = await convertCategoryNamesToIds.ConvertNames(categoryNames);
                ThrowIfNull(nameof(_args.Categories), _args.Categories);
                return this;
            }

            public Builder SetImages(AuctionImages images)
            {
                _args.AuctionImages = images;
                ThrowIfNull(nameof(_args.AuctionImages), _args.AuctionImages);
                return this;
            }

            public Builder SetName(AuctionName name)
            {
                _args.Name = name;
                ThrowIfNull(nameof(_args.Name), _args.Name);
                return this;
            }

            public AuctionArgs Build()
            {
                return _args;
            }
        }
    }
}