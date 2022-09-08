using Auctions.Domain;
using Auctions.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    internal class InMemAuctionCreateSessionStore : IAuctionCreateSessionStore
    {
        public static InMemAuctionCreateSessionStore Instance { get; } = new InMemAuctionCreateSessionStore();

        public static InMemAuctionCreateSessionStore Create() => new InMemAuctionCreateSessionStore();

        private AuctionCreateSession? _auctionCreateSession;

        public AuctionCreateSession GetExistingSession()
        {
            return _auctionCreateSession;
        }

        public void RemoveSession()
        {
            _auctionCreateSession = null;
        }

        public void SaveSession(AuctionCreateSession session)
        {
            _auctionCreateSession = session;
        }

        public bool SessionExists()
        {
            return _auctionCreateSession != null;
        }
    }
}
