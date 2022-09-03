using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.Mediator;
using Microsoft.Extensions.Logging;
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
        private readonly Lazy<IUserRepository> _users;
        private readonly Lazy<IUserAuthenticationDataRepository> _userAuthenticationData;
        private readonly Lazy<IUnitOfWorkFactory> _uowFactory;
        private readonly Lazy<ILogger<SignUpSaga>> _logger;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;

        public SignUpSaga(Lazy<ImmediateCommandQueryMediator> mediator, Lazy<IUserRepository> users, Lazy<IUserAuthenticationDataRepository> userAuthenticationData, Lazy<IUnitOfWorkFactory> uowFactory, 
            Lazy<ILogger<SignUpSaga>> logger, ICommandHandlerCallbacks commandHandlerCallbacks)
        {
            _mediator = mediator;
            _users = users;
            _userAuthenticationData = userAuthenticationData;
            _uowFactory = uowFactory;
            _logger = logger;
            _commandHandlerCallbacks = commandHandlerCallbacks;
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

        public async Task CompensateAsync(UserPaymentsCreated message, ISagaContext context)
        {
            using(var uow = _uowFactory.Value.Begin())
            {
                _users.Value.DeleteUser(new(message.UserId)); //delete should be idempotent
                _userAuthenticationData.Value.DeleteUserAuth(message.UserId); //delete should be idempotent
                await _commandHandlerCallbacks.OnUowCommit(GetCommandContext(context));
                uow.Commit();
            }
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
