using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class EquipmentApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EquipmentDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/equipment");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<EquipmentDto>>(stream, JsonOptions);

        return list ?? new List<EquipmentDto>();
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
    }

    public async Task UpdateAsync(string id, EquipmentDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/equipment/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/equipment/{id}");
        response.EnsureSuccessStatusCode();
    }
}