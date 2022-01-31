using Microsoft.AspNetCore.Http;

namespace Common.WebAPI.Auth
{
    internal class IdTokenSlidingExpirationMiddleware
    {
        private readonly RequestDelegate _next;

        public IdTokenSlidingExpirationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, JwtService jwtService)
        {
            var idToken = context.Request.Cookies["IdToken"];
            //TODO (crucial for performance) - exclude with options some urls fetched automatically very often (ex. images)
            //token should be reissued only when user performs some action on site
            if (idToken != null && !context.IsIdTokenDeactivated())
            {
                if(jwtService.TryExtendLifetimeOfToken(idToken, out var newToken))
                {
                    context.Response.Cookies.Append("IdToken", newToken!);
                }
            }

            await _next(context);
        }
    }
}
