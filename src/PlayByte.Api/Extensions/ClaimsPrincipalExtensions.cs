using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PlayByte.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Le o 'sub' (UserId) do token autenticado.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Token sem identificador de usuario valido.");
    }
}
