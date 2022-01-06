using Common.Application.Commands;
using System;

namespace Common.Application.Queries
{
    internal interface IQueryAttribute
    {
        Action<IImplProvider, IQuery> AttributeStrategy { get; }
        int Order { get; }
    }
}