using ReadModelNotifications.Settings;

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
