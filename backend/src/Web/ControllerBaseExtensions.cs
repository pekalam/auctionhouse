using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Common;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Commands;

namespace Web
{
    public static class ControllerBaseExtensions
    {
        public static ActionResult<RequestStatusDto> StatusResponse(this ControllerBase controller,
            RequestStatus status)
        {
            var dto = (RequestStatusDto) status;
            if (status.Status == Status.COMPLETED || status.Status == Status.PENDING)
            {
                return controller.Ok(dto);
            }
            else
            {
                return controller.StatusCode(500, dto);
            }
        }
    }
}
