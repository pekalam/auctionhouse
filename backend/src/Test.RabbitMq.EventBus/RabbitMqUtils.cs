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
}
