using System;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using NUnit.Framework;

namespace Core.DomainModelTests
{
    public class AuctionArgs_Tests
    {
        [Test]
        public void AuctionArgsBuilder_when_invalid_params_throws()
        {
            var builder = new AuctionArgs.Builder();
            Assert.Throws<DomainException>(() => builder.Build());
            builder.SetCategory(new Category("test", 0));
            Assert.Throws<DomainException>(() => builder.Build());
            builder.SetOwner(new UserIdentity());
            Assert.Throws<DomainException>(() => builder.Build());
            builder.SetProduct(new Product("test name", "desccription 1111", Condition.New));
            Assert.Throws<DomainException>(() => builder.Build());
            builder.SetStartDate(DateTime.UtcNow.AddMinutes(20));
            Assert.Throws<DomainException>(() => builder.Build());
            builder.SetEndDate(DateTime.UtcNow.AddDays(5));
            Assert.Throws<DomainException>(() => builder.Build());

            builder.SetTags(new [] {"t1", "t2"});
            Assert.Throws<DomainException>(() => builder.Build());
            builder.SetName("Example name");
            Assert.DoesNotThrow(() => builder.Build());
            builder.SetBuyNow(20.0m);
            Assert.DoesNotThrow(() => builder.Build());
            builder.SetBuyNowOnly(false);
            Assert.DoesNotThrow(() => builder.Build());


            builder.SetBuyNow(20.0m);
            Assert.DoesNotThrow(() => builder.Build());
        }
    }
}