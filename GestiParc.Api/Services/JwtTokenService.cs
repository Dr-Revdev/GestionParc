using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using GestiParc.Core.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GestiParc.Api.Services;

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Issuer))
            throw new InvalidOperationException("Configuration JWT manquante : Jwt:Issuer");
        if (string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("Configuration JWT manquante : Jwt:Audience");
        if (string.IsNullOrWhiteSpace(_options.Secret))
            throw new InvalidOperationException("Configuration JWT manquante : Jwt:Secret");
        if (_options.Secret.Trim().Length < 32)
            throw new InvalidOperationException("Configuration JWT invalide : Jwt:Secret doit faire au moins 32 caractères.");
        if (_options.ExpirationMinutes <= 0)
            throw new InvalidOperationException("Configuration JWT invalide : Jwt:ExpirationMinutes doit être > 0");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string CreateToken(Utilisateur user, out int expiresInSeconds)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.ExpirationMinutes);
        expiresInSeconds = (int)Math.Max(0, (expires - now).TotalSeconds);

        var role = NormalizeRole(user.Role);

        var claims = new List<Claim>
        {
            // sub = identifiant stable (évite les pseudos dans les logs/audits)
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new("role", role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string NormalizeRole(string? role)
    {
        var normalized = string.IsNullOrWhiteSpace(role) ? "USER" : role.Trim().ToUpperInvariant();
        return normalized is "ADMIN" or "USER" ? normalized : "USER";
    }
}

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Secret { get; set; } = "";
    public int ExpirationMinutes { get; set; } = 60;
}