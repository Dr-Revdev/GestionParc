using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class AgentApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<AgentDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/agent");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<AgentDto>>(stream, JsonOptions);

        return list ?? new List<AgentDto>();
    }

    public async Task<AgentDto?> GetByIdAsync(string id)
    {
        var response = await _http.GetAsync($"api/agent/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<AgentDto>(stream, JsonOptions);
    }

    public async Task CreateAsync(AgentDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/agent", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(string id, AgentDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/agent/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/agent/{id}");
        response.EnsureSuccessStatusCode();
    }
}