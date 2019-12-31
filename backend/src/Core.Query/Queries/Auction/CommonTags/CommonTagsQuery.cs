using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Core.Common;
using Core.Common.Query;
using MediatR;

namespace Core.Query.Queries.Auction.CommonTags
{
    public class CommonTagsQuery : IQuery<Views.CommonTags>
    {
        [Required]
        [MinLength(Common.Domain.Auctions.Tag.MIN_LENGTH)]
        [MaxLength(Common.Domain.Auctions.Tag.MAX_LENGTH)]
        public string Tag { get; set; }
    }
}
