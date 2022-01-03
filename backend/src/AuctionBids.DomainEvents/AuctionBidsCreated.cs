﻿using Core.Common.Domain;

namespace AuctionBids.DomainEvents
{
    public partial class Events
    {
        public partial class V1
        {

            public class AuctionBidsCreated : Event
            {
                public Guid AuctionBidsId { get; set; }
                public Guid AuctionId { get; set; }
                public Guid OwnerId { get; set; }

                public AuctionBidsCreated() : base("auctionBidsCreated")
                {
                }
            }



        }

    }


}