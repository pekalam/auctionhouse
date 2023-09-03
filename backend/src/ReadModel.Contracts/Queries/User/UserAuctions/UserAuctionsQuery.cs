using System;
using System.ComponentModel.DataAnnotations;
using Auctions.Domain;
using Common.Application.Commands.Attributes;
using Common.Application.Queries;

namespace ReadModel.Contracts.Queries.User.UserAuctions
{
    public enum UserAuctionsSorting
    {
        DATE_CREATED = 0
    }

    public enum UserAuctionsSortDir
    {
        DESCENDING = 0, ASCENDING
    }

    [AuthorizationRequired]
    public class UserAuctionsQuery : IQuery<UserAuctionsQueryResult>
    {
        [Range(0, int.MaxValue)]
        public int Page { get; set; } = 0;

        public bool ShowArchived { get; set; } = false;

        public UserAuctionsSorting Sorting { get; set; }

        public UserAuctionsSortDir SortingDirection { get; set; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }
    }
}
