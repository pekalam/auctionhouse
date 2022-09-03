using Common.Application.Events;

namespace Common.Application.Commands
{
    public interface ICommandHandlerCallbacks
    {
        Task OnExecute<T>(AppCommand<T> appCommand) where T : ICommand;

        Task OnCompleted<T>(AppCommand<T> appCommand) where T : ICommand;

        Task OnEventsSent(IReadOnlyList<OutboxItem> items);

        /// <summary>
        /// Implemented by extension. Available keys should be provided in Common.Extensions.
        /// This should be used when extension cannot easily provide functionality by implementing only callback classes.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task CallExtension(string key, string value);

        /// <summary>
        /// Called manually by command handler until UOW will be separated from handler.
        /// TODO: decorate command handlers with optimistic concurrency + uow
        /// </summary>
        /// <returns></returns>
        Task OnUowCommit<T>(AppCommand<T> appCommand) where T : ICommand;

        Task OnUowCommit(CommandContext commandContext);
    }

    internal class DefaultCommandHandlerCallbacks : ICommandHandlerCallbacks
    {
        public Task CallExtension(string key, string value)
        {
            return Task.CompletedTask;
        }

        public Task OnCompleted<T>(AppCommand<T> appCommand) where T : ICommand
        {
            return Task.CompletedTask;
        }

        public Task OnEventsSent(IReadOnlyList<OutboxItem> items)
        {
            return Task.CompletedTask;
        }

        public Task OnExecute<T>(AppCommand<T> appCommand) where T : ICommand
        {
            return Task.CompletedTask;
        }

        public Task OnUowCommit<T>(AppCommand<T> appCommand) where T : ICommand
        {
            return Task.CompletedTask;
        }

        public Task OnUowCommit(CommandContext commandContext)
        {
            return Task.CompletedTask;
        }
    }
}
