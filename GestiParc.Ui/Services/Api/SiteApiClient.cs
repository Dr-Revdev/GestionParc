using System;
using System.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class SiteApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<SiteDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/site");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<SiteDto>>(stream, JsonOptions);

        return list ?? new List<SiteDto>();
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
    }

    public async Task UpdateAsync(int id, SiteDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/site/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/site/{id}");
        response.EnsureSuccessStatusCode();
    }
}