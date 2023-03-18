using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core;
using Adapter.MongoDb;

namespace ReadModel.DI;

public static class InstallationExtensions
{
    public static ReadModelInstaller AddReadModelModule(this IServiceCollection services, IConfiguration configuration)
    {
        var installer = new ReadModelInstaller(services, configuration);
        installer
            .AddMongoDbImageReadRepositoryAdapter(configuration);

        return installer;
    }
}