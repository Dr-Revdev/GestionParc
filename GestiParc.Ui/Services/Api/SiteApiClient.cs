using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class SiteApiClient
{
    private static readonly HttpClient _http = ApiHttpClient.Instance;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<SiteDto>> GetAllAsync()
    {
        // Vérifier le cache d'abord
        var cached = ApiCache.GetSites();
        if (cached != null)
            return cached;

        var response = await _http.GetAsync("api/site");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<SiteDto>>(stream, JsonOptions);

        var result = list ?? new List<SiteDto>();
        
        // Mettre en cache
        ApiCache.SetSites(result);
        
        return result;
    }

    public async Task<SiteDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/site/{id}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<SiteDto>(stream, JsonOptions);
    }

    public async Task CreateAsync(SiteDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/site", dto);
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateSites();
    }

    public async Task UpdateAsync(int id, SiteDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/site/{id}", dto);
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateSites();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/site/{id}");
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateSites();
    }
}