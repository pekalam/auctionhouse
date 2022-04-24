using Auctions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Tests.Base.Domain.ModelBuilders.Shared
{
    public static class GivenUserId
    {
        public static UserId Build() => UserId.New();
    }
}
