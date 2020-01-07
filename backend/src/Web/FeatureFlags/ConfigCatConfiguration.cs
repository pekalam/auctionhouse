using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigCat.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Web.FeatureFlags
{
    public class ConfigCatConfiguration : IConfigurationSection
    {
        private IConfigCatClient _catClient;
        private string _key;

        public ConfigCatConfiguration(IConfigCatClient catClient, string key)
        {
            _catClient = catClient;
            _key = key;
        }

        public IConfigurationSection GetSection(string key)
        {
            return new ConfigCatConfiguration(_catClient, key);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return Enumerable.Empty<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get => _catClient.GetValue(key, "true");
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Key { get => _key; }
        public string Path { get; }

        public string Value
        {
            get
            {
                return _catClient.GetValue(_key, "true");
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
