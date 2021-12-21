using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Users;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace UnitTests.AuctionCreateSessionAttributeTests
{
    [InAuctionCreateSession]
    public class TestCommandBase : CommandBase
    {
        public AuctionCreateSession CreateSession { get; set; }
        public int Param { get; set; }
    }

    [TestFixture]
    public class InAuctionCreateSessionAttribute_Tests
    {
        [Test]
        public void LoadAuctionCreateSessionCommandMembers_loads_commands_and_their_members_to_internal_map()
        {
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers("Test.UnitTests");

            var internalMap = InAuctionCreateSessionAttribute._auctionCreateSessionCommandProperties;

            internalMap.Count.Should().Be(1);
            internalMap.First().Key.Should().Be(typeof(TestCommandBase));
            internalMap.First().Value.PropertyType.Should().Be(typeof(AuctionCreateSession));
            internalMap.First().Value.Name.Should().Be("CreateSession");
        }

        [Test]
        public void AttributeStrategy_should_set_create_session_parameter()
        {
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers("Test.UnitTests");
            var attr = new InAuctionCreateSessionAttribute();

            var testSession = new AuctionCreateSession(UserId.New());
            var mockImplProvider = new Mock<IImplProvider>();
            var mockAuctionCreateSessionService = new Mock<IAuctionCreateSessionService>();
            mockAuctionCreateSessionService.Setup(service => service.GetExistingSession())
                .Returns(testSession);
            mockImplProvider.Setup(provider => provider.Get<IAuctionCreateSessionService>())
                .Returns(mockAuctionCreateSessionService.Object);

            var cmd = new TestCommandBase(){Param = 1};
            attr.PreHandleAttributeStrategy.Invoke(mockImplProvider.Object, cmd);

            cmd.CreateSession.Should().BeEquivalentTo(testSession);
            cmd.Param.Should().Be(1);
        }
    }
}
