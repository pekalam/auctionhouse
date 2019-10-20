using Core.Common.EventBus;
using Core.Common.Interfaces;
using MediatR;

namespace Core.Command.SignUp
{
    public class SignUpCommand : ICommand, IRequest
    {
        public string UserName { get; }
        public string Password { get; }
        public CorrelationId CorrelationId { get; }

        public SignUpCommand(string userName, string password, CorrelationId correlationId)
        {
            UserName = userName;
            Password = password;
            CorrelationId = correlationId;
        }
    }
}
