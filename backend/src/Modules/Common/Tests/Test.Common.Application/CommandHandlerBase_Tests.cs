using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.Application.Tests
{
    public class CommandHandlerBase_Tests
    {
        public class TestCommand : ICommand
        {
            [MinLength(5)] public string Param { get; }

            public TestCommand(string param)
            {
                Param = param;
            }
        }

        public class InvalidTestCommandHandler : CommandHandlerBase<TestCommand>
        {

            public InvalidTestCommandHandler(ILogger<CommandHandlerBase<TestCommand>> logger) : base(ReadModelNotificationsMode.Disabled, new CommandHandlerBaseDependencies
            {
                Logger = Mock.Of<ILogger<RequestStatus>>()
            })
            {
            }

            protected override Task<RequestStatus> HandleCommand(AppCommand<TestCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
            {
                Assert.False(true, "Should not be called");
                return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED));
            }
        }


        [Fact]
        public async Task Handle_InvalidCommand_ThrowsInvalidCommandException()
        {
            var stubCommand = new TestCommand("12");
            var mockCommandHandler = new InvalidTestCommandHandler(Mock.Of<ILogger<CommandHandlerBase<TestCommand>>>());

            var action = () =>
                mockCommandHandler.Handle(new AppCommand<TestCommand>()
                {
                    CommandContext = CommandContext.CreateNew("test"),
                    Command = stubCommand,
                }, CancellationToken.None);

            await Assert.ThrowsAsync<InvalidCommandException>(action);
        }
    }
}