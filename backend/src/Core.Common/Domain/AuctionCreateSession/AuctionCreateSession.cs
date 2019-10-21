using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.AuctionCreateSession
{
    public class AuctionCreateSession
    {
        public const int DEFAULT_SESSION_MAX_TIME = 1000 * 60 * 10;
        public static int SESSION_MAX_TIME { get; internal set; } = DEFAULT_SESSION_MAX_TIME;

        private AuctionImage[] _auctionImages = new AuctionImage[Auction.MAX_IMAGES];
        private readonly UserIdentity _creator;
        private DateTime _dateCreated;
        
        //TODO: internal + serialization
        public AuctionCreateSession(UserIdentity creator)
        {
            _creator = creator;
            _dateCreated = DateTime.UtcNow;
        }

        private void CheckIsSessionValid()
        {
            if (!ValidateSessionTime(_dateCreated))
            {
                throw new DomainException($"Sesion time expired");
            }
        }

        public IReadOnlyList<AuctionImage> AuctionImages => _auctionImages.ToList();

        public static bool ValidateSessionTime(DateTime dateCreated)
        {
            if (DateTime.UtcNow.Subtract(dateCreated).Milliseconds > SESSION_MAX_TIME)
            {
                return false;
            }

            return true;
        }

        public void ResetSession()
        {
            CheckIsSessionValid();
            _dateCreated = DateTime.UtcNow;
            for (int i = 0; i < _auctionImages.Length; i++)
            {
                _auctionImages[i] = null;
            }
        }

        public void AddOrReplaceImage(AuctionImage img, int imgNum)
        {
            CheckIsSessionValid();   
            if (imgNum > _auctionImages.Length)
            {
                throw new DomainException($"Cannot add more than {_auctionImages.Length} images");
            }

            _auctionImages[imgNum] = img;
        }

        public Auction CreateAuction(decimal? buyNowPrice, DateTime startDate, DateTime endDate, Product product, Category category)
        {
            CheckIsSessionValid();
            if (_creator == null)
            {
                throw new DomainException("User must be registered to create auction");
            }
            var auction = new Auction(buyNowPrice, startDate, endDate, _creator, product, category, _auctionImages);
            return auction;
        }

    }
}