﻿using System;
using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Domain.Users;
using Core.Common.Query;
using MediatR;

namespace Core.Query.Queries.User.UserAuctions
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
        [Range(0, Int32.MaxValue)]
        public int Page { get; set; } = 0;

        public bool ShowArchived { get; set; } = false;

        public UserAuctionsSorting Sorting { get; set; }

        public UserAuctionsSortDir SortingDirection { get; set; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }
    }
}
