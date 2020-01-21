using System;
using System.Runtime.CompilerServices;
using Core.Common.Command;

[assembly: InternalsVisibleTo("Core.Command")]
namespace Core.Common.Attributes
{
    internal interface ICommandAttribute
    {
        Action<IImplProvider, CommandBase> PreHandleAttributeStrategy { get; }
        Action<IImplProvider, CommandBase> PostHandleAttributeStrategy { get; }
        int Order { get; }
    }
}