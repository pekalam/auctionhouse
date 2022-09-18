using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;

namespace FunctionalTests.Mocks
{
    // copied from aspnetcore github repo
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _innerDictionary = new Dictionary<string, byte[]>();

        public IEnumerable<string> Keys { get { return _innerDictionary.Keys; } }

        public string Id => "TestId";

        public bool IsAvailable { get; } = true;

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public void Remove(string key)
        {
            _innerDictionary.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _innerDictionary[key] = value.ToArray();
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }
    }
}