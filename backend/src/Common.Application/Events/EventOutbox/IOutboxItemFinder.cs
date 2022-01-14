namespace Common.Application.Events
{
    public interface IOutboxItemFinder
    {
        Task<IEnumerable<OutboxItem>> GetUnprocessedItemsOlderThan(long diff, long currentTimestamp, int limit);
        Task<int> GetTotalUnprocessedItemsOlderThan(long diff, long currentTimestamp);
    }
}
