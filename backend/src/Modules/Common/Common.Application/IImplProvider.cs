using Microsoft.Extensions.DependencyInjection;
using System;

namespace Common.Application
{
    public interface IImplProvider //TODO REMOVE
    {
        T Get<T>() where T : class;
        object Get(Type t);
    }

    internal class DefaultDIImplProvider : IImplProvider //TODO REMOVE
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
