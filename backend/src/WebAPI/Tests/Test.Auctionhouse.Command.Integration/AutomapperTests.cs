using Auctionhouse.Command;
using AutoMapper;
using Xunit;

namespace Test.Auctionhouse.Command
{
    [Trait("Category", "Unit")]
    public class AutomapperTests
    {
        [Fact]
        public void Test1()
        {
            var configuration = new MapperConfiguration(cfg =>
            cfg.AddProfile<CommandMapperProfile>());

            configuration.AssertConfigurationIsValid();
        }
    }
}