using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application
{
    public static class CommonInstaller
    {
        public static void AddCommon(this IServiceCollection services)
        {
            services.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            services.AddTransient<ISagaNotifications, InMemorySagaNotifications>();
            services.AddTransient<IImplProvider, DefaultDIImplProvider>();
            services.AddTransient<EventBusFacade>();
        }
    }
}
