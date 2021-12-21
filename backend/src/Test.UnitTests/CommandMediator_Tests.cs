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
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;

namespace UnitTests.CommandMediator_Tests
{
    public class TestCommandBase : CommandBase
    {
    }

    [AuthorizationRequired]
    public class TestCommandBaseAuth : CommandBase
    {

    }

    [AuthorizationRequired]
    public class TestCommandBaseAuthWithSignedUser : CommandBase
    {
        public int AnotherProp { get; }
        [SignedInUser]
        public UserId SignedInUser { get; set; }

        public TestCommandBaseAuthWithSignedUser(int anotherProp)
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
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers("Test.UnitTests");
            CommandMediator.LoadCommandAttributeStrategies("Test.UnitTests");
        }

        [Test]
        public async Task Send_when_valid_command_passed_returns_response()
        {
            var expectedResponse = new RequestStatus(Status.COMPLETED);
            var mockMediatr = new Mock<IMediator>();
            mockMediatr
                .Setup(f => f.Send(It.IsAny<TestCommandBase>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);
            var mediatrHandlerMediator = new MediatRCommandHandlerMediator(mockMediatr.Object);

            var mediator = new CommandMediator(mediatrHandlerMediator, Mock.Of<IImplProvider>());

            var response = await mediator.Send(new TestCommandBase());

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
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(UserId.Empty);
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var mediatrHandlerMediator = new MediatRCommandHandlerMediator(mockMediatr.Object);
            var mediator = new CommandMediator(mediatrHandlerMediator, stubImplProvider.Object);


            Assert.ThrowsAsync<NotSignedInException>(async () => await mediator.Send(new TestCommandBaseAuth()));
        }

        public bool VerifyTestCommand(IRequest<RequestStatus> req, UserId userId)
        {
            var cmd = (TestCommandBaseAuthWithSignedUser) req;
            cmd.SignedInUser.Should().Be(userId);
            return true;
        }

        [Test]
        public void Send_when_command_with_authorization_required_attribute_and_signed_in_sets_user_prop()
        {
            var userId = UserId.New();
            var expectedResponse = new RequestStatus(Status.COMPLETED);
            var mockMediatr = new Mock<IMediator>();
            mockMediatr.Setup(f => f.Send(It.Is<IRequest<RequestStatus>>(
                    request => VerifyTestCommand(request, userId)
                ),
                It.IsAny<CancellationToken>()));


            var stubUserIdentityService = new Mock<IUserIdentityService>();
            stubUserIdentityService.Setup(f => f.GetSignedInUserIdentity())
                .Returns(userId);
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<IUserIdentityService>())
                .Returns(stubUserIdentityService.Object);

            var mediatrHandlerMediator = new MediatRCommandHandlerMediator(mockMediatr.Object);
            var mediator = new CommandMediator(mediatrHandlerMediator, stubImplProvider.Object);


            Assert.DoesNotThrowAsync(async () => await mediator.Send(new TestCommandBaseAuthWithSignedUser(12)));
        }
    }
}