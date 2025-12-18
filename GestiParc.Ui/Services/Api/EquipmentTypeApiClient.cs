using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class EquipmentTypeApiClient
{
    private static readonly HttpClient _http = ApiHttpClient.Instance;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EquipmentTypeDto>> GetAllAsync()
    {
        // Vérifier le cache d'abord
        var cached = ApiCache.GetTypes();
        if (cached != null)
            return cached;

        var response = await _http.GetAsync("api/equipmentType");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<EquipmentTypeDto>>(stream, JsonOptions);

        var result = list ?? new List<EquipmentTypeDto>();
        
        // Mettre en cache
        ApiCache.SetTypes(result);
        
        return result;
    }

    public async Task<EquipmentTypeDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/equipmentType/{id}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<EquipmentTypeDto>(stream, JsonOptions);
    }

    public async Task CreateAsync(EquipmentTypeDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/equipmentType", dto);
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateTypes();
    }

    public async Task UpdateAsync(int id, EquipmentTypeDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/equipmentType/{id}", dto);
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateTypes();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/equipmentType/{id}");
        response.EnsureSuccessStatusCode();
        
        // Invalider le cache après modification
        ApiCache.InvalidateTypes();
    }
}