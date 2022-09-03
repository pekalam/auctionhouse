namespace Common.Application.Events
{
    public interface IEventOutboxSavedItems
    {
        IReadOnlyList<OutboxItem> SavedOutboxStoreItems { get; }
    }
}
