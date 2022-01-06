using Common.Application;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Application
{
    public static class ModuleInstaller
    {
        public static void AddUserPaymentsModule(this IServiceCollection services)
        {
            services.AddEventSubscribers(typeof(ModuleInstaller));
        }
    }
}
