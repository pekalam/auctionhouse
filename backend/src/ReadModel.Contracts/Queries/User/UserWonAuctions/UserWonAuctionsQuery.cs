﻿using System.ComponentModel.DataAnnotations;
using Auctions.Domain;
using Common.Application.Commands.Attributes;
using Common.Application.Queries;
using System;
using ReadModel.Contracts.Queries.User.UserAuctions;

namespace ReadModel.Contracts.Queries.User.UserWonAuctions
{
    [AuthorizationRequired]
    public class UserWonAuctionsQuery : IQuery<UserWonAuctionQueryResult>
    {
        [Range(0, int.MaxValue)] public int Page { get; }

        public UserAuctionsSorting Sorting { get; }

        public UserAuctionsSortDir SortingDirection { get; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public UserWonAuctionsQuery(int page, UserAuctionsSorting sorting, UserAuctionsSortDir sortingDirection)
        {
            Page = page;
            Sorting = sorting;
            SortingDirection = sortingDirection;
        }
    }
}