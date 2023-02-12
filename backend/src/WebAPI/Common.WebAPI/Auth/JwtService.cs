using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Common.Auth
{
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IWebHostEnvironment _hostEnv;

        public JwtService(JwtSettings settings, IWebHostEnvironment hostEnv)
        {
            _jwtSettings = settings;
            _hostEnv = hostEnv;
        }

        public string GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            SecurityToken readToken;
            var claimsPrincipal = handler.ValidateToken(token, _jwtSettings.TokenValidationParameters, out readToken);
            if (claimsPrincipal == null)
            {
                throw new Exception($"Invalid token");
            }
            var id = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid);
            return id?.Value;
        }

        public string IssueToken(Guid userId, string userName)
        {
            return IssueTokenCore(userId, userName, DateTime.UtcNow.AddSeconds(_jwtSettings.ExpireTimeSec));
        }

        private string IssueTokenCore(Guid userId, string userName, DateTime expires)
        {
            var issuedAt = DateTime.UtcNow;
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Sid, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("iat", (issuedAt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.ToString())
            };
            var secKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SymetricKey));
            var creds = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience,
                claims: claims, signingCredentials: creds, notBefore: issuedAt, expires: expires);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void SetCookie(string token, HttpResponse response)
        {
            response.Cookies.Append("IdToken", token, new()
            {
                HttpOnly = true,
                Secure = !_hostEnv.IsDevelopment(),
            });
        }

        public bool TryExtendLifetimeOfToken(string token, out string? newToken)
        {
            var handler = new JwtSecurityTokenHandler();
            SecurityToken readToken;
            ClaimsPrincipal claimsPrincipal;
            try
            {
                claimsPrincipal = handler.ValidateToken(token, _jwtSettings.TokenValidationParameters, out readToken);
            }
            catch (Exception ex)
            {
                newToken = null;
                return false;
            }

            if (readToken.ValidTo > DateTime.UtcNow)
            {
                //TODO executed too frequently
                var userId = Guid.Parse(claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.Sid).Value);
                var userName = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                newToken = IssueTokenCore(userId, userName, readToken.ValidFrom.AddSeconds(_jwtSettings.ExpireTimeSec));
                return true;
            }
            newToken = null;
            return false;
        }
    }
}