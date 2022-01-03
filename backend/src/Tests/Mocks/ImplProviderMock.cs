using Microsoft.Extensions.DependencyInjection;
using System;
using Common.Application;

namespace FunctionalTests.Mocks
{
    internal class ImplProviderMock : IImplProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ImplProviderMock(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        public object Get(Type t)
        {
            return _serviceProvider.GetService(t);
        }
    }
}