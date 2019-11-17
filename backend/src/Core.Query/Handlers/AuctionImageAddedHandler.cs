using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Auth;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.Handlers
{
    public class AuctionImageAddedHandler : EventConsumer<AuctionImageAdded>
    {
        private ReadModelDbContext _dbContext;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<AuctionImageAddedHandler> _logger;

        public AuctionImageAddedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, IEventSignalingService eventSignalingService, IUserIdentityService userIdentityService, ILogger<AuctionImageAddedHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _eventSignalingService = eventSignalingService;
            _userIdentityService = userIdentityService;
            _logger = logger;
        }

        private void AddImg(AuctionImageAdded auctionEvent)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, auctionEvent.AuctionId.ToString());
            var update = Builders<AuctionRead>.Update.Push(f => f.AuctionImages, auctionEvent.AddedImage);
            try
            {
                _dbContext.AuctionsReadModel.FindOneAndUpdate(filter, update);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot add image to read model");
                throw;
            }
        }

        public override void Consume(IAppEvent<AuctionImageAdded> appEvent)
        {
            var signedUser = _userIdentityService.GetSignedInUserIdentity();

            AddImg(appEvent.Event);

            _eventSignalingService.TrySendEventCompletionToUser(appEvent, signedUser);
        }
    }
}
