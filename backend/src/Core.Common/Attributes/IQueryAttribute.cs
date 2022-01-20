using System;
using Core.Common.Query;

namespace Core.Common.Attributes
{
    internal interface IQueryAttribute
    {
        Action<IImplProvider, IQuery> AttributeStrategy { get; } //TODO custom delegate insteat of generic delegate
        int Order { get; }
    }
}