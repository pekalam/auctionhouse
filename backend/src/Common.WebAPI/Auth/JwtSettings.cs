using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Common.WebAPI.Auth
{
    internal static class JwtConfiguration
    {
        public static void ConfigureJwt(JwtBearerOptions options, JwtSettings settings)
        {
            options.Audience = settings.Audience;
            options.TokenValidationParameters = settings.TokenValidationParameters;
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    return AssignTokenFromMsgContext(context);
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.Cookies.Delete("IdToken");
                    return Task.CompletedTask;
                }
            };
        }

        private static Task AssignTokenFromMsgContext(MessageReceivedContext context)
        {
            if (!context.HttpContext.EndpointRequiresAuthorization()) //TODO does it mean that it should be ignored in other requests?
            {
                return Task.CompletedTask;
            }

            if (context.HttpContext.IsIdTokenDeactivated())
            {
                context.Token = null;
            }
            else
            {
                context.Token = context.Request.Cookies["IdToken"]; //TODO is bearer header ignored??
            }
            return Task.CompletedTask;
        }
    }


    public class JwtSettings
    {
        public string SymetricKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireTimeSec { get; set; } = 60 * 45; //45MIN

        public TokenValidationParameters TokenValidationParameters =>
            new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateLifetime = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(SymetricKey)),
                ClockSkew = TimeSpan.Zero,
            };
    }
}