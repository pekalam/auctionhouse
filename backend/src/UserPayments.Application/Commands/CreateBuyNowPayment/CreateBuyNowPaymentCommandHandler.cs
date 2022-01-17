using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CreateBuyNowPayment
{
    using Common.Application.Events;
    using Domain;
    using Domain.Shared;

    public class CreateBuyNowPaymentCommandHandler : CommandHandlerBase<CreateBuyNowPaymentCommand>
    {
        private readonly IUserPaymentsRepository _userPayments;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        public CreateBuyNowPaymentCommandHandler(ILogger<CreateBuyNowPaymentCommandHandler> logger, IUserPaymentsRepository userPayments, 
            CommandHandlerBaseDependencies dependencies, IUnitOfWorkFactory unitOfWorkFactory)
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _userPayments = userPayments;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(
            AppCommand<CreateBuyNowPaymentCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var userPayments = await _userPayments.WithUserId(new UserId(request.Command.BuyerId));

            var payment = userPayments.CreateBuyNowPayment(new TransactionId(request.Command.TransactionId),
                            request.Command.Amount, request.Command.PaymentMethod, paymentTargetId: new PaymentTargetId(request.Command.AuctionId));

            using (var uow = _unitOfWorkFactory.Begin())
            {
                _userPayments.Update(userPayments);
                await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
                uow.Commit();
            }

            userPayments.MarkPendingEventsAsHandled();



            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}
