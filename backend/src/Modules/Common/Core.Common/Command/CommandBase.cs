using System;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using MediatR;

namespace Core.Common.Command
{
    /// <summary>
    /// It can be used to wait for command to complete it's work. Used instead of correlation id.
    /// </summary>
    public class CommandId
    {
        public string Id { get; }

        public CommandId(string id)
        {
            Id = id;
        }

        public static CommandId CreateNew() => new CommandId(Guid.NewGuid().ToString());

        public static CommandId From(string s) => new CommandId(s);
    }

    public class CommandContext
    {
        public CommandId CommandId { get; set; }
        public CorrelationId CorrelationId { get; set; }
        public Guid User { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
        public string Name { get; set; }

        public static CommandContext CreateHttpQueued(Guid user, string name) => new CommandContext { HttpQueued = true, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = EventBus.CorrelationId.CreateNew(), WSQueued = false };
        public static CommandContext CreateNew(Guid user, string name) => new CommandContext { HttpQueued = false, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = EventBus.CorrelationId.CreateNew(), WSQueued = false };
        public static CommandContext CreateWSQueued(Guid user, string name) => new CommandContext { HttpQueued = false, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = EventBus.CorrelationId.CreateNew(), WSQueued = true };
    }

    public interface ICommand : IRequest<RequestStatus>
    {

    }

    public interface ICommandContextOwner
    {
        CommandContext CommandContext { get; set; }
    }

    public class AppCommand<T> : IRequest<RequestStatus>, ICommandContextOwner where T : ICommand
    {
        public T Command { get; set; }
        public CommandContext CommandContext { get; set; }
    }
}
