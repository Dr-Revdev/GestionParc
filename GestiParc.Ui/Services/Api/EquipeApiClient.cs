using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class EquipeApiClient
{
    private static readonly HttpClient _http = ApiHttpClient.Instance;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EquipeDto>> GetAllAsync()
    {
        // Vérifier le cache d'abord
        var cached = ApiCache.GetEquipes();
        if (cached != null)
            return cached;

        var response = await _http.GetAsync("api/equipe");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<EquipeDto>>(stream, JsonOptions);

        var result = list ?? new List<EquipeDto>();
        
        // Mettre en cache
        ApiCache.SetEquipes(result);
        
        return result;
    }

    public async Task<EquipeDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/equipe/{id}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<EquipeDto>(stream, JsonOptions);
    }

    public async Task CreateAsync(EquipeDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/equipe", dto);
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateEquipes();
    }

    public async Task UpdateAsync(int id, EquipeDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/equipe/{id}", dto);
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateEquipes();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/equipe/{id}");
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateEquipes();
    }
}