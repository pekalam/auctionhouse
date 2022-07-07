namespace Common.Application.Events
{
    public interface IOutboxItemStore
    {
        Task<OutboxItem> Save(OutboxItem item);
        Task<IEnumerable<OutboxItem>> SaveMany(IEnumerable<OutboxItem> items);
        Task Update(OutboxItem item);
        Task UpdateMany(IEnumerable<OutboxItem> items);
    }
}
