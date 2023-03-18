using Chronicle;
using Chronicle.Integrations.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChronicleEfCoreStorage
{
    public static class ChronicleEfCoreStorageInstaller
    {
        public static void AddChronicleSQLServerStorage(this IServiceCollection services, string connectionString)
        {
            services.AddChronicle(builder =>
            {
                builder.UseEfCorePersistence<SagaLogDataSerialization, SagaDataSerialization>(services, SagaTypeSerialization.GetSagaType, opt =>
                {
                    opt.UseSqlServer(connectionString);
                });
            });
        }
    }
}
