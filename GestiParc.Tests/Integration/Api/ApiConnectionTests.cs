using System.Net.Http;
using GestiParc.Tests.TestUtils;
using Xunit;

namespace GestiParc.Tests.Integration.Api;

[Trait("Category", "Integration")]
public class ApiConnectionTests
{
    [RequiresEnvironmentVariableFact("GESTIPARC_TEST_API_BASEURL")]
    public void Ping_ShouldReturnSuccessStatusCode()
    {
        var apiBaseUrl = Environment.GetEnvironmentVariable("GESTIPARC_TEST_API_BASEURL");
        Assert.False(string.IsNullOrWhiteSpace(apiBaseUrl));

        Assert.True(Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri));

        using var http = new HttpClient
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(5)
        };

        var response = http.GetAsync("api/ping").GetAwaiter().GetResult();
        Assert.True(response.IsSuccessStatusCode);
    }
}
