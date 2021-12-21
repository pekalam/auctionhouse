using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.EventBus;

namespace Core.Common.DomainServices
{
    public class BuyNowService
    {
        private IUserRepository _userRepository;

        public BuyNowService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<Event> BuyNow(Auction auction, User buyer)
        {
            var generatedEvents = new List<Event>();

            foreach (var bid in auction.Bids)
            {
                if (!buyer.AggregateId.Value.Equals(bid.UserId))
                {
                    var user = _userRepository.FindUser(bid.UserId);
                    user.ReturnCredits(bid.Price);
                    generatedEvents.AddRange(user.PendingEvents);
                    _userRepository.UpdateUser(user);
                }
                else
                {
                    buyer.ReturnCredits(bid.Price);
                }
            }

            auction.BuyNow(buyer);

            return generatedEvents;
        }
    }
}
