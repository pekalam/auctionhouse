using Common.Application.Commands;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Common.Application.Tests
{
    public class CommandContext_Tests
    {
        [Fact]
        public void CreateNew_ValidParams_ConstructsObjectWithNewIds()
        {
            var (ctx1, ctx2) = (CommandContext.CreateNew("Test1"), CommandContext.CreateNew("Test2"));
            
            ctx1.CommandId.Should().NotBe(ctx2.CommandId);
            ctx1.CorrelationId.Should().NotBe(ctx2.CorrelationId);
        }

        [Fact]
        public void CreateNew_ValidParams_ConstructsObjectWithValidProperties()
        {
            var commandName = "Test1";
            var ctx = CommandContext.CreateNew(commandName);

            ctx.CommandId.ToString().Should().NotBeNullOrEmpty();
            ctx.CorrelationId.ToString().Should().NotBeNullOrEmpty();
            ctx.User.Should().BeNull();
            ctx.HttpQueued.Should().BeFalse();
            ctx.WSQueued.Should().BeFalse();
            ctx.Name.Should().Be(commandName);
        }
    }
}
