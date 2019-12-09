using System;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public class DefaultDIImplProvider : IImplProvider
    {
        private IServiceScopeFactory _scopeFactory;

        public DefaultDIImplProvider(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public T Get<T>() where T : class
        {

            var scope = _scopeFactory.CreateScope();
                return scope.ServiceProvider.GetRequiredService<T>();
            
        }

        public object Get(Type t)
        {
                var scope = _scopeFactory.CreateScope();
                return scope.ServiceProvider.GetRequiredService(t);
        }
    }
}