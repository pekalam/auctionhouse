using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Core.Command.Handler;
using Core.Common;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.BuyCredits
{
    public class BuyCreditsCommandHandlerTransactionDecorator : CommandHandlerBase<BuyCreditsCommand>
    {
        private BuyCreditsCommandHandler _buyCreditsCommandHandler;

        public BuyCreditsCommandHandlerTransactionDecorator(
            ILogger<BuyCreditsCommandHandlerTransactionDecorator> logger,
            BuyCreditsCommandHandler buyCreditsCommandHandler) : base(logger)
        {
            _buyCreditsCommandHandler = buyCreditsCommandHandler;
        }

        protected override Task<RequestStatus> HandleCommand(BuyCreditsCommand request,
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