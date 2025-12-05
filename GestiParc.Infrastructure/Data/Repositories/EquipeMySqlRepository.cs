using System.Data;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;

namespace GestiParc.Infrastructure.Data.Repositories;

// Repository MySQL pour les équipes - CRUD simple (Insert, Update, Delete, GetAll)
public sealed class EquipeMySqlRepository : IEquipeRepository
{
    // Crée une nouvelle équipe et retourne son ID
    public int Insert(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO equipes (name) VALUES (@name); SELECT LAST_INSERT_ID();";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Modifie le nom d'une équipe
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

    // Supprime une équipe par son ID
    public void Delete(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM equipes WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));
        command.ExecuteNonQuery();
    }

    // Récupère une équipe par son ID
    public EquipeDto? GetById(int id)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM equipes WHERE id = @id";
        command.Parameters.Add(CreateParameter(command, "@id", id));

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return new EquipeDto(reader.GetInt32(0), reader.GetString(1));
    }

    // Récupère toutes les équipes triées par nom
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

    // Vérifie si une équipe existe déjà avec ce nom (pour éviter les doublons)
    public bool ExistsByName(string name)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM equipes WHERE name = @name";
        command.Parameters.Add(CreateParameter(command, "@name", name));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Vérifie si l'équipe est utilisée par des agents (pour bloquer la suppression)
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
