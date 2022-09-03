using Adapter.SqlServer.EventOutbox;
using Common.Application.Commands;
using Xunit;

namespace Test.Adapter.SqlServer.EventOutbox
{
    internal static class CommandContextAssertions
    {
        public static void AssertCommandContextIsEqualTo(this CommandContext ctx, DbCommandContext dbCtx)
        {
            Assert.Equal(ctx.CommandId.Id, dbCtx.CommandId);
            Assert.Equal(ctx.CorrelationId.Value, dbCtx.CorrelationId);
            Assert.Equal(ctx.HttpQueued, dbCtx.HttpQueued);
            Assert.Equal(ctx.WSQueued, dbCtx.WSQueued);
            Assert.Equal(ctx.Name, dbCtx.Name);
            Assert.Equal(ctx.Name, dbCtx.Name);
        }
    }
}