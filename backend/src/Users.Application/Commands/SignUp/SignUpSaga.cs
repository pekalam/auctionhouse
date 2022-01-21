using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.Mediator;
using Common.Application.SagaNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain;
using Users.Application.Commands.SignUp.AssignUserPayments;
using Users.Domain.Events;
using Users.Domain.Repositories;
using Users.DomainEvents;

namespace Users.Application.Commands.SignUp
{
    public class SignUpSaga : Saga,
        ISagaStartAction<UserCreated>,
        ISagaAction<UserPaymentsCreated>
    {
        public const string CmdContextParamName = "CommandContext";

        private readonly Lazy<ImmediateCommandQueryMediator> _mediator;

        public SignUpSaga(Lazy<ImmediateCommandQueryMediator> mediator)
        {
            _mediator = mediator;
        }

        private CommandContext GetCommandContext(ISagaContext context)
        {
            if (!context.TryGetMetadata(CmdContextParamName, out var metadata))
            {
                throw new NullReferenceException();
            }

            return (CommandContext)metadata.Value;
        }

        public Task CompensateAsync(UserCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task CompensateAsync(UserPaymentsCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(UserCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UserPaymentsCreated message, ISagaContext context)
        {
            var cmd = new AssignUserPaymentsCommand
            {
                UserId = message.UserId,
                UserPaymentsId = message.UserPaymentsId,
            };
            await _mediator.Value.Send(cmd, GetCommandContext(context));

            await CompleteAsync();
        }
    }
}
