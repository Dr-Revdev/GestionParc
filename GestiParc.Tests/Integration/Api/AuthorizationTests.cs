using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestiParc.Core.DTOs;
using GestiParc.Tests.TestUtils.Api;
using Xunit;

namespace GestiParc.Tests.Integration.Api;

public class AuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AdminOnlyEndpoint_WithoutToken_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/utilisateur");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_WithUserRole_Returns403()
    {
        using var client = _factory.CreateClient();

        var token = TestAuthTokenFactory.CreateToken(userId: 2, role: "USER");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/utilisateur");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_WithAdminRole_Returns200_AndPayload()
    {
        using var client = _factory.CreateClient();

        var token = TestAuthTokenFactory.CreateToken(userId: 1, role: "ADMIN");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/utilisateur");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<UtilisateurDto>>();
        Assert.NotNull(users);
        Assert.True(users!.Count >= 2);
    }

    [Fact]
    public async Task UserOrAdminEndpoint_WithUserRole_IsAuthorized_AndReturns400OnInvalidPayload()
    {
        using var client = _factory.CreateClient();

        var token = TestAuthTokenFactory.CreateToken(userId: 2, role: "USER");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // {} -> dto non null mais champs requis manquants => BadRequest attendu (preuve que l'autorisation est passee)
        var response = await client.PostAsJsonAsync("/api/utilisateur/changer-motdepasse", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
