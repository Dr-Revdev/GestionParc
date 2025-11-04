using System.Data;
using ProjetParc.Data.DTOs;
using ProjetParc.Data.Repositories.Contracts;

namespace ProjetParc.Data.Repositories.MySQL;

/// <summary>
/// Implémentation MySQL du repository pour la gestion des équipes
/// </summary>
public sealed class EquipeMySqlRepository : IEquipeRepository
{
    public int Insert(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO equipes (name) VALUES (@name); SELECT LAST_INSERT_ID();";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(EquipeDto equipe)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE equipes SET name = @name WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", equipe.Id));
        command.Parameters.Add(CreateParameter(command, "@name", equipe.Name));
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM equipes WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));
        command.ExecuteNonQuery();
    }

    public EquipeDto GetById(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM equipes WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException($"Équipe avec ID '{id}' introuvable.");

        return new EquipeDto(reader.GetInt32(0), reader.GetString(1));
    }

    public List<EquipeDto> GetAll()
    {
        var list = new List<EquipeDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM equipes ORDER BY name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new EquipeDto(reader.GetInt32(0), reader.GetString(1)));

        return list;
    }

    public bool ExistsByName(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM equipes WHERE name = @name";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public bool IsInUse(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM agents WHERE equipe_id = @id";
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
