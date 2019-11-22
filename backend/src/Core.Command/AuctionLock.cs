using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Core.Command
{
    public static class AuctionLock
    {
        public static volatile ConcurrentDictionary<Guid, Tuple<int, object>> _auctionLockObjs =
            new ConcurrentDictionary<Guid, Tuple<int, object>>();

        public static void Lock(Guid auctionId)
        {
            Tuple<int, object> lockTupl;
            lock (_auctionLockObjs)
            {
                lockTupl = _auctionLockObjs.GetOrAdd(auctionId, new Tuple<int, object>(0, new object()));
                _auctionLockObjs[auctionId] = new Tuple<int, object>(lockTupl.Item1 + 1, lockTupl.Item2);
            }

            bool lockTaken = false;
            Monitor.TryEnter(lockTupl.Item2, TimeSpan.FromSeconds(60), ref lockTaken);
            if (!lockTaken)
            {
                throw new Exception();
            }
        }

        public static void ReleaseLock(Guid auctionId)
        {
            lock (_auctionLockObjs)
            {
                if (!_auctionLockObjs.TryGetValue(auctionId, out var lockTupl))
                {
                    throw new Exception();
                }

                if (lockTupl.Item1 - 1 == 0)
                {
                    _auctionLockObjs.TryRemove(auctionId, out var _);
                }
                else
                {
                    _auctionLockObjs[auctionId] = new Tuple<int, object>(lockTupl.Item1 - 1, lockTupl.Item2);
                }

                Monitor.Exit(lockTupl.Item2);
            }
        }
    }
}