using System;
using Core;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Adapters
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
