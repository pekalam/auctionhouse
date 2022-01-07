using Core.Common.Domain;
using Core.DomainFramework;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    internal class MsSqlESRepositoryBaseExceptionDecorator : MsSqlESRepositoryBase
    {
        private readonly MsSqlESRepositoryBase _base;

        public MsSqlESRepositoryBaseExceptionDecorator(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
            _base = new MsSqlESRepositoryBase(connectionSettings);
        }

        protected internal override void AddAggregate(IReadOnlyCollection<Event> pendingEvents, string aggregateId, long aggVersion, string aggregateName)
        {
            try
            {
                _base.AddAggregate(pendingEvents, aggregateId, aggVersion, aggregateName);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(AddAggregate)} thrown an exception when inserting events", e);
            }
        }

        protected internal override void RemoveAggregate(string aggregateId)
        {
            try
            {
                _base.RemoveAggregate(aggregateId);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(RemoveAggregate)} thrown an exception when droping events", e);
            }
        }

        protected internal override List<Event>? ReadEvents(Guid aggId)
        {
            try
            {
                return _base.ReadEvents(aggId);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(ReadEvents)} thrown an exception while querying db", e);
            }
        }

        protected internal override List<Event>? ReadEvents(Guid aggId, long version)
        {
            try
            {
                return _base.ReadEvents(aggId, version);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(ReadEvents)} thrown an exception while querying db", e);
            }
        }

        protected internal override void UpdateAggregate(IReadOnlyCollection<Event> pendingEvents, string aggregateId, long aggVersion, string aggregateName)
        {
            try
            {
                _base.UpdateAggregate(pendingEvents, aggregateId, aggVersion, aggregateName);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"{nameof(UpdateAggregate)} thrown an exception when inserting events", e);
            }
        }

    }
}