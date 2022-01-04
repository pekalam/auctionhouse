using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Core.Command")]
namespace Common.Application.Commands
{
    public interface ICommandAttribute
    {
        Action<IImplProvider, CommandContext, ICommand> PreHandleAttributeStrategy { get; }
        Action<IImplProvider, CommandContext, ICommand> PostHandleAttributeStrategy { get; }
        int Order { get; }
    }
}