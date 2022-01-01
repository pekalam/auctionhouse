using Auctions.Application.CommandAttributes;
using Auctions.Domain;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using FluentAssertions;
using Moq;
using System.Linq;
using Xunit;

namespace Test.Auctions.Application.InAuctionCreateSessionAttr
{
    [InAuctionCreateSession]
    public class TestCommandBase : ICommand
    {
        public AuctionCreateSession CreateSession { get; set; }
        public int Param { get; set; }
    }

    public class InAuctionCreateSessionAttribute_Tests
    {
        [Fact]
        public void Loads_commands_and_their_members_to_internal_map()
        {
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers("Test.Auctions.Application");

            var internalMap = InAuctionCreateSessionAttribute._auctionCreateSessionCommandProperties;

            internalMap.Count.Should().Be(1);
            internalMap.First().Key.Should().Be(typeof(TestCommandBase));
            internalMap.First().Value.PropertyType.Should().Be(typeof(AuctionCreateSession));
            internalMap.First().Value.Name.Should().Be("CreateSession");
        }

        [Fact]
        public void Should_set_create_session_parameter()
        {
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers("Test.Auctions.Application");
            var attr = new InAuctionCreateSessionAttribute();

            var testSession = AuctionCreateSession.CreateSession(UserId.New());
            var mockImplProvider = new Mock<IImplProvider>();
            var mockAuctionCreateSessionService = new Mock<IAuctionCreateSessionStore>();
            mockAuctionCreateSessionService.Setup(service => service.GetExistingSession())
                .Returns(testSession);
            mockImplProvider.Setup(provider => provider.Get<IAuctionCreateSessionStore>())
                .Returns(mockAuctionCreateSessionService.Object);

            var cmd = new TestCommandBase() { Param = 1 };
            attr.PreHandleAttributeStrategy.Invoke(mockImplProvider.Object, cmd);

            cmd.CreateSession.Should().BeEquivalentTo(testSession);
            cmd.Param.Should().Be(1);
        }
    }
}
