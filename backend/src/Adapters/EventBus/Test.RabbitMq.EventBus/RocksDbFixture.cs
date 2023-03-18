using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using System;
using System.IO;

namespace Test.Adapter.RabbitMq.EventBus
{
    public class RocksDbFixture : IDisposable
    {
        public string TestDbPath { get; } = @$".{Path.DirectorySeparatorChar}{Guid.NewGuid()}";

        public RocksDbFixture()
        {
        }

        public void Dispose()
        {
            Directory.Delete(TestDbPath, true);
        }
    }
}
