using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;

namespace Core.Common.Domain.AuctionCreateSession
{
    public class AuctionCreateSession
    {
        public const int DEFAULT_SESSION_MAX_TIME = 1000 * 60 * 10;
        public static int SESSION_MAX_TIME { get; internal set; } = DEFAULT_SESSION_MAX_TIME;

        public AuctionImage[] SessionAuctionImages { get; }
        public DateTime DateCreated { get; private set; }

        public UserIdentity Creator { get; }

        public AuctionCreateSession(AuctionImage[] sessionAuctionImages, DateTime dateCreated, UserIdentity creator)
        {
            if (sessionAuctionImages.Length < Auction.MAX_IMAGES)
            {
                throw new DomainException("");
            }
            SessionAuctionImages = sessionAuctionImages;
            DateCreated = dateCreated;
            Creator = creator;
        }

        internal AuctionCreateSession(UserIdentity creator)
        {
            Creator = creator;
            DateCreated = DateTime.UtcNow;
            SessionAuctionImages = new AuctionImage[Auction.MAX_IMAGES];
        }

        private void CheckIsSessionValid()
        {
            if (!ValidateSessionTime(DateCreated))
            {
                throw new DomainException($"Sesion time expired");
            }
        }

        public IReadOnlyList<AuctionImage> AuctionImages => SessionAuctionImages.ToList();

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
            DateCreated = DateTime.UtcNow;
            for (int i = 0; i < SessionAuctionImages.Length; i++)
            {
                SessionAuctionImages[i] = null;
            }
        }

        public void AddOrReplaceImage(AuctionImage img, int imgNum)
        {
            CheckIsSessionValid();   
            if (imgNum > SessionAuctionImages.Length)
            {
                throw new DomainException($"Cannot add more than {SessionAuctionImages.Length} images");
            }

            SessionAuctionImages[imgNum] = img;
        }

        public Auction CreateAuction(AuctionArgs auctionArgs)
        {
            CheckIsSessionValid();
            if (Creator == null)
            {
                throw new DomainException("User must be registered to create auction");
            }
            var args = new AuctionArgs.Builder()
                .From(auctionArgs)
                .SetImages(SessionAuctionImages)
                .SetOwner(Creator)
                .Build();
            var auction = new Auction(args);
            return auction;
        }

    }
}