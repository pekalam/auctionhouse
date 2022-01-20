using Auctionhouse.Command;
using AutoMapper;
using Xunit;

namespace Test.Auctionhouse.Command.Integration
{
    [Trait("Category", "Integration")]
    public class Automapper_Integration_Tests
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