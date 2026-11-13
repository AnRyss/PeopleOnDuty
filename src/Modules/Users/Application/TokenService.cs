using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PeopleOnDuty.src.Modules.Users;

namespace src.Modules.Users.Application
{
    public class TokenService(IConfiguration config)
    {
        public string Generate(User user)
        {
            var keyString = config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key missing");
            var issuer = config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer missing");
            var audience = config["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience missing");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}