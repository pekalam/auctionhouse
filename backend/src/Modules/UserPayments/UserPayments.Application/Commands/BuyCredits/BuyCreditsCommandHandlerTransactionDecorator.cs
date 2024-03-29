﻿using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Core.Command.Commands.BuyCredits
{
    public class BuyCreditsCommandHandlerTransactionDecorator : CommandHandlerBase<BuyCreditsCommand>
    {
        private readonly BuyCreditsCommandHandler _buyCreditsCommandHandler;

        public BuyCreditsCommandHandlerTransactionDecorator(
            ILogger<BuyCreditsCommandHandlerTransactionDecorator> logger,
            BuyCreditsCommandHandler buyCreditsCommandHandler, CommandHandlerBaseDependencies dependencies) 
            : base(dependencies)
        {
            _buyCreditsCommandHandler = buyCreditsCommandHandler;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<BuyCreditsCommand> request,
            IEventOutbox eventOutbox,
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