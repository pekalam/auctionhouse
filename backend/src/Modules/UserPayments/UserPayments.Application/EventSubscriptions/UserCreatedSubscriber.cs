using Common.Application.Events;
using Common.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Application.Commands.CreateUserPayments;
using UserPayments.Domain.Repositories;
using Users.DomainEvents;

namespace UserPayments.Application.EventSubscriptions
{
    public class UserCreatedSubscriber : EventSubscriber<UserCreated>
    {
        private readonly CommandQueryMediator _mediator;

        public UserCreatedSubscriber(IAppEventBuilder eventBuilder, CommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<UserCreated> appEvent)
        {
            var cmd = new CreateUserPaymentsCommand
            {
                UserId = appEvent.Event.UserId,
            };
            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
