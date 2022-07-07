using System;
using System.Runtime.CompilerServices;
using Core.Common.Command;

[assembly: InternalsVisibleTo("Core.Command")]
namespace Core.Common.Attributes
{
    internal interface ICommandAttribute
    {
        Action<IImplProvider, ICommand> PreHandleAttributeStrategy { get; }
        Action<IImplProvider, ICommand> PostHandleAttributeStrategy { get; }
        int Order { get; }
    }
}