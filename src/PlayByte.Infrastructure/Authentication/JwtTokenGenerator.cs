using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlayByte.Application.Abstractions.Security;
using PlayByte.Domain.Users;

namespace PlayByte.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator(IOptions<JwtSettings> options, TimeProvider timeProvider)
    : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = options.Value;

    public AccessToken Generate(UserId userId, string email)
    {
        var now = timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_settings.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(jwt, expires);
    }
}
