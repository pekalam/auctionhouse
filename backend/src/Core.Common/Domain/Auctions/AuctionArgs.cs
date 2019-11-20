using System;
using System.Diagnostics;
using System.Linq;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
{
    public class AuctionArgs
    {
        public BuyNowPrice BuyNowPrice { get; set; }
        public bool BuyNowOnly { get; set; }
        public AuctionImage[] AuctionImages { get; set; }
        public AuctionDate StartDate { get; set; }
        public AuctionDate EndDate { get; set; }
        public UserIdentity Creator { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }
        public Tag[] Tags { get; set; }
        public AuctionName Name { get; set; }

        public class Builder
        {
            private AuctionArgs args = new AuctionArgs();

            private void CheckCanBuild()
            {
                if (args.Product == null ||
                    args.StartDate == default ||
                    args.EndDate == default ||
                    args.Creator == null ||
                    args.Category == null ||
                    args.Tags == null ||
                    args.Name == null
                )
                {
                    throw new DomainException("Invalid auctionArgs");
                }
            }

            public Builder From(AuctionArgs args)
            {
                this.args = args;
                return this;
            }

            public Builder SetTags(string[] tags)
            {
                this.args.Tags = tags.Select(s => (Tag)s).ToArray();
                return this;
            }
            public Builder SetTags(Tag[] tags)
            {
                this.args.Tags = tags;
                return this;
            }
            public Builder SetBuyNow(BuyNowPrice buyNowPrice)
            {
                args.BuyNowPrice = buyNowPrice;
                return this;
            }

            public Builder SetBuyNowOnly(bool buyNowOnly)
            {
                args.BuyNowOnly = buyNowOnly;
                return this;
            }

            public Builder SetStartDate(AuctionDate startDate)
            {
                args.StartDate = startDate;
                return this;
            }

            public Builder SetEndDate(AuctionDate endDate)
            {
                args.EndDate = endDate;
                return this;
            }

            public Builder SetOwner(UserIdentity owner)
            {
                args.Creator = owner;
                return this;
            }

            public Builder SetProduct(Product product)
            {
                args.Product = product;
                return this;
            }

            public Builder SetCategory(Category category)
            {
                args.Category = category;
                return this;
            }

            public Builder SetImages(AuctionImage[] images)
            {
                args.AuctionImages = images;
                return this;
            }

            public Builder SetName(AuctionName name)
            {
                args.Name = name;
                return this;
            }

            public AuctionArgs Build()
            {
                CheckCanBuild();
                return args;
            }
        }
    }
}