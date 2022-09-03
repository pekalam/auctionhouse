using Common.Application.Commands;
using Common.Application.Events;
using ReadModelNotifications.Settings;

namespace ReadModelNotifications
{
    internal static class ExtraDataDictionaryExtensions
    {
        public static bool HasSagaInitiatorKey(this Dictionary<string, string> dictionary)
        {
            return dictionary.ContainsKey(ReadModelNotificationsSettings.SagaInitiator_KeyName);
        }

        public static ReadModelNotificationsMode? GetNotificationsMode(this Dictionary<string, string> dictionary)
        {
            return dictionary.ContainsKey(nameof(ReadModelNotificationsMode)) ? GetNotificationsModeFromString(dictionary[nameof(ReadModelNotificationsMode)]) : null;
        }

        public static void SetNotificationsMode(this Dictionary<string, string> dictionary, ReadModelNotificationsMode mode)
        {
            dictionary[nameof(ReadModelNotificationsMode)] = ((int)mode).ToString();
        }

        private static ReadModelNotificationsMode GetNotificationsModeFromString(string str)
        {
            return str == "0" ? 0 : (str == "1" ? (ReadModelNotificationsMode)1 : (ReadModelNotificationsMode)2);
        }
    }

    internal static class CommandContextExtensions
    {
        public static ReadModelNotificationsMode? GetNotificationsMode(this CommandContext ctx) => ctx.ExtraData.GetNotificationsMode();

        public static void SetNotificationsMode(this CommandContext ctx, ReadModelNotificationsMode mode) => ctx.ExtraData.SetNotificationsMode(mode);
    }
}
