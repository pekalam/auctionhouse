using Auctionhouse.Query;
using AutoMapper;
using Xunit;

namespace Test.Auctionhouse.Query.Integration
{
    [Trait("Category", "Integration")]
    public class Automapper_Integration_Tests
    {
        [Fact]
        public void Test1()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<QueryMapperProfile>();
                cfg.AddProfile<AuctionReadProfile>();
            });

            configuration.AssertConfigurationIsValid();
        }
    }
}