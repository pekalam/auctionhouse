using System;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public class MicrosoftDIImplProvider : IImplProvider
    {
        private IServiceProvider _serviceProvider;

        public MicrosoftDIImplProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
