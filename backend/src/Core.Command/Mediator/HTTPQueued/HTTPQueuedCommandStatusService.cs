using System;
using System.Collections.Generic;
using System.Text;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Command.Mediator
{
    public class HTTPQueuedCommandStatusService
    {
        private IHTTPQueuedCommandStatusStorage _commandStatusStorage;
        private IImplProvider _implProvider;

        public HTTPQueuedCommandStatusService(IHTTPQueuedCommandStatusStorage commandStatusStorage, IImplProvider implProvider)
        {
            _commandStatusStorage = commandStatusStorage;
            _implProvider = implProvider;
        }

        public RequestStatus GetCommandStatus(CommandId commandId)
        {
            var (requestStatus, command) = _commandStatusStorage.GetCommandStatus(commandId);

            if (requestStatus != null && requestStatus.Status == Status.COMPLETED)
            {
                CommandMediator.InvokePostCommandAttributeStrategies(_implProvider, command);
            }

            return requestStatus;
        }
    }
}
