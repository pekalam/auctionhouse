namespace ReadModelNotifications.Settings
{
    public interface ICommandNotificationSettingsReader
    {
        CommandNotificationSettings[] Read();
    }
}
