using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace UnitTests
{
    public class TestCommandBase : ICommand    {
        [Required] public string Param1 { get; }

        [MinLength(5)] public string Param2 { get; }

        public TestCommandBase(string param1, string param2)
        {
            Param1 = param1;
            Param2 = param2;
        }
    }

    public class TestCommandHandler : CommandHandlerBase<TestCommandBase>
    {

        public TestCommandHandler(ILogger<CommandHandlerBase<TestCommandBase>> logger) : base(logger)
        {
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<TestCommandBase> request, CancellationToken cancellationToken)
        {
            Assert.Fail("Should not be called");
            return Task.FromResult(new RequestStatus(Status.FAILED));
        }

      
    }

    [TestFixture]
    public class CommandHandlerBase_Tests
    {
        [Test]
        public void Handle_when_command_is_invalid_throws()
        {
            var stubCommand = new TestCommandBase(null, "11");

            var mockCommandHandler = new TestCommandHandler(Mock.Of<ILogger<CommandHandlerBase<TestCommandBase>>>());


            Assert.Throws<InvalidCommandException>(() =>
                mockCommandHandler.Handle(stubCommand, CancellationToken.None));
        }
    }
}