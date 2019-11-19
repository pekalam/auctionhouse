using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Command;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Web.Dto.Commands
{
    public class CommandResponseDto
    {
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public object ResponseData { get; set; }

        public static explicit operator CommandResponseDto(CommandResponse response)
        {
            return new CommandResponseDto()
            {
                CorrelationId = response.CorrelationId.Value,
                Status = response.Status.ToString(),
                ResponseData = response.ResponseData
            };
        }
    }
}
