using Common.Application.Mediator;
using Common.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Commands
{
    public partial class TestBase
    {
        public async Task<TResponse> SendQuery<T, TResponse>(T query) where T : IQuery<TResponse>
        {
            using var scope = ServiceProvider.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<CommandQueryMediator>().SendQuery(query);
            return result;
        }
    }
}
