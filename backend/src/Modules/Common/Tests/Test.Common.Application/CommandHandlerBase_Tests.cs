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
    public class TestCommand : ICommand
    {
        [Required] public string Param1 { get; }

        [MinLength(5)] public string Param2 { get; }

        public TestCommand(string param1, string param2)
        {
            Param1 = param1;
            Param2 = param2;
        }
    }

    public class TestCommandHandler : CommandHandlerBase<TestCommand>
    {

        public TestCommandHandler(ILogger<CommandHandlerBase<TestCommand>> logger) : base(ReadModelNotificationsMode.Disabled, new CommandHandlerBaseDependencies
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

    public class CommandHandlerBase_Tests
    {
        [Fact]
        public async Task Handle_when_command_is_invalid_throws()
        {
            var stubCommand = new TestCommand(null, "11");

            var mockCommandHandler = new TestCommandHandler(Mock.Of<ILogger<CommandHandlerBase<TestCommand>>>());


            await Assert.ThrowsAsync<InvalidCommandException>(() =>
                mockCommandHandler.Handle(new AppCommand<TestCommand>()
                {
                    CommandContext = CommandContext.CreateNew("test"),
                    Command = stubCommand,
                }, CancellationToken.None));
        }
    }
}