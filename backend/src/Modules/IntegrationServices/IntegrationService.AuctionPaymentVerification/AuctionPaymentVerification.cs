using Auctions.Domain;
using Auctions.Domain.Services;
using System.Diagnostics;
using UserPayments.Domain.Services;
using Users.Domain.Repositories;

namespace IntegrationService.AuctionPaymentVerification
{
    internal class AuctionPaymentVerification : IAuctionPaymentVerification
    {
        private readonly IUserRepository _users;
        private readonly PaymentMethodVerificationService _paymentMethodVerification;

        public AuctionPaymentVerification(IUserRepository users, PaymentMethodVerificationService paymentMethodVerification)
        {
            _users = users;
            _paymentMethodVerification = paymentMethodVerification;
        }

        public async Task<bool> Verification(Auction auction, UserId buyerId, string paymentMethod)
        {
            Debug.Assert(auction.BuyNowPrice != null);
            //checks if user exists, but could also call dedicted service in users module
            var user = _users.FindUser(new(buyerId.Value)); 

            if (user == null)
            {
                return false;
            }
            if (user.Credits < auction.BuyNowPrice!.Value)
            {
                return false;
            }

            var verifiedPositively = await _paymentMethodVerification.Verify(new(paymentMethod), new(buyerId.Value));
            return verifiedPositively;
        }
    }
}