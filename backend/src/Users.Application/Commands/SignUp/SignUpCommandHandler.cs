using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;
using Users.Application.Exceptions;
using Users.Domain;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Commands.SignUp
{
    public class SignUpCommandHandler : CommandHandlerBase<SignUpCommand>
    {
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(IUserAuthenticationDataRepository userAuthenticationDataRepository, 
            IUserRepository userRepository, ILogger<SignUpCommandHandler> logger,
            Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) : base(ReadModelNotificationsMode.Disabled, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        protected override async Task<RequestStatus> HandleCommand(
            AppCommand<SignUpCommand> request, Lazy<EventBusFacade> eventBus,
            CancellationToken cancellationToken)
        {
            var existing = _userAuthenticationDataRepository.FindUserAuth(request.Command.Username);
            if (existing != null)
            {
                throw new UsernameConflictException($"User {request.Command.Username} already exists");
            }

            var username = await Username.Create(request.Command.Username);
            var user = User.Create(username);

            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            var userAuth = new UserAuthenticationData()
            {
                Password = request.Command.Password,
                UserId = user.AggregateId,
                UserName = user.Username,
                Email = request.Command.Email
            };
            _userAuthenticationDataRepository.AddUserAuth(userAuth);
            _userRepository.AddUser(user);
            eventBus.Value.Publish(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
            user.MarkPendingEventsAsHandled();

            return response;
        }
    }
}