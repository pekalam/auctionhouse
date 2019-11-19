using System;
using Core.Common.Command;

namespace Core.Common.Attributes
{
    internal interface ICommandAttribute
    {
        Action<IImplProvider, ICommand> AttributeStrategy { get; }
        int Order { get; }
    }
}