using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Command
{
    public class AtomicSequence<T>
    {
        private List<Tuple<Action<T, AtomicSequence<T>>, Action<T, AtomicSequence<T>>>> _tasks = new List<Tuple<Action<T, AtomicSequence<T>>, Action<T, AtomicSequence<T>>>>();
        public object TransactionContext { get; set; }

        public AtomicSequence<T> AddTask(Action<T, AtomicSequence<T>> execute, Action<T, AtomicSequence<T>> rollback)
        {
            _tasks.Add(new Tuple<Action<T, AtomicSequence<T>>, Action<T, AtomicSequence<T>>>(execute, rollback));
            return this;
        }

        public void ExecuteSequence(T param)
        {
            var exceptions = new List<Exception>();
            //commit
            int i = 0;
            while (i < _tasks.Count)
            {
                try
                {
                    _tasks[i].Item1?.Invoke(param, this);
                    i++;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    break;
                }
            }

            //rollback
            if (i != _tasks.Count)
            {
                i--;
                while (i >= 0)
                {
                    _tasks[i].Item2?.Invoke(param, this);
                    i--;
                }
            }

            if (exceptions.Count > 0)
            {
                throw exceptions.Aggregate<Exception, Exception>(null,
                    (exception, exception1) => new Exception("", exception1));
            }
        }

        public void Rollback(T param)
        {
            int i = _tasks.Count - 1;
            while (i >= 0)
            {
                _tasks[i].Item2.Invoke(param, this);
                i--;
            }
        }
    }
}
