using System;
using System.Linq;
using AutoMapper;
using Core.Common;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Core.Query.EventHandlers
{
    public class AuctionReadProfile : Profile
    {
        public AuctionReadProfile()
        {
            //CreateMap<UserId, UserIdentityRead>() //TODO
            //    .ForMember(read => read.UserId, opt => opt.MapFrom(identity => identity.ToString()));
            //CreateMap<Bid, BidRead>() //TODO
            //    .ForMember(read => read.AuctionId, opt => opt.MapFrom(bid => bid.AuctionId.ToString()));
            CreateMap<AuctionArgs, AuctionRead>();
            CreateMap<AuctionCreated, AuctionRead>()
                .ForMember(read => read.AuctionId, opt => opt.MapFrom(created => created.AuctionId.ToString()))
                .ForMember(read => read.Id, opt => opt.AllowNull())
                .ForMember(read => read.Version, opt => opt.MapFrom(created => created.AggVersion))
                .ForMember(read => read.DateCreated, opt => opt.Ignore())
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }


    public class AuctionCreatedHandler : EventConsumer<AuctionCreated>
    {
        
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<AuctionCreatedHandler> _logger;

        public AuctionCreatedHandler(
            IAppEventBuilder appEventBuilder, 
            ReadModelDbContext dbContext,
            ILogger<AuctionCreatedHandler> logger, 
            Func<ISagaNotifications> sagaNotificationsFactory, 
            Func<IReadModelNotifications> readModelNotificationsFactory) : base(appEventBuilder, logger, sagaNotificationsFactory, readModelNotificationsFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override void Consume(IAppEvent<AuctionCreated> message)
        {
            var ev = message.Event;
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var auction = mapper.Map<AuctionCreated, AuctionRead>(ev,
                opt => opt.AfterMap((created, read) => mapper.Map(created.AuctionArgs, read)));

            auction.DateCreated = DateTime.UtcNow;


            try
            {
                _dbContext.AuctionsReadModel.WithWriteConcern(new WriteConcern(mode: "majority", journal: true))
                    .InsertOne(auction);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Cannot create an auction");
                throw;
            }

        }
    }
}