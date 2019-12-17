using System.Runtime.CompilerServices;
using MediatR;

namespace Core.Common.Query
{
    public interface IQuery
    {
    }

    public interface IQuery<T> : IQuery, IRequest<T>
    {
    }
}
