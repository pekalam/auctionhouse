using Microsoft.Extensions.Hosting;
using XmlCategoryTreeStore;

namespace Adapter.XmlCategoryTreeStore
{
    internal class XmlCategoryTreeInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public XmlCategoryTreeInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            XmlCategoryTreeStoreInstaller.Init(_serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
