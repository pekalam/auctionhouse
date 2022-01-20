using EasyNetQ;
using System.Text.Json.Serialization;

namespace Adatper.RabbitMq.EventBus.ErrorEventOutbox
{
    internal interface IErrorEventOutboxStore
    {
        void Save(ErrorEventOutboxItem item);
        void Update(ErrorEventOutboxItem item);
        void Delete(ErrorEventOutboxItem item);
    }

    internal interface IErrorEventOutboxUnprocessedItemsFinder
    {
        ErrorEventOutboxItem[] FindUnprocessed(int max);
    }

    internal class ErrorEventOutboxItem
    {
        public long Timestamp { get; set; }
        public MessageProperties MessageProperties { get; set; }
        public string MessageJson { get; set; }
        public string RoutingKey { get; set; }
    }

    [JsonSerializable(typeof(ErrorEventOutboxItem))]
    internal partial class ErrorEventOutboxJsonSerializerContext : JsonSerializerContext
    {
    }

    internal static class ErrorEventOutboxItemTimestampFactory
    {
        public static long CreateTimestamp() => DateTime.Now.ToFileTime();
    }

    internal static class RocksDbSerializationExtensions
    {
        public static byte[] ToBytes(this long value) => BitConverter.GetBytes(value);
    }

    internal static class RocksDbSerializationUtils
    {
        public static byte[] Serialize(ErrorEventOutboxItem item)
        {
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(item, typeof(ErrorEventOutboxItem),
                ErrorEventOutboxJsonSerializerContext.Default);
            return bytes;
        }

        public static ErrorEventOutboxItem? Deserialize(byte[] bytes)
        {
            return (ErrorEventOutboxItem?)
                System.Text.Json.JsonSerializer.Deserialize(bytes, typeof(ErrorEventOutboxItem), ErrorEventOutboxJsonSerializerContext.Default);
        }
    }
}
