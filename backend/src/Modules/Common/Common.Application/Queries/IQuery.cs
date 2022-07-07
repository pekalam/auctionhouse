using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Queries
{
    public interface IQuery
    {
    }

    public interface IQuery<T> : IQuery, IRequest<T>
    {
    }
}
