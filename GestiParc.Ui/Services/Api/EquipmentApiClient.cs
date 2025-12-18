using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class EquipmentApiClient
{
    private static readonly HttpClient _http = ApiHttpClient.Instance;

    private static readonly object _lock = new object();
    private static List<EquipmentDto>? _cachedAll;
    private static DateTime _cachedAllExpiry = DateTime.MinValue;
    private static readonly TimeSpan AllCacheDuration = TimeSpan.FromSeconds(15);

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EquipmentDto>> GetAllAsync()
    {
        // Vérifier le cache d'abord
        lock (_lock)
            if (_cachedAll != null && DateTime.Now < _cachedAllExpiry)
                return _cachedAll;

        var response = await _http.GetAsync("api/equipment");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<EquipmentDto>>(stream, JsonOptions);

        var result = list ?? new List<EquipmentDto>();

        // Mettre en cache
        lock (_lock)
        {
            _cachedAll = result;
            _cachedAllExpiry = DateTime.Now.Add(AllCacheDuration);
        }

        return result;
    }

    private static void InvalidateCache()
    {
        lock (_lock)
        {
            _cachedAll = null;
            _cachedAllExpiry = DateTime.MinValue;
        }
    }

    public async Task<EquipmentDto?> GetByIdAsync(string id)
    {
        var response = await _http.GetAsync($"api/equipment/{id}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<EquipmentDto>(stream, JsonOptions);

    }

    public async Task CreateAsync(EquipmentDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/equipment", dto);
        response.EnsureSuccessStatusCode();

        // Invalider le cache après modification
        InvalidateCache();
    }

    public async Task UpdateAsync(string id, EquipmentDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/equipment/{id}", dto);
        response.EnsureSuccessStatusCode();

        // Invalider le cache après modification
        InvalidateCache();
    }

    public async Task DeleteAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/equipment/{id}");
        response.EnsureSuccessStatusCode();

        // Invalider le cache après modification
        InvalidateCache();
    }
}