using Common.Application.Commands;
using MediatR;

namespace Common.Application.Mediator
{
    public class ImmediateCommandMediator : CommandMediator
    {
        private readonly IMediator _mediator;

        public ImmediateCommandMediator(IImplProvider implProvider, IMediator mediator) : base(implProvider)
        {
            _mediator = mediator;
        }

        protected override async Task<(RequestStatus, bool)> SendAppCommand<T>(AppCommand<T> command)
        {
            var status = await _mediator.Send(command);
            return (status, true);
        }
    }
}