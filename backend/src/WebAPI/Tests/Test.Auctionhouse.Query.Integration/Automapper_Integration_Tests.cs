using Auctionhouse.Query;
using AutoMapper;
using Xunit;

namespace Test.Auctionhouse.Query
{
    [Trait("Category", "Unit")]
    public class AutomapperTests
    {
        [Fact]
        public void Test1()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<QueryMapperProfile>();
                //cfg.AddProfile<AuctionReadProfile>();
            });

            configuration.AssertConfigurationIsValid();
        }
    }
}