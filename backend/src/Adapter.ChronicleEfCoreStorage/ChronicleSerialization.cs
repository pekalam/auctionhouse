using Chronicle.Integrations.SQLServer;
using Newtonsoft.Json;

namespace ChronicleEfCoreStorage
{
    internal class SagaLogDataSerialization : ISagaLogDataSerialization
    {
        public object DeserializeMessage(string messageType, string serializedMessage)
        {
            return JsonConvert.DeserializeObject(serializedMessage, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            })!;
        }

        public string SerializeMessage(object message)
        {
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }

    internal class SagaDataSerialization : ISagaDataSerialization
    {
        public object DeserializeSagaData(string serializedData, string sagaType)
        {
            return JsonConvert.DeserializeObject(serializedData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            })!;
        }

        public string SerializeSagaData(object data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}
