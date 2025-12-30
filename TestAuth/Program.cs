using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

class TestAuthViaApi
{
    static async Task Main()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("     TEST AUTHENTIFICATION VIA API - GestiParc");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        // Configuration
        Console.Write("URL de l'API (ex: http://localhost:5139): ");
        string? apiUrl = Console.ReadLine()?.TrimEnd('/');
        
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            Console.WriteLine("❌ URL API vide!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine();
        Console.Write("Username à tester (ex: admin): ");
        string? username = Console.ReadLine();
        
        Console.Write("Password à tester (ex: admin123): ");
        string? password = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("❌ Username ou password vide!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("ÉTAPE 1: Test de connectivité API");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            var pingUrl = $"{apiUrl}/api/ping";
            Console.WriteLine($"Test: GET {pingUrl}");
            var pingResponse = await httpClient.GetAsync(pingUrl);
            
            if (pingResponse.IsSuccessStatusCode)
            {
                var pingContent = await pingResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ API accessible: {pingContent}");
            }
            else
            {
                Console.WriteLine($"⚠️  API répond mais avec erreur: {pingResponse.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ Impossible de joindre l'API: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Vérifications:");
            Console.WriteLine("1. L'API est-elle lancée ? (dotnet run --project GestiParc.Api)");
            Console.WriteLine("2. L'URL est-elle correcte ?");
            Console.WriteLine("3. Le port est-il le bon ?");
            Console.ReadKey();
            return;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("❌ Timeout: L'API ne répond pas");
            Console.ReadKey();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("ÉTAPE 2: Test d'authentification");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        var authUrl = $"{apiUrl}/api/utilisateur/authentifier";
        Console.WriteLine($"POST {authUrl}");
        Console.WriteLine($"Body: {{ \"Username\": \"{username}\", \"Password\": \"***\" }}");
        Console.WriteLine();

        var request = new { Username = username, Password = password };

        try
        {
            var response = await httpClient.PostAsJsonAsync(authUrl, request);
            
            Console.WriteLine($"Status Code: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                Console.WriteLine("✅✅✅ AUTHENTIFICATION RÉUSSIE! ✅✅✅");
                Console.WriteLine();
                Console.WriteLine("Informations utilisateur:");
                Console.WriteLine($"  Username: {authResponse?.User?.Username}");
                Console.WriteLine($"  Nom: {authResponse?.User?.Nom} {authResponse?.User?.Prenom}");
                Console.WriteLine($"  Rôle: {authResponse?.User?.Role}");
                Console.WriteLine();
                Console.WriteLine("Token JWT:");
                Console.WriteLine($"  {authResponse?.Token?.Substring(0, Math.Min(50, authResponse.Token?.Length ?? 0))}...");
                Console.WriteLine($"  Expire dans: {authResponse?.ExpiresIn} secondes");
                Console.WriteLine();
                Console.WriteLine("🎉 L'authentification fonctionne CORRECTEMENT!");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("❌❌❌ AUTHENTIFICATION ÉCHOUÉE! ❌❌❌");
                Console.WriteLine();
                Console.WriteLine("L'API retourne 401 Unauthorized");
                Console.WriteLine();
                Console.WriteLine("CAUSES POSSIBLES:");
                Console.WriteLine("1. Le mot de passe est incorrect");
                Console.WriteLine("2. Le hash BCrypt en base ne correspond pas");
                Console.WriteLine("3. L'utilisateur n'existe pas");
                Console.WriteLine("4. L'utilisateur est inactif (actif = FALSE)");
                Console.WriteLine();
                Console.WriteLine("SOLUTIONS:");
                Console.WriteLine("1. Vérifier dans MySQL:");
                Console.WriteLine($"   SELECT * FROM utilisateurs WHERE username = '{username}';");
                Console.WriteLine();
                Console.WriteLine("2. Vérifier le hash avec l'outil TestBCrypt");
                Console.WriteLine();
                Console.WriteLine("3. Mettre à jour le hash si nécessaire");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("❌ Requête invalide (400 Bad Request)");
                Console.WriteLine($"Erreur: {error}");
                Console.WriteLine();
                Console.WriteLine("Le format de la requête est probablement incorrect");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Erreur inattendue: {response.StatusCode}");
                Console.WriteLine($"Réponse: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERREUR: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }

    class AuthResponse
    {
        public UserDto? User { get; set; }
        public string? Token { get; set; }
        public int ExpiresIn { get; set; }
    }

    class UserDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Role { get; set; }
    }
}
