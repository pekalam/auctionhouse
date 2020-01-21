using System.Threading.Tasks;
using Core.Common;
using Core.Common.Command;
using MediatR;

namespace Core.Command.Mediator
{
    public class MediatRCommandHandlerMediator : ICommandHandlerMediator
    {
        private IMediator _mediator;

        public MediatRCommandHandlerMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<RequestStatus> Send(CommandBase commandBase)
        {
            return _mediator.Send(commandBase);
        }
    }
}