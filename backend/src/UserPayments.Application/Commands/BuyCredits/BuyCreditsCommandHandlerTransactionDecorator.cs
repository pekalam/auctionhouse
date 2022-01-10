using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Core.Command.Commands.BuyCredits
{
    public class BuyCreditsCommandHandlerTransactionDecorator : CommandHandlerBase<BuyCreditsCommand>
    {
        private readonly BuyCreditsCommandHandler _buyCreditsCommandHandler;

        public BuyCreditsCommandHandlerTransactionDecorator(
            ILogger<BuyCreditsCommandHandlerTransactionDecorator> logger,
            BuyCreditsCommandHandler buyCreditsCommandHandler, Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) 
            : base(ReadModelNotificationsMode.Disabled, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _buyCreditsCommandHandler = buyCreditsCommandHandler;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<BuyCreditsCommand> request,
            Lazy<EventBusFacade> eventBus,
            CancellationToken cancellationToken)
        {
            var transactionOpt = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(10)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOpt))
            {
                var status = _buyCreditsCommandHandler.Handle(request, cancellationToken);
                scope.Complete();

                return status;
            }
        }
    }
}