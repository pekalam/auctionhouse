using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;
using Core.Common.Query;
using Core.Query;
using Core.Query.Mediator;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;

namespace UnitTests.CommandMediator_Tests
{
    public class TestCommand : ICommand
    {
    }

    [AuthorizationRequired]
    public class TestCommandAuth : ICommand
    {

    }

    [AuthorizationRequired]
    public class TestCommandAuthWithSignedUser : ICommand
    {
        public int AnotherProp { get; }
        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public TestCommandAuthWithSignedUser(int anotherProp)
        {
            AnotherProp = anotherProp;
        }
    }

    [TestFixture]
    public class CommandMediator_Tests
    {
        [SetUp]
        public void SetUp()
        {
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers("UnitTests");
            CommandMediator.LoadCommandAttributeStrategies("UnitTests");
        }

        [Test]
        public async Task Send_when_valid_command_passed_returns_response()
        {
            var expectedResponse = new RequestStatus(Status.COMPLETED);
            var mockMediatr = new Mock<IMediator>();
            mockMediatr
                .Setup(f => f.Send(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            var mediatrHandlerMediator = new MediatRCommandHandlerMediator(mockMediatr.Object);

            var mediator = new CommandMediator(mediatrHandlerMediator, Mock.Of<IImplProvider>());

            var response = await mediator.Send(new TestCommand());

            response.Status.Should()
                .Be(Status.COMPLETED);
            response.CorrelationId.Should().BeEquivalentTo(expectedResponse.CorrelationId);
        }

        [Test]
        public void Send_when_command_with_authorization_required_attribute_and_not_signed_in_throws()
        {
            var expectedResponse = new RequestStatus(Status.COMPLETED);
            var mockMediatr = new Mock<IMediator>();

            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(UserIdentity.Empty);
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var mediatrHandlerMediator = new MediatRCommandHandlerMediator(mockMediatr.Object);
            var mediator = new CommandMediator(mediatrHandlerMediator, stubImplProvider.Object);


            Assert.ThrowsAsync<NotSignedInException>(async () => await mediator.Send(new TestCommandAuth()));
        }

        public bool VerifyTestCommand(IRequest<RequestStatus> req)
        {
            var cmd = (TestCommandAuthWithSignedUser) req;
            cmd.SignedInUser.UserName.Should().Be("test");
            return true;
        }

        [Test]
        public void Send_when_command_with_authorization_required_attribute_and_signed_in_sets_user_prop()
        {
            var expectedResponse = new RequestStatus(Status.COMPLETED);
            var mockMediatr = new Mock<IMediator>();
            mockMediatr.Setup(f => f.Send(It.Is<IRequest<RequestStatus>>(
                    request => VerifyTestCommand(request)
                    ),
                It.IsAny<CancellationToken>()));
                

            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(new UserIdentity(Guid.NewGuid(), "test"));
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var mediatrHandlerMediator = new MediatRCommandHandlerMediator(mockMediatr.Object);
            var mediator = new CommandMediator(mediatrHandlerMediator, stubImplProvider.Object);


            Assert.DoesNotThrowAsync(async () => await mediator.Send(new TestCommandAuthWithSignedUser(12)));
        }
    }

    [AuthorizationRequired]
    public class TestQueryAuth : IQuery<int>
    {
        public int AnotherProp { get; }
        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

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
            QueryMediator.LoadQueryAttributeStrategies("UnitTests");
        }

        [Test]
        public async Task Send_when_valid_query_passed_returns_response()
        {
            int expectedResponse = 5;
            var mockMediatr = new Mock<IMediator>();
            mockMediatr
                .Setup(f => f.Send(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            var queryMediator = new QueryMediator(mockMediatr.Object, Mock.Of<IImplProvider>());

            var response = await queryMediator.Send(new TestQuery());

            response.Should().Be(5);
        }

        [Test]
        public void Send_when_query_with_authorization_required_attribute_and_not_signed_in_throws()
        {
            var mockMediatr = new Mock<IMediator>();

            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(UserIdentity.Empty);
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var queryMediator = new QueryMediator(mockMediatr.Object, stubImplProvider.Object);

            Assert.ThrowsAsync<NotSignedInException>(async () => await queryMediator.Send(new TestQueryAuth(5)));
        }

        private bool VerifyTestQuery(IRequest<int> req)
        {
            var cmd = (TestQueryAuth)req;
            cmd.AnotherProp.Should().Be(100);
            cmd.SignedInUser.UserName.Should().Be("test");
            return true;
        }


        [Test]
        public async Task Send_when_query_with_authorization_required_attribute_and_signed_in_sets_user_prop()
        {
            int expectedResponse = 5;
            var mockMediatr = new Mock<IMediator>();
            mockMediatr.Setup(f => f.Send(It.Is<IRequest<int>>(
                    request => VerifyTestQuery(request)
                ),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(new UserIdentity(Guid.NewGuid(), "test"));
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var queryMediator = new QueryMediator(mockMediatr.Object, stubImplProvider.Object);

            var response = await queryMediator.Send(new TestQueryAuth(100));

            response.Should().Be(expectedResponse);
        }
    }
}