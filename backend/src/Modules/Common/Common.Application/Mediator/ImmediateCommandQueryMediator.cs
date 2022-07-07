using Common.Application.Commands;
using Common.Application.Queries;
using MediatR;

namespace Common.Application.Mediator
{
    public class ImmediateCommandQueryMediator : CommandQueryMediator
    {
        private readonly IMediator _mediator;

        public ImmediateCommandQueryMediator(IImplProvider implProvider, IMediator mediator) : base(implProvider)
        {
            _mediator = mediator;
        }

        protected override Task<T> Send<T>(IQuery<T> query)
        {
            return _mediator.Send(query);
        }

        protected override async Task<(RequestStatus, bool)> SendAppCommand<T>(AppCommand<T> command)
        {
            var status = await _mediator.Send(command);
            return (status, true);
        }
    }
}