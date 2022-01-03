using Common.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application
{
    public static class InstallerExtensions
    {
        public static void AddEventSubscribers(this IServiceCollection services, params Type[] typesFromAssembliesToScan)
        {
            if (typesFromAssembliesToScan.Length == 0) throw new ArgumentException($"Empty {nameof(typesFromAssembliesToScan)}");

            services.Scan(s =>
            {
                var x = s.FromAssembliesOf(typesFromAssembliesToScan)
                .AddClasses(filter =>
                {
                    filter.AssignableTo(typeof(EventSubscriber<>));
                }).AsSelf().WithTransientLifetime();
            });
        }
    }
}
