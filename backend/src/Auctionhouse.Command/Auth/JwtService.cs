﻿using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auctionhouse.Command.Auth
{
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(JwtSettings settings)
        {
            _jwtSettings = settings;
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
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Sid, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Role, "User")
            };
            var secKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SymetricKey));
            var creds = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience,
                claims: claims, signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}