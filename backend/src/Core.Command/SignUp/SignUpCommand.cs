using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;
using MediatR;

namespace Core.Command.SignUp
{
    public class SignUpCommand : ICommand
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
