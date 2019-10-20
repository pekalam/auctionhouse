using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Web
{
    public class JwtSettings
    {
        public string SymetricKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

        public void ConfigureJwt(JwtBearerOptions options)
        {
            options.Audience = Audience;
            options.TokenValidationParameters = TokenValidationParameters;
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var path = context.HttpContext.Request.Path;
                    if (path.StartsWithSegments("/app"))
                    {
                        var token = context.Request.Query["token"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        }

        public TokenValidationParameters TokenValidationParameters =>
            new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateLifetime = false,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(SymetricKey))
            };
    }
}