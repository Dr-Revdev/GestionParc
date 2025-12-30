using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GestiParc.Core.DTOs;
using GestiParc.Ui.Services;

namespace GestiParc.Ui.Services.Api;

public class UtilisateurApiClient
{
    private static readonly HttpClient _http = ApiHttpClient.Instance;

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

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = "";
        try
        {
            body = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            // ignore
        }

        var message = string.IsNullOrWhiteSpace(body)
            ? $"Erreur API (HTTP {(int)response.StatusCode})."
            : body;

        throw new InvalidOperationException(message);
    }

    public async Task<UtilisateurDto?> AuthentifierAsync(string username, string password)
    {
        var request = new AuthRequestDto { Username = username, Password = password };
        var response = await _http.PostAsJsonAsync("api/utilisateur/authentifier", request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return null;
        
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var auth = await JsonSerializer.DeserializeAsync<AuthResponseDto>(stream, JsonOptions);
        if (auth == null)
            return null;

        if (!string.IsNullOrWhiteSpace(auth.Token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
            // Sauvegarder le token de manière sécurisée
            TokenStorage.SaveToken(auth.Token, username);
        }

        return auth.User;
    }

    /// <summary>
    /// Restaure le token depuis le stockage sécurisé (au démarrage de l'app)
    /// </summary>
    public static void RestoreToken(string username)
    {
        var token = TokenStorage.GetToken(username);
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Déconnecte l'utilisateur (supprime le token)
    /// </summary>
    public static void Logout()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        TokenStorage.ClearToken();
    }

    public async Task<UtilisateurDto> CreateAsync(CreateUtilisateurRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/utilisateur", dto);
        await EnsureSuccessOrThrowAsync(response);

        var stream = await response.Content.ReadAsStreamAsync();
        var created = await JsonSerializer.DeserializeAsync<UtilisateurDto>(stream, JsonOptions);
        if (created == null)
            throw new InvalidOperationException("Réponse API invalide.");

        return created;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/utilisateur/changer-motdepasse", dto);
        await EnsureSuccessOrThrowAsync(response);
    }

    public async Task<ResetPasswordResponseDto> ResetPasswordAsync(int userId)
    {
        var response = await _http.PostAsync($"api/utilisateur/{userId}/reset-motdepasse", content: null);
        await EnsureSuccessOrThrowAsync(response);

        var stream = await response.Content.ReadAsStreamAsync();
        var payload = await JsonSerializer.DeserializeAsync<ResetPasswordResponseDto>(stream, JsonOptions);
        if (payload == null)
            throw new InvalidOperationException("Réponse API invalide.");

        return payload;
    }

    public async Task SetRoleAsync(int userId, string role)
    {
        var request = new SetRoleRequestDto { Role = role };
        var response = await _http.PutAsJsonAsync($"api/utilisateur/{userId}/role", request);
        await EnsureSuccessOrThrowAsync(response);
    }

    public async Task SetActifAsync(int userId, bool actif)
    {
        var request = new SetActifRequestDto { Actif = actif };
        var response = await _http.PutAsJsonAsync($"api/utilisateur/{userId}/actif", request);
        await EnsureSuccessOrThrowAsync(response);
    }

    public async Task UpdateAsync(int id, UtilisateurDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/utilisateur/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/utilisateur/{id}");
        await EnsureSuccessOrThrowAsync(response);
    }
}
