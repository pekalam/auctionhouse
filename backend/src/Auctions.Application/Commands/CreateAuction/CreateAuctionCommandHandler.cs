using Auctions.Domain;
using Common.Application;
using Common.Application.Commands;
using Core.DomainFramework;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : CommandHandlerBase<CreateAuctionCommand>
    {
        private readonly CreateAuctionCommandHandlerDepedencies _deps;

        public CreateAuctionCommandHandler(CreateAuctionCommandHandlerDepedencies depedencies) : base(depedencies.logger)
        {
            _deps = depedencies;
        }



        private AuctionArgs GetAuctionArgs(CreateAuctionCommand request, UserId owner)
        {
            //var category = _deps.categoryBuilder.FromCategoryNamesList(request.Category);
            var builder = new AuctionArgs.Builder()
                .SetStartDate(request.StartDate)
                .SetEndDate(request.EndDate)
                //.SetCategory(category)
                .SetProduct(request.Product)
                .SetTags(request.Tags)
                .SetName(request.Name)
                .SetBuyNowOnly(request.BuyNowOnly.Value)
                .SetOwner(owner);
            if (request.BuyNowPrice != null)
            {
                builder.SetBuyNow(request.BuyNowPrice.Value);
            }

            return builder.Build();
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<CreateAuctionCommand> request, CancellationToken cancellationToken)
        {
            //var auction = request.Command.AuctionCreateSession.CreateAuction(GetAuctionArgs(request.Command, new Common.Domain.Auctions.UserId(user.AggregateId)));

            var response = RequestStatus.CreatePending(request.CommandContext);
            //var addAuctionSequence = new AtomicSequence<Auction>()
            //    .AddTask(AddToRepository, AddToRepository_Rollback)
            //    .AddTask(SheduleAuctionEndTask, ScheduleAuctionEndTask_Rollback)
            //    .AddTask(ChangeImagesMetadata, ChangeImagesMetadata_Rollback)
            //    .AddTask((param, _) => PublishEvents(auction, user, request), null);

            return Task.FromResult(response);
        }
    }
}