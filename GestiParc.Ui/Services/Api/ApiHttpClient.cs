using System.Configuration;
using System.Net.Http;

namespace GestiParc.Ui.Services.Api;

public static class ApiHttpClient
{
    public static HttpClient Instance { get; } = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };
}