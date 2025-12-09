using System;
using System.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class EquipeApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EquipeDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/equipe");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<EquipeDto>>(stream, JsonOptions);

        return list ?? new List<EquipeDto>();
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
    }

    public async Task UpdateAsync(int id, EquipeDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/equipe/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/equipe/{id}");
        response.EnsureSuccessStatusCode();
    }
}