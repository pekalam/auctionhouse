using AuctionBids.Domain.Shared;
using Core.Common.Domain;
using Core.DomainFramework;
using System.Diagnostics;

namespace AuctionBids.Domain
{
    public sealed class BidCancelled : Event
    {
        public Guid BidId { get; set; }

        public BidCancelled() : base("bidCanceled")
        {
        }
    }

    public class Bid : Entity<BidId>
    {
        public UserId UserId { get; private set; }

        public decimal Price { get; private set; }

        public DateTime Date { get; private set; }

        public bool Accepted { get; private set; }

        public bool Cancelled { get; private set; }

        public Bid(BidId bidId, UserId userId, decimal price, DateTime date, bool accepted, bool cancelled,
            IInternalEventAdd parentAdd, IInternalEventApply? parentApply = null) : base(parentAdd, parentApply)
        {
            //TODO checks
            Id = bidId;
            UserId = userId;
            Price = price;
            Date = date;
            Accepted = accepted;
            Cancelled = cancelled;
        }

        internal Bid(IInternalEventAdd parentAdd, IInternalEventApply? parentApply = null) : base(parentAdd, parentApply)
        {
        }

        public static Bid CreateAccepted(AuctionBids parent, UserId userId, decimal price)
        {
            return new Bid(BidId.New(), userId, price, DateTime.UtcNow, true, false, parent);
        }

        public static Bid CreateNotAccepted(AuctionBids parent, UserId userId, decimal price)
        {
            return new Bid(BidId.New(), userId, price, DateTime.UtcNow, false, false, parent);
        }

        public void Cancel()
        {
            if (Cancelled)
            {
                throw new DomainException($"Cannot cancel already cancelled bid {Id}");
            }
            Cancelled = true;
            AddEvent(new BidCancelled { BidId = Id });
        }

        public override void Apply(Event @event)
        {
            switch (@event)
            {
                case BidCancelled e:
                    Debug.Assert(e.BidId == Id);
                    Cancelled = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        internal new void ApplyInternal(Event @event) => base.ApplyInternal(@event);
    }

    public sealed class BidId : GuidId<BidId>
    {
        public BidId(Guid value) : base(value) { }
        public static BidId New() => new BidId(Guid.NewGuid());
    }


}
