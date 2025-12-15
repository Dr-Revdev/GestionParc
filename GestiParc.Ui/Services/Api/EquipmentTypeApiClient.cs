using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class EquipmentTypeApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EquipmentTypeDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/equipmentType");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<EquipmentTypeDto>>(stream, JsonOptions);

        return list ?? new List<EquipmentTypeDto>();
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
    }

    public async Task UpdateAsync(int id, EquipmentTypeDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/equipmentType/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/equipmentType/{id}");
        response.EnsureSuccessStatusCode();
    }
}