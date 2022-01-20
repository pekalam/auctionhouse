using RocksDbSharp;

namespace Adatper.RabbitMq.EventBus.ErrorEventOutbox
{
    public class RocksDbOptions
    {
        public string DatabasePath { get; set; } = "./errorEventOutboxDb";
    }

    internal class RocksDbErrorEventOutboxStorage : IErrorEventOutboxStore, IErrorEventOutboxUnprocessedItemsFinder
    {
        private static readonly DbOptions dbOptions = new DbOptions().SetCreateIfMissing(true);
        private static readonly byte[] KeyZeroBytes = 0L.ToBytes();

        public static RocksDbOptions Options { get; set; } = new();
        internal static readonly Lazy<RocksDb> db;

        static RocksDbErrorEventOutboxStorage()
        {
            db = new(() => OpenDb(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static RocksDb OpenDb()
        {
            var db = RocksDb.Open(dbOptions, Options.DatabasePath, new ColumnFamilies());
            return db;
        }


        public void Delete(ErrorEventOutboxItem item)
        {
            db.Value.Remove(item.Timestamp.ToBytes());
        }

        public ErrorEventOutboxItem[] FindUnprocessed(int max)
        {
            var iterator = db.Value.NewIterator(db.Value.GetDefaultColumnFamily());
            var items = new List<ErrorEventOutboxItem>(20);
            iterator.Seek(KeyZeroBytes);

            while (iterator.Valid())
            {
                items.Add(RocksDbSerializationUtils.Deserialize(iterator.Value()) ?? throw new NullReferenceException());
                iterator.Next();
            }
            return items.ToArray();
        }

        public void Save(ErrorEventOutboxItem item)
        {
            var bytes = RocksDbSerializationUtils.Serialize(item);

            if (item.Timestamp == default)
            {
                item.Timestamp = ErrorEventOutboxItemTimestampFactory.CreateTimestamp();
            }

            db.Value.Put(item.Timestamp.ToBytes(), bytes);
        }

        public void Update(ErrorEventOutboxItem item) => Save(item);
    }
}
