using System;
using EventStore.ClientAPI;

namespace Infrastructure.Adapters.Repositories.EventStore
{
    public class ESConnectionContext
    {
        private EventStoreConnectionSettings _connectionSettings;

        public ESConnectionContext(EventStoreConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Connect()
        {
            var uri = new UriBuilder("tcp", _connectionSettings.IPAddress, _connectionSettings.Port, "").Uri;
            var settings = ConnectionSettings.Create();
            Connection =
                EventStoreConnection.Create(uri);
            Connection.ConnectAsync().Wait();
        }

        public IEventStoreConnection Connection { get; private set; }
    }
}
