using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Core.Command.SignUp
{
    public class SignUpCommandHandler : CommandHandlerBase<SignUpCommand>
    {
        private readonly EventBusService _eventBusService;
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SignUpCommandHandler> _logger;

        public SignUpCommandHandler(EventBusService eventBusService, IUserAuthenticationDataRepository userAuthenticationDataRepository, IUserRepository userRepository, ILogger<SignUpCommandHandler> logger) : base(logger)
        {
            _eventBusService = eventBusService;
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
            _eventBusService.Publish(user.PendingEvents, request.CommandContext, Common.EventBus.ReadModelNotificationsMode.Disabled);
            user.MarkPendingEventsAsHandled();

            return response;
        }
    }
}