using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;
using Users.Domain;
using Users.Domain.Auth;
using Users.Domain.Exceptions;
using Users.Domain.Repositories;

namespace Core.Command.SignUp
{
    public class SignUpCommandHandler : CommandHandlerBase<SignUpCommand>
    {
        private readonly EventBusFacade _eventBus;
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(EventBusFacade eventBusFacade, IUserAuthenticationDataRepository userAuthenticationDataRepository, IUserRepository userRepository, ILogger<SignUpCommandHandler> logger) : base(logger)
        {
            _eventBus = eventBusFacade;
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<SignUpCommand> request, CancellationToken cancellationToken)
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
            _eventBus.Publish(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
            user.MarkPendingEventsAsHandled();

            return response;
        }
    }
}