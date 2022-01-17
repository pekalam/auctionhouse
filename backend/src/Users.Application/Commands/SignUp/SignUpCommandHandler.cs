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
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public SignUpCommandHandler(IUserAuthenticationDataRepository userAuthenticationDataRepository,
            IUserRepository userRepository, ILogger<SignUpCommandHandler> logger, CommandHandlerBaseDependencies dependencies, IUnitOfWorkFactory unitOfWorkFactory)
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _userRepository = userRepository;
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(
            AppCommand<SignUpCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var existing = _userAuthenticationDataRepository.FindUserAuth(request.Command.Username);
            if (existing != null)
            {
                throw new UsernameConflictException($"User {request.Command.Username} already exists");
            }

            var username = await Username.Create(request.Command.Username);
            var user = User.Create(username, 1000);

            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            var userAuth = new UserAuthenticationData()
            {
                Password = request.Command.Password,
                UserId = user.AggregateId,
                UserName = user.Username,
                Email = request.Command.Email
            };

            using(var uow = _unitOfWorkFactory.Begin())
            {
                _userAuthenticationDataRepository.AddUserAuth(userAuth);
                _userRepository.AddUser(user);
                await eventOutbox.SaveEvents(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                user.MarkPendingEventsAsHandled();
                
                uow.Commit();
            }


            return response;
        }
    }
}