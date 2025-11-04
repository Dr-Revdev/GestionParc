using System.Data;
using ProjetParc.Data.DTOs;
using ProjetParc.Data.Repositories.Contracts;

namespace ProjetParc.Data.Repositories.MySQL;

/// <summary>
/// Implémentation MySQL du repository pour la gestion des sites
/// </summary>
public sealed class SiteMySqlRepository : ISiteRepository
{
    public int Insert(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO sites (name) VALUES (@name); SELECT LAST_INSERT_ID();";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar());
    }

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

    public void Delete(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM sites WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));
        command.ExecuteNonQuery();
    }

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

    public bool ExistsByName(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sites WHERE name = @name";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

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
