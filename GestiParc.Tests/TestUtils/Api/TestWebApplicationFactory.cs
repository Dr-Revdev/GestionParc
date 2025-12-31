using GestiParc.Core.Domain.Entities;
using GestiParc.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestiParc.Tests.TestUtils.Api;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // IMPORTANT: ne pas utiliser "Development" ici.
        // Program.cs charge un fichier .env en environnement Development, ce qui peut ecraser nos valeurs de config
        // (notamment le secret JWT) et faire echouer les tests.
        builder.UseEnvironment("Testing");

        // On force aussi les variables d'environnement car elles ont une precedence elevee dans la config ASP.NET Core.
        // Ca evite qu'une variable vide/mal definie sur le poste ecrase nos valeurs.
        Environment.SetEnvironmentVariable("Jwt__Secret", TestAuthTokenFactory.JwtSecret);
        Environment.SetEnvironmentVariable("JWT_SECRET", TestAuthTokenFactory.JwtSecret);
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__GestiParcDb",
            "Server=localhost;Database=gestiparc_test;Uid=test;Pwd=test;");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestAuthTokenFactory.JwtSecret,
                ["ConnectionStrings:GestiParcDb"] = "Server=localhost;Database=gestiparc_test;Uid=test;Pwd=test;",
                ["LOG_DIR"] = Path.Combine(Path.GetTempPath(), "gestiparc-tests-logs")
            };

            configBuilder.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Remplace le repository DB par un fake 100% en memoire.
            services.RemoveAll(typeof(IUtilisateurRepository));
            services.AddSingleton<IUtilisateurRepository>(new FakeUtilisateurRepository());
        });
    }

    private sealed class FakeUtilisateurRepository : IUtilisateurRepository
    {
        private readonly List<Utilisateur> _users =
        [
            new Utilisateur { Id = 1, Username = "admin", Role = "ADMIN", Actif = true },
            new Utilisateur { Id = 2, Username = "user", Role = "USER", Actif = true }
        ];

        public Utilisateur? Authentifier(string username, string password) =>
            _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) && u.Actif);

        public Utilisateur? GetByUsername(string username) =>
            _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        public Utilisateur? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

        public List<Utilisateur> GetAll() => _users.ToList();

        public int Create(Utilisateur utilisateur, string password)
        {
            var nextId = _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;
            utilisateur.Id = nextId;
            _users.Add(utilisateur);
            return nextId;
        }

        public bool CheckPassword(int userId, string password) => true;

        public void ChangePassword(int userId, string newPassword)
        {
            // no-op
        }

        public void SetRole(int userId, string role)
        {
            var u = GetById(userId);
            if (u != null) u.Role = role;
        }

        public void SetActif(int userId, bool actif)
        {
            var u = GetById(userId);
            if (u != null) u.Actif = actif;
        }

        public void Insert(Utilisateur utilisateur)
        {
            if (utilisateur.Id == 0)
                utilisateur.Id = _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;
            _users.Add(utilisateur);
        }

        public void Update(Utilisateur utilisateur)
        {
            var existing = GetById(utilisateur.Id);
            if (existing == null) return;

            existing.Username = utilisateur.Username;
            existing.Nom = utilisateur.Nom;
            existing.Prenom = utilisateur.Prenom;
            existing.Role = utilisateur.Role;
            existing.Actif = utilisateur.Actif;
        }

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null) _users.Remove(existing);
        }

        public bool ExistsByUsername(string username) =>
            _users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}
