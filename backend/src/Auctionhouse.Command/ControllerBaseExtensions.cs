using Auctionhouse.Command.Dto;
using Common.Application;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Auctionhouse.Command
{
    internal static class ControllerBaseExtensions
    {
        public static ActionResult<RequestStatusDto> StatusResponse(this ControllerBase controller,
            RequestStatus status, RequestStatusDto dto)
        {
            if (status.Status == Status.COMPLETED || status.Status == Status.PENDING)
            {
                return controller.Ok(dto);
            }
            else
            {
                return controller.StatusCode((int)HttpStatusCode.BadRequest, dto);
            }
        }
    }
}
