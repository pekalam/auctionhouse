using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using EasyNetQ.Management.Client;
using System;

namespace Test.Adapter.RabbitMq.EventBus
{
    internal static class RabbitMqUtils
    {
        public static void PurgeQueues(string hostUrl = "localhost")
        {
            var managmentClient = new ManagementClient(hostUrl, "guest", "guest", 15672);

            var queues = managmentClient.GetQueues();

            foreach (var queue in queues)
            {
                Console.WriteLine("Purging = {0}", queue.Name);
                managmentClient.Purge(queue);
            }
        }
    }

    internal static class RockdbUtils
    {
        public static void ClearOutboxItems()
        {
            var storage = new RocksDbErrorEventOutboxStorage();

            var unprocessed = storage.FindUnprocessed(10);
            while (unprocessed.Length > 0)
            {
                foreach (var item in unprocessed)
                {
                    storage.Delete(item);
                }

                unprocessed = storage.FindUnprocessed(10);
            }
        }
    }
}
