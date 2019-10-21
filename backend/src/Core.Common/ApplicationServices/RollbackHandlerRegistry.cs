using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Common.Interfaces;

[assembly: InternalsVisibleTo("Infrastructure")]
namespace Core.Common.ApplicationServices
{
    public static class RollbackHandlerRegistry
    {
        private static Dictionary<string, Func<IImplProvider, ICommandRollbackHandler>> _rollbackHandlerMap = new Dictionary<string, Func<IImplProvider, ICommandRollbackHandler>>();
        static internal IImplProvider ImplProvider { get; set; }


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
                throw new Exception("handler not registered");
            }
        }
    }
}