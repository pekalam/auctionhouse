﻿using Core.DomainFramework;

namespace Auctions.Domain
{
    /// <summary>
    /// Holds images added to auction when user creates new auction. 
    /// It also contains logic to handle how long it can be used until it expires and should be deleted.
    /// </summary>
    public class AuctionCreateSession
    {
        public const int DEFAULT_SESSION_MAX_TIME = 1000 * 60 * 10;
        public static int SESSION_MAX_TIME { get; internal set; } = DEFAULT_SESSION_MAX_TIME;

        public AuctionImages AuctionImages { get; private set; }

        public DateTime DateCreated { get; private set; }

        public UserId OwnerId { get; private set; }

        public AuctionCreateSession(AuctionImages auctionImages, DateTime dateCreated, UserId ownerId)
        {
            AuctionImages = auctionImages;
            DateCreated = dateCreated;
            OwnerId = ownerId;
        }

        private AuctionCreateSession(UserId owner)
        {
            OwnerId = owner;
            DateCreated = DateTime.UtcNow;
            AuctionImages = new();
        }

        public static AuctionCreateSession CreateSession(UserId ownerId)
        {
            return new AuctionCreateSession(ownerId);
        }

        private void CheckIsSessionValid()
        {
            if (!ValidateSessionTime(DateCreated))
            {
                throw new DomainException($"Sesion time expired");
            }
        }

        public IReadOnlyList<AuctionImage?> AuctionImagesList => AuctionImages.ToList();

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
            AuctionImages.ClearAll();
        }

        public void AddOrReplaceImage(AuctionImage img, int imgNum)
        {
            CheckIsSessionValid();
            AuctionImages[imgNum] = img;
        }

        public Auction CreateAuction(AuctionArgs auctionArgs)
        {
            CheckIsSessionValid();
            if (OwnerId == null)
            {
                throw new DomainException("User must be registered to create auction");
            }
            var args = new AuctionArgs.Builder()
                .From(auctionArgs)
                .SetImages(AuctionImages)
                .SetOwner(OwnerId)
                .Build();
            var auction = new Auction(args);
            return auction;
        }

    }
}