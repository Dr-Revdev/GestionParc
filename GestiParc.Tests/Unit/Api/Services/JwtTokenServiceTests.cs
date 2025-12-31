using System.IdentityModel.Tokens.Jwt;
using GestiParc.Api.Services;
using GestiParc.Core.Domain.Entities;
using Microsoft.Extensions.Options;
using Xunit;

namespace GestiParc.Tests.Unit.Api.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_UsesUserIdAsSub_AndNormalizesRole()
    {
        // Arrange
        var options = Options.Create(new JwtOptions
        {
            Issuer = "GestiParc",
            Audience = "GestiParc.Ui",
            Secret = "0123456789abcdef0123456789abcdef", // 32 chars min
            ExpirationMinutes = 60
        });

        var service = new JwtTokenService(options);
        var user = new Utilisateur
        {
            Id = 42,
            Username = "alice",
            Role = "admin" // volontairement en minuscule
        };

        // Act
        var token = service.CreateToken(user, out var expiresInSeconds);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.InRange(expiresInSeconds, 1, 60 * 60);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        Assert.Equal("42", sub);
        Assert.Equal("ADMIN", role);

        Assert.Equal("GestiParc", jwt.Issuer);
        Assert.Contains("GestiParc.Ui", jwt.Audiences);
    }

    [Theory]
    [InlineData(null, "USER")]
    [InlineData("", "USER")]
    [InlineData("  ", "USER")]
    [InlineData("User", "USER")]
    [InlineData("ADMIN", "ADMIN")]
    [InlineData("something-else", "USER")]
    public void CreateToken_NormalizesUnexpectedRoles(string? roleInput, string expectedRole)
    {
        // Arrange
        var options = Options.Create(new JwtOptions
        {
            Issuer = "GestiParc",
            Audience = "GestiParc.Ui",
            Secret = "0123456789abcdef0123456789abcdef",
            ExpirationMinutes = 60
        });

        var service = new JwtTokenService(options);
        var user = new Utilisateur
        {
            Id = 1,
            Username = "bob",
            Role = roleInput ?? ""
        };

        // Act
        var token = service.CreateToken(user, out _);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        Assert.Equal(expectedRole, role);
    }
}
