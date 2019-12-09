using System;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using MediatR;

namespace Core.Common.Command
{
    public class CommandContext
    {
        public CorrelationId CorrelationId { get; }
        public UserIdentity User { get; }

        public CommandContext(CorrelationId correlationId, UserIdentity user)
        {
            CorrelationId = correlationId;
            User = user;
        }
    }

    public class ICommand : IRequest<RequestStatus>
    {
        public CommandContext CommandContext { get; set; }
    }
}
