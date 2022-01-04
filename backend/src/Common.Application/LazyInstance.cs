using Microsoft.Extensions.DependencyInjection;

namespace Common.Application
{
    public class LazyInstance<T> : Lazy<T> where T : class
    {
        public LazyInstance(IServiceProvider serviceProvider) : base(() => serviceProvider.GetRequiredService<T>()) { }
    }
}
