using System.Threading.Tasks;

namespace Core.Common.Command
{
    public interface ICommandHandlerMediator
    {
        Task<RequestStatus> Send(CommandBase commandBase);
    }
}