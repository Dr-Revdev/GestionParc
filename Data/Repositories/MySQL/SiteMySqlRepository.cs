using System.Data;
using ProjetParc.Data.DTOs;
using ProjetParc.Data.Repositories.Contracts;

namespace ProjetParc.Data.Repositories.MySQL;

// Repository MySQL pour les sites - CRUD simple (Insert, Update, Delete, GetAll)
public sealed class SiteMySqlRepository : ISiteRepository
{
    // Crée un nouveau site et retourne son ID
    public int Insert(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO sites (name) VALUES (@name); SELECT LAST_INSERT_ID();";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Modifie le nom d'un site
    public void Update(SiteDto site)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE sites SET name = @name WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", site.Id));
        command.Parameters.Add(CreateParameter(command, "@name", site.Name));
        command.ExecuteNonQuery();
    }

    // Supprime un site par son ID
    public void Delete(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM sites WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));
        command.ExecuteNonQuery();
    }

    // Récupère un site par son ID
    public SiteDto GetById(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM sites WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException($"Site avec ID '{id}' introuvable.");

        return new SiteDto(reader.GetInt32(0), reader.GetString(1));
    }

    // Récupère tous les sites triés par nom
    public List<SiteDto> GetAll()
    {
        var list = new List<SiteDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM sites ORDER BY name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new SiteDto(reader.GetInt32(0), reader.GetString(1)));

        return list;
    }

    // Vérifie si un site existe déjà avec ce nom (pour éviter les doublons)
    public bool ExistsByName(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sites WHERE name = @name";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Vérifie si le site est utilisé par des agents (pour bloquer la suppression)
    public bool IsInUse(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM agents WHERE site_id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
