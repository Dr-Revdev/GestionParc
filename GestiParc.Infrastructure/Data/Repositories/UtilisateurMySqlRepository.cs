using System.Data;
using System.Security.Cryptography;
using System.Text;
using GestiParc.Core.Domain.Entities;
using GestiParc.Core.Interfaces.Repositories;

namespace GestiParc.Infrastructure.Data.Repositories;

public class UtilisateurMySqlRepository : IUtilisateurRepository
{
    /// <summary>
    /// Authentifie un utilisateur avec son username et mot de passe
    /// </summary>
    /// <param name="username">Nom d'utilisateur</param>
    /// <param name="password">Mot de passe en clair</param>
    /// <returns>L'utilisateur si authentifié, null sinon</returns>
    public Utilisateur? Authentifier(string username, string password)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        var passwordHash = HashPassword(password);
        
        command.CommandText = @"
            SELECT id, username, password_hash, nom, prenom, role, 
                   date_creation, derniere_connexion, actif
            FROM utilisateurs 
            WHERE username = @username 
              AND password_hash = @passwordHash 
              AND actif = TRUE";
        
        var paramUsername = command.CreateParameter();
        paramUsername.ParameterName = "@username";
        paramUsername.Value = username;
        command.Parameters.Add(paramUsername);
        
        var paramPassword = command.CreateParameter();
        paramPassword.ParameterName = "@passwordHash";
        paramPassword.Value = passwordHash;
        command.Parameters.Add(paramPassword);
        
        using var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            var utilisateur = new Utilisateur
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Nom = reader.GetString(3),
                Prenom = reader.GetString(4),
                Role = reader.GetString(5),
                DateCreation = reader.GetDateTime(6),
                DerniereConnexion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Actif = reader.GetBoolean(8)
            };
            
            reader.Close();
            
            // Mettre à jour la date de dernière connexion
            UpdateDerniereConnexion(utilisateur.Id);
            
            return utilisateur;
        }
        
        return null;
    }

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    public Utilisateur? GetById(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT id, username, password_hash, nom, prenom, role, 
                   date_creation, derniere_connexion, actif
            FROM utilisateurs 
            WHERE id = @id";
        
        var paramId = command.CreateParameter();
        paramId.ParameterName = "@id";
        paramId.Value = id;
        command.Parameters.Add(paramId);
        
        using var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            return new Utilisateur
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Nom = reader.GetString(3),
                Prenom = reader.GetString(4),
                Role = reader.GetString(5),
                DateCreation = reader.GetDateTime(6),
                DerniereConnexion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Actif = reader.GetBoolean(8)
            };
        }
        
        return null;
    }

    /// <summary>
    /// Récupère tous les utilisateurs
    /// </summary>
    public List<Utilisateur> GetAll()
    {
        var utilisateurs = new List<Utilisateur>();
        
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT id, username, password_hash, nom, prenom, role, 
                   date_creation, derniere_connexion, actif
            FROM utilisateurs 
            ORDER BY nom, prenom";
        
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            utilisateurs.Add(new Utilisateur
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Nom = reader.GetString(3),
                Prenom = reader.GetString(4),
                Role = reader.GetString(5),
                DateCreation = reader.GetDateTime(6),
                DerniereConnexion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Actif = reader.GetBoolean(8)
            });
        }
        
        return utilisateurs;
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    public int Create(Utilisateur utilisateur, string password)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        var passwordHash = HashPassword(password);
        
        command.CommandText = @"
            INSERT INTO utilisateurs (username, password_hash, nom, prenom, role, actif)
            VALUES (@username, @passwordHash, @nom, @prenom, @role, @actif);
            SELECT LAST_INSERT_ID();";
        
        command.Parameters.Add(CreateParameter(command, "@username", utilisateur.Username));
        command.Parameters.Add(CreateParameter(command, "@passwordHash", passwordHash));
        command.Parameters.Add(CreateParameter(command, "@nom", utilisateur.Nom));
        command.Parameters.Add(CreateParameter(command, "@prenom", utilisateur.Prenom));
        command.Parameters.Add(CreateParameter(command, "@role", utilisateur.Role));
        command.Parameters.Add(CreateParameter(command, "@actif", utilisateur.Actif));
        
        return Convert.ToInt32(command.ExecuteScalar());
    }

    /// <summary>
    /// Met à jour un utilisateur (sans changer le mot de passe)
    /// </summary>
    public void Update(Utilisateur utilisateur)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = @"
            UPDATE utilisateurs 
            SET nom = @nom,
                prenom = @prenom,
                role = @role,
                actif = @actif
            WHERE id = @id";
        
        command.Parameters.Add(CreateParameter(command, "@id", utilisateur.Id));
        command.Parameters.Add(CreateParameter(command, "@nom", utilisateur.Nom));
        command.Parameters.Add(CreateParameter(command, "@prenom", utilisateur.Prenom));
        command.Parameters.Add(CreateParameter(command, "@role", utilisateur.Role));
        command.Parameters.Add(CreateParameter(command, "@actif", utilisateur.Actif));
        
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Change le mot de passe d'un utilisateur
    /// </summary>
    public void ChangePassword(int userId, string newPassword)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        var passwordHash = HashPassword(newPassword);
        
        command.CommandText = "UPDATE utilisateurs SET password_hash = @passwordHash WHERE id = @id";
        
        command.Parameters.Add(CreateParameter(command, "@id", userId));
        command.Parameters.Add(CreateParameter(command, "@passwordHash", passwordHash));
        
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Désactive un utilisateur (soft delete)
    /// </summary>
    public void Desactiver(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = "UPDATE utilisateurs SET actif = FALSE WHERE id = @id";
        
        command.Parameters.Add(CreateParameter(command, "@id", id));
        
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Vérifie si un username existe déjà
    /// </summary>
    public bool UsernameExists(string username, int? excludeId = null)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        if (excludeId.HasValue)
        {
            command.CommandText = "SELECT COUNT(*) FROM utilisateurs WHERE username = @username AND id != @excludeId";
            command.Parameters.Add(CreateParameter(command, "@username", username));
            command.Parameters.Add(CreateParameter(command, "@excludeId", excludeId.Value));
        }
        else
        {
            command.CommandText = "SELECT COUNT(*) FROM utilisateurs WHERE username = @username";
            command.Parameters.Add(CreateParameter(command, "@username", username));
        }
        
        var count = Convert.ToInt32(command.ExecuteScalar());
        
        return count > 0;
    }

    /// <summary>
    /// Met à jour la date de dernière connexion
    /// </summary>
    private void UpdateDerniereConnexion(int userId)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = "UPDATE utilisateurs SET derniere_connexion = @now WHERE id = @id";
        
        command.Parameters.Add(CreateParameter(command, "@id", userId));
        command.Parameters.Add(CreateParameter(command, "@now", DateTime.Now));
        
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Insère un nouvel utilisateur
    /// </summary>
    public void Insert(Utilisateur utilisateur)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        var passwordHash = HashPassword(utilisateur.PasswordHash); // Assume le mot de passe est passé en clair
        
        command.CommandText = @"
            INSERT INTO utilisateurs (username, password_hash, nom, prenom, role, date_creation, actif)
            VALUES (@username, @passwordHash, @nom, @prenom, @role, @dateCreation, @actif)";
        
        command.Parameters.Add(CreateParameter(command, "@username", utilisateur.Username));
        command.Parameters.Add(CreateParameter(command, "@passwordHash", passwordHash));
        command.Parameters.Add(CreateParameter(command, "@nom", utilisateur.Nom));
        command.Parameters.Add(CreateParameter(command, "@prenom", utilisateur.Prenom));
        command.Parameters.Add(CreateParameter(command, "@role", utilisateur.Role));
        command.Parameters.Add(CreateParameter(command, "@dateCreation", DateTime.Now));
        command.Parameters.Add(CreateParameter(command, "@actif", utilisateur.Actif));
        
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Supprime un utilisateur
    /// </summary>
    public void Delete(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = "DELETE FROM utilisateurs WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));
        
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Vérifie si un nom d'utilisateur existe déjà
    /// </summary>
    public bool ExistsByUsername(string username)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM utilisateurs WHERE username = @username";
        command.Parameters.Add(CreateParameter(command, "@username", username));
        
        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    /// <summary>
    /// Hash un mot de passe avec SHA256
    /// </summary>
    private string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// Crée un paramètre pour une commande SQL
    /// </summary>
    private IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }
}
