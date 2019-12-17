using System;
using System.Runtime.CompilerServices;
using Core.Common.Command;

[assembly: InternalsVisibleTo("Core.Command")]
namespace Core.Common.Attributes
{
    internal interface ICommandAttribute
    {
        Action<IImplProvider, ICommand> AttributeStrategy { get; }
        int Order { get; }
    }
}