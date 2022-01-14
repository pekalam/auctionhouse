using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Common.Application
{
    /// <summary>
    /// Should be implemented by adapter that provides access to events database and implements outbox store.
    /// It is meant to be used in command handler where aggregate is processed, stored and events are saved in outbox.
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Begin();
    }

    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }


    internal class DefaultUnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork Begin()
        {
            var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            return new DefaultUnitOfWork(scope);
        }
    }

    internal class DefaultUnitOfWork : IUnitOfWork
    {
        private readonly TransactionScope _scope;
        private bool _disposed;

        public DefaultUnitOfWork(TransactionScope scope)
        {
            _scope = scope;
        }

        public void Commit()
        {
            _scope.Complete();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _scope.Dispose();
            }
        }

        public void Rollback()
        {
            _scope.Dispose();
            _disposed = true;
        }
    }
}
