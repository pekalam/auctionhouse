using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Command;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Web.Dto.Commands
{
    public class RequestStatusDto
    {
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }

        public static explicit operator RequestStatusDto(RequestStatus response)
        {
            return new RequestStatusDto()
            {
                CorrelationId = response.CorrelationId.Value,
                Status = response.Status.ToString(),
                ExtraData = response.ExtraData
            };
        }
    }
}
