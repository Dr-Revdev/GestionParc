using GestiParc.Api.Services;
using GestiParc.Core.Domain.Entities;
using Microsoft.Extensions.Options;

namespace GestiParc.Tests.TestUtils.Api;

public static class TestAuthTokenFactory
{
    public const string Issuer = "GestiParc";
    public const string Audience = "GestiParc.Ui";

    // 32+ chars pour HMAC-SHA256
    public const string JwtSecret = "0123456789abcdef0123456789abcdef";

    public static string CreateToken(int userId, string role)
    {
        var jwtService = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            Secret = JwtSecret,
            ExpirationMinutes = 60
        }));

        var user = new Utilisateur
        {
            Id = userId,
            Username = "test",
            Role = role
        };

        return jwtService.CreateToken(user, out _);
    }
}
