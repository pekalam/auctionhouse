using System;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using MediatR;

namespace Core.Common.Command
{
    public class CommandContext
    {
        public CorrelationId CorrelationId { get; set; }
        public Guid User { get; set; }
    }

    public class CommandBase : IRequest<RequestStatus>
    {
        public CommandContext CommandContext { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
    }
}
