using System;
using System.Linq;
using AutoMapper;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Core.Query.Handlers
{
    public class AuctionReadProfile : Profile
    {
        public AuctionReadProfile()
        {
            CreateMap<UserIdentity, UserIdentityRead>()
                .ForMember(read => read.UserId, opt => opt.MapFrom(identity => identity.UserId.ToString()));
            CreateMap<Bid, BidRead>()
                .ForMember(read => read.AuctionId, opt => opt.MapFrom(bid => bid.AuctionId.ToString()));
            CreateMap<AuctionArgs, AuctionRead>();
            CreateMap<AuctionCreated, AuctionRead>()
                .ForMember(read => read.AuctionId, opt => opt.MapFrom(created => created.AuctionId.ToString()))
                .ForMember(read => read.Id, opt => opt.AllowNull())
                .ForMember(read => read.Version, opt => opt.MapFrom(created => created.AggVersion))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }


    public class AuctionCreatedHandler : EventConsumer<AuctionCreated>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;

        public AuctionCreatedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IEventSignalingService eventSignalingService) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
        }

        public override void Consume(IAppEvent<AuctionCreated> message)
        {
            var ev = message.Event;
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var auction = mapper.Map<AuctionCreated, AuctionRead>(ev, opt => opt.AfterMap((created, read) => mapper.Map(created.AuctionArgs, read)));
            

            var filter = Builders<UserRead>.Filter.Eq(f => f.UserIdentity.UserId, ev.AuctionArgs.Creator.UserId.ToString());
            var update = Builders<UserRead>.Update.Push(f => f.CreatedAuctions, ev.AuctionId.ToString());


            var session = _dbContext.Client.StartSession();

            session.StartTransaction();
            try
            {
                _dbContext.AuctionsReadModel.WithWriteConcern(WriteConcern.WMajority).InsertOne(session, auction);
                _dbContext.UsersReadModel.UpdateOne(session, filter, update);
                session.CommitTransaction();
            }
            catch (Exception)
            {
                session.AbortTransaction();
                _eventSignalingService.TrySendEventFailureToUser(message, ev.AuctionArgs.Creator);
                throw;
            }

            _eventSignalingService.TrySendEventCompletionToUser(message, ev.AuctionArgs.Creator);
        }
    }
}
