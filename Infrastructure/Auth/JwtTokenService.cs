using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure.Auth;

namespace VerticalSliceArchitecture.Infrastructure.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly AuthOptions _opts;
        public JwtTokenService(AuthOptions opts) => _opts = opts;

        public string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_opts.AccessTokenMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
