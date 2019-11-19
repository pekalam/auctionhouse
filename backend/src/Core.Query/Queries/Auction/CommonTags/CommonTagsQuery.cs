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
        public string Tag { get; set; }
    }
}
