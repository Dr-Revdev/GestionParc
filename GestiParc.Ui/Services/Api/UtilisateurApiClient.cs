using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Services.Api;

public class UtilisateurApiClient
{
    private static readonly HttpClient _http = new HttpClient
    {
        BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]!)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<UtilisateurDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/utilisateur");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var list = await JsonSerializer.DeserializeAsync<List<UtilisateurDto>>(stream, JsonOptions);

        return list ?? new List<UtilisateurDto>();
    }

    public async Task<UtilisateurDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/utilisateur/{id}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<UtilisateurDto>(stream, JsonOptions);
    }

    public async Task<UtilisateurDto?> AuthentifierAsync(string username, string password)
    {
        var request = new { Username = username, Password = password };
        var response = await _http.PostAsJsonAsync("api/utilisateur/authentifier", request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return null;
        
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<UtilisateurDto>(stream, JsonOptions);
    }

    public async Task CreateAsync(UtilisateurDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/utilisateur", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(int id, UtilisateurDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/utilisateur/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/utilisateur/{id}");
        response.EnsureSuccessStatusCode();
    }
}
