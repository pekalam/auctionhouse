using System;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public class DefaultDIImplProvider : IImplProvider
    {
        private IServiceProvider _serviceProvider;

        public DefaultDIImplProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>() where T : class
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<T>();
            }
        }
    }
}