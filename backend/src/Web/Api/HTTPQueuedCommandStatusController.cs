using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.EventBus;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Commands;

namespace Web.Api
{
    [ApiController]
    [Route("api/command")]
    public class HTTPQueuedCommandStatusController : ControllerBase
    {
        private HTTPQueuedCommandStatusService _commandStatusService;

        public HTTPQueuedCommandStatusController(HTTPQueuedCommandStatusService commandStatusService)
        {
            _commandStatusService = commandStatusService;
        }

        [HttpGet("{correlationId}")]
        public ActionResult<RequestStatusDto> GetCommandStatus(string correlationId)
        {
            var correlationIdObj = new CorrelationId(correlationId);
            var status = _commandStatusService.GetCommandStatus(correlationIdObj);

            if (status != null)
            {
                return Ok((RequestStatusDto)status);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
