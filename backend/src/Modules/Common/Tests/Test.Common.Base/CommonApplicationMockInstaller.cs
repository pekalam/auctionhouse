using Common.Application.Events;
using Common.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Base
{
    public class CommonApplicationMockInstaller : CommonApplicationInstaller
    {
        public CommonApplicationMockInstaller(IServiceCollection services) : base(services)
        {
            AddOutboxItemStore(_ => Mock.Of<IOutboxItemStore>());
        }
    }
}
