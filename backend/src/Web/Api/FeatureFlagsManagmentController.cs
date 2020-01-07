using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigCat.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "X-API-Key", Roles = "FeatureFlagsManagment")]
    public class FeatureFlagsManagmentController : ControllerBase
    {
        private IConfigCatClient _client;

        public FeatureFlagsManagmentController(IConfigCatClient client)
        {
            _client = client;
        }

        [HttpPost("refreshFeatureFlags")]
        public async Task RefreshFeatureFlags()
        {
            await _client.ForceRefreshAsync();
        }
    }
}
