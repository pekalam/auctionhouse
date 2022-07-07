using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Common.WebAPI.Auth
{
    internal static class HttpContextExtensions
    {
        public static bool EndpointRequiresAuthorization(this HttpContext context)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            if (endpoint == null)
            {
                return false;
            }

            var auth = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
            if (auth == null)
            {
                return false;
            }
            return true;
        }
    }
}