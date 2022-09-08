using Chronicle;
using Chronicle.Integrations.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChronicleEfCoreStorage
{
    public static class ChronicleEfCoreStorageInstaller
    {
        public static void AddChronicleSQLServerStorage(this IServiceCollection services, GetSagaType getSagaTypeDelegate, string connectionString)
        {
            services.AddChronicle(builder =>
            {
                builder.UseEfCorePersistence<SagaLogDataSerialization, SagaDataSerialization>(services, getSagaTypeDelegate, opt =>
                {
                    opt.UseSqlServer(connectionString);
                });
            });
        }


        public static void AddChronicleSQLServerStorage(this IChronicleBuilder builder, IServiceCollection services, GetSagaType getSagaTypeDelegate, string connectionString)
        {
            builder.UseEfCorePersistence<SagaLogDataSerialization, SagaDataSerialization>(services, getSagaTypeDelegate, opt =>
            {
                opt.UseSqlServer(connectionString);
            });
        }
    }
}
