using System.Runtime.CompilerServices;
using MediatR;

[assembly: InternalsVisibleTo("Core.Query")]
namespace Core.Common.Query
{
    public interface IQuery
    {
    }

    public interface IQuery<T> : IQuery, IRequest<T>
    {
    }
}
