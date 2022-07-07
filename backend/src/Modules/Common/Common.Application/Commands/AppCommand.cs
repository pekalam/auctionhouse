using MediatR;

namespace Common.Application.Commands
{
    public interface ICommandContextOwner
    {
        CommandContext CommandContext { get; set; }
    }

    public class AppCommand<T> : IRequest<RequestStatus>, ICommandContextOwner where T : ICommand
    {
        public T Command { get; set; }
        public CommandContext CommandContext { get; set; }
    }
}
