using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common.Application.ServiceContracts
{
    internal record OutboxItemFinderContract_GetUnprocessedItemsOlder(OutboxItemFinder_GetUnprocessedItemsOlderThanArgs Given, IEnumerable<OutboxItem> Expected);
    internal record OutboxItemFinderContract_GetTotalUnprocessedItemsOlder(OutboxItemFinder_GetTotalUnprocessedItemsOlderThanArgs Given, Task<int> Expected);

    internal class OutboxItemFinderContracts
    {
        public static OutboxItemFinderContract_GetUnprocessedItemsOlder
            ValidGetUnprocessedItemsOlderThanArgs(IEnumerable<OutboxItem> saved, Timestamp diff, Timestamp currentTimestamp, int limit)
        {
            return new(new(diff.Value, currentTimestamp.Value, limit), 
                saved.Where(i => currentTimestamp.Value - i.Timestamp > diff.Value).Take(limit));
        }

        public static OutboxItemFinderContract_GetTotalUnprocessedItemsOlder
            ValidGetTotalUnprocessedItemsOlder(Timestamp diff, Timestamp currentTimestamp, int result)
        {
            return new(new(diff.Value, currentTimestamp.Value), Task.FromResult(result));
        }
    }

    internal record OutboxItemFinder_GetUnprocessedItemsOlderThanArgs(long diff, long currentTimestamp, int limit);
    internal record OutboxItemFinder_GetTotalUnprocessedItemsOlderThanArgs(long diff, long currentTimestamp);
}
