using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Domain
{
    public abstract class AggregateRoot<T, TId, U> : AggregateRoot<T, TId> where T : AggregateRoot<T, TId, U>, new() where U : UpdateEventGroup
    {
        private U _updateEventGroup = null;

        protected void AddUpdateEvent(UpdateEvent @event)
        {
            if (_canAddNewEvents)
            {
                if (_updateEventGroup == null)
                {
                    _updateEventGroup = CreateUpdateEventGroup();
                    _updateEventGroup.AggVersion = ++Version;
                    _updateEventGroup.AggregateId = Guid.Parse(AggregateId.ToString()); //TODO
                    _pendingEvents.Add(_updateEventGroup);
                }
                _updateEventGroup.Add(@event);
            }
        }

        protected abstract U CreateUpdateEventGroup();
    }
}
