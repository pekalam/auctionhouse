using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;
using Core.Common.Query;
using Core.Query.Mediator;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests.CommandMediator_Tests
{
    [AuthorizationRequired]
    public class TestQueryAuth : IQuery<int>
    {
        public int AnotherProp { get; }
        [SignedInUser]
        public UserId SignedInUser { get; set; }

        public TestQueryAuth(int anotherProp)
        {
            AnotherProp = anotherProp;
        }
    }

    public class TestQuery : IQuery<int>
    {

    }

    [TestFixture]
    public class QueryMediator_Tests
    {
        [SetUp]
        public void SetUp()
        {
            QueryMediator.LoadQueryAttributeStrategies("Test.UnitTests");
        }

        [Test]
        public async Task Send_when_valid_query_passed_returns_response()
        {
            int expectedResponse = 5;
            var mockMediatr = new Mock<IMediator>();
            mockMediatr
                .Setup(f => f.Send(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            var queryMediator = new QueryMediator(mockMediatr.Object, Mock.Of<IImplProvider>(), Mock.Of<ILogger<QueryMediator>>());

            var response = await queryMediator.Send(new TestQuery());

            response.Should().Be(5);
        }

        [Test]
        public void Send_when_query_with_authorization_required_attribute_and_not_signed_in_throws()
        {
            var mockMediatr = new Mock<IMediator>();

            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(UserId.Empty);
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var queryMediator = new QueryMediator(mockMediatr.Object, stubImplProvider.Object, Mock.Of<ILogger<QueryMediator>>());

            Assert.ThrowsAsync<NotSignedInException>(async () => await queryMediator.Send(new TestQueryAuth(5)));
        }

        private bool VerifyTestQuery(IRequest<int> req, UserId userId)
        {
            var cmd = (TestQueryAuth)req;
            cmd.AnotherProp.Should().Be(100);
            cmd.SignedInUser.Should().Be(userId);
            return true;
        }


        [Test]
        public async Task Send_when_query_with_authorization_required_attribute_and_signed_in_sets_user_prop()
        {
            var userId = UserId.New();
            int expectedResponse = 5;
            var mockMediatr = new Mock<IMediator>();
            mockMediatr.Setup(f => f.Send(It.Is<IRequest<int>>(
                        request => VerifyTestQuery(request, userId)
                    ),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(userId);
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var queryMediator = new QueryMediator(mockMediatr.Object, stubImplProvider.Object, Mock.Of<ILogger<QueryMediator>>());

            var response = await queryMediator.Send(new TestQueryAuth(100));

            response.Should().Be(expectedResponse);
        }
    }
}