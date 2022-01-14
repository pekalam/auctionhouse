using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CompletePayment
{
    public class CompletePaymentCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public Guid AuctionId { get; set; }
        public Guid BuyerId { get; set; }
    }

    public class CompletePaymentCommandHandler : CommandHandlerBase<CompletePaymentCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public CompletePaymentCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository allUserPayments, IUnitOfWorkFactory unitOfWorkFactory)
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _allUserPayments = allUserPayments;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CompletePaymentCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.BuyerId));

            var payment = userPayments.Payments.First(p => p.TransactionId.Value == request.Command.TransactionId);
            userPayments.CompletePayment(payment.Id); 

            using(var uow = _unitOfWorkFactory.Begin())
            {
                _allUserPayments.Update(userPayments);
                await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                userPayments.MarkPendingEventsAsHandled();
                uow.Commit();
            }

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
