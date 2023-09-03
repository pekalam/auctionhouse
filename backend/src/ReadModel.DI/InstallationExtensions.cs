using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core;
using Adapter.MongoDb;
using ReadModel.Contracts;

namespace ReadModel.DI;

public static class InstallationExtensions
{
    public static ReadModelInstaller AddReadModelModule(this IServiceCollection services, IConfiguration configuration)
    {
        var installer = new ReadModelInstaller(services);
        installer
            .AddMongoDbReadModel(configuration)
            .AddMongoDbImageReadRepositoryAdapter(configuration);

        return installer;
    }
}