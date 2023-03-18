using Microsoft.Extensions.Options;
using RocksDbSharp;

namespace Adatper.RabbitMq.EventBus.ErrorEventOutbox
{
    public class RocksDbOptions
    {
        public string DatabasePath { get; set; } = "./errorEventOutboxDb";
    }

    public class GlobalRocksDb : IDisposable
    {
        private readonly RocksDbOptions _options;
        private readonly DbOptions _dbOptions = new DbOptions().SetCreateIfMissing(true);
        private Lazy<RocksDb> _db;

        public GlobalRocksDb(IOptions<RocksDbOptions> options)
        {
            _options = options.Value;
            _db = new(() => OpenDb(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public RocksDb Db => _db.Value;

        public void Dispose()
        {
            _db.Value.Dispose();
        }

        private RocksDb OpenDb()
        {
            var db = RocksDb.Open(_dbOptions, _options.DatabasePath, new ColumnFamilies());
            return db;
        }
    }

    internal class RocksDbErrorEventOutboxStorage : IErrorEventOutboxStore, IErrorEventOutboxUnprocessedItemsFinder
    {
        private static readonly byte[] KeyZeroBytes = 0L.ToBytes();
        private readonly GlobalRocksDb _db;

        public RocksDbErrorEventOutboxStorage(GlobalRocksDb db)
        {
            _db = db;
        }

        public void Delete(ErrorEventOutboxItem item)
        {
            _db.Db.Remove(item.Timestamp.ToBytes());
        }

        public ErrorEventOutboxItem[] FindUnprocessed(int max)
        {
            var iterator = _db.Db.NewIterator(_db.Db.GetDefaultColumnFamily());
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
            if (item.Timestamp == default)
            {
                item.Timestamp = ErrorEventOutboxItemTimestampFactory.CreateTimestamp();
            }
            var bytes = RocksDbSerializationUtils.Serialize(item);

            _db.Db.Put(item.Timestamp.ToBytes(), bytes);
        }

        public void Update(ErrorEventOutboxItem item) => Save(item);
    }
}
