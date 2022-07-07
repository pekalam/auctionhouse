using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Infrastructure")]
namespace Core.Common.ApplicationServices
{
    internal class DefaultCommandErrorHandler : ICommandRollbackHandler
    {
        private readonly ILogger<DefaultCommandErrorHandler> _logger;

        public DefaultCommandErrorHandler(ILogger<DefaultCommandErrorHandler> logger)
        {
            _logger = logger;
        }

        public void Rollback(IAppEvent<Event> commandEvent)
        {
            _logger.LogDebug("Default command rollback handler for commandEvent: {commandEvent}", commandEvent);
        }
    }

    public static class RollbackHandlerRegistry
    {
        private static Dictionary<string, Func<IImplProvider, ICommandRollbackHandler>> _rollbackHandlerMap = new Dictionary<string, Func<IImplProvider, ICommandRollbackHandler>>();
        static internal IImplProvider ImplProvider { get; set; }

        static RollbackHandlerRegistry()
        {
            _rollbackHandlerMap.Add("*", (provider => new DefaultCommandErrorHandler(provider.Get<ILogger<DefaultCommandErrorHandler>>())));
        }

        public static void RegisterCommandRollbackHandler(string commandName,
            Func<IImplProvider, ICommandRollbackHandler> rollbackHandlerFactory)
        {
            if (_rollbackHandlerMap.ContainsKey(commandName) == false)
            {
                _rollbackHandlerMap.Add(commandName, rollbackHandlerFactory);
            }
        }

        public static ICommandRollbackHandler GetCommandRollbackHandler(string commandName)
        {
            if (ImplProvider == null)
            {
                throw new Exception("Null implProvider");
            }
            if (_rollbackHandlerMap.TryGetValue(commandName, out var handler))
            {
                return handler.Invoke(ImplProvider);
            }
            else
            {
                return _rollbackHandlerMap["*"]
                    .Invoke(ImplProvider);
            }
        }
    }
}