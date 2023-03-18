using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using EasyNetQ.Management.Client;
using System;
using TestConfigurationAccessor;

namespace Test.Adapter.RabbitMq.EventBus
{
    internal static class RabbitMqUtils
    {
        public static void PurgeQueues()
        {
            var settings = TestConfig.Instance.GetRabbitMqSettings();
            var hostStart = "host=";
            var hostInd = settings.ConnectionString.IndexOf(hostStart);
            var hostEnd = settings.ConnectionString.IndexOf(';', hostInd);
            var host = settings.ConnectionString[(hostInd + hostStart.Length)..hostEnd];

            using var managmentClient = new ManagementClient(host, "guest", "guest", 15672);

            var queues = managmentClient.GetQueues();

            foreach (var queue in queues)
            {
                Console.WriteLine("Purging = {0}", queue.Name);
                managmentClient.Purge(queue);
            }
        }
    }
}
