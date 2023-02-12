using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace WebAPI.Common.Auth
{
    public static class IdTokenManagerMiddlewareExtensions
    {
        public static bool IsIdTokenDeactivated(this HttpContext httpContext)
        {
            return !httpContext.Items.ContainsKey(IdTokenManagerMiddleware.TokenDeactivatedKey) ? false : (bool)httpContext.Items[IdTokenManagerMiddleware.TokenDeactivatedKey]!;
        }
    }

    internal class IdTokenManagerMiddleware
    {
        public const string TokenDeactivatedKey = "IsTokenDeactivated";
        private readonly RequestDelegate _next;

        public IdTokenManagerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IIdTokenManager idTokenManager)
        {
            var idToken = context.Request.Cookies["IdToken"];
            if (context.Request.Headers.TryGetValue("Authorization", out var authorization) && authorization.Count > 0)
            {
                var authorizationValue = authorization.First();
                if (authorizationValue.IndexOf("Bearer ") == 0)
                {
                    idToken = authorizationValue.Substring("Bearer ".Length);
                }
            }
            if (idToken != null)
            {
                context.Items[TokenDeactivatedKey] = await idTokenManager.CheckIsDeactivated(idToken, CancellationToken.None);
            }

            await _next(context);
        }
    }
}
