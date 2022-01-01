using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Core.Command")]
namespace Common.Application.Commands
{
    public interface ICommandAttribute
    {
        Action<IImplProvider, ICommand> PreHandleAttributeStrategy { get; }
        Action<IImplProvider, ICommand> PostHandleAttributeStrategy { get; }
        int Order { get; }
    }
}