using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Domain.Default
{
    public abstract class AggregateRoot<T, U> : AggregateRoot<T, Guid, U>
        where T : AggregateRoot<T, Guid, U>, new() where U : UpdateEventGroup
    {

    }

    public abstract class AggregateRoot<T> : Domain.AggregateRoot<T, Guid> where T : Domain.AggregateRoot<T, Guid>, new()
    {
    }
}
