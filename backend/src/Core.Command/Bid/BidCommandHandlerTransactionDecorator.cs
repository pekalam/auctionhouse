using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Command.Bid;
using Core.Common;
using Core.Common.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.Bid
{
    public class BidCommandHandlerTransactionDecorator : CommandHandlerBase<BidCommand>
    {
        private BidCommandHandler _bidCommandHandler;

        public BidCommandHandlerTransactionDecorator(ILogger<BidCommandHandlerTransactionDecorator> logger, BidCommandHandler bidCommandHandler) : base(logger)
        {
            _bidCommandHandler = bidCommandHandler;
        }

        protected override Task<RequestStatus> HandleCommand(BidCommand request, CancellationToken cancellationToken)
        {
            var transactionOpt = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(10)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOpt))
            {
                var status = _bidCommandHandler.Handle(request, cancellationToken);
                scope.Complete();

                return status;
            }
        }
    }
}