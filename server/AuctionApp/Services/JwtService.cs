using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuctionApp.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuctionApp.Services;

public class JwtService(IConfiguration config)
{
    public string Generate(User user)
    {
        var secret   = config["Jwt:Secret"]!;
        var issuer   = config["Jwt:Issuer"]!;
        var audience = config["Jwt:Audience"]!;
        var expiry   = int.Parse(config["Jwt:ExpiryHours"] ?? "24");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username",                    user.Username),
            new Claim(ClaimTypes.Role,               user.Role.Name),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(expiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
