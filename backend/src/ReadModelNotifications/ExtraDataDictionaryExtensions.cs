using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModelNotifications
{
    internal static class ExtraDataDictionaryExtensions
    {
        public static bool HasSagaInitiatorKey(this Dictionary<string, string> dictionary)
        {
            return dictionary.ContainsKey(ReadModelNotificationsSettings.SagaInitiator_KeyName);
        }
    }
}
