using System.Data;
using ProjetParc.Data.DTOs;
using ProjetParc.Data.Repositories.Contracts;

namespace ProjetParc.Data.Repositories.MySQL;

/// <summary>
/// Implémentation MySQL du repository pour la gestion des agents
/// </summary>
public sealed class AgentMySqlRepository : IAgentRepository
{
    public void Insert(AgentDto agent)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO agents (idrh, nom, prenom, email, equipe_id, site_id, heberge, commentaire)
            VALUES (@idrh, @nom, @prenom, @email, @equipeId, @siteId, @heberge, @commentaire)";

        command.Parameters.Add(CreateParameter(command, "@idrh", agent.Idrh));
        command.Parameters.Add(CreateParameter(command, "@nom", agent.Nom));
        command.Parameters.Add(CreateParameter(command, "@prenom", agent.Prenom));
        command.Parameters.Add(CreateParameter(command, "@email", agent.Email));
        command.Parameters.Add(CreateParameter(command, "@equipeId", agent.EquipeId));
        command.Parameters.Add(CreateParameter(command, "@siteId", agent.SiteId));
        command.Parameters.Add(CreateParameter(command, "@heberge", agent.Heberge));
        command.Parameters.Add(CreateParameter(command, "@commentaire", agent.Commentaire));

        command.ExecuteNonQuery();
    }

    public void Update(AgentDto agent)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE agents 
            SET nom = @nom, prenom = @prenom, email = @email, 
                equipe_id = @equipeId, site_id = @siteId, 
                heberge = @heberge, commentaire = @commentaire
            WHERE idrh = @idrh";

        command.Parameters.Add(CreateParameter(command, "@idrh", agent.Idrh));
        command.Parameters.Add(CreateParameter(command, "@nom", agent.Nom));
        command.Parameters.Add(CreateParameter(command, "@prenom", agent.Prenom));
        command.Parameters.Add(CreateParameter(command, "@email", agent.Email));
        command.Parameters.Add(CreateParameter(command, "@equipeId", agent.EquipeId));
        command.Parameters.Add(CreateParameter(command, "@siteId", agent.SiteId));
        command.Parameters.Add(CreateParameter(command, "@heberge", agent.Heberge));
        command.Parameters.Add(CreateParameter(command, "@commentaire", agent.Commentaire));

        command.ExecuteNonQuery();
    }

    public void Delete(string idrh)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM agents WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));
        command.ExecuteNonQuery();
    }

    public AgentDto GetById(string idrh)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT idrh, nom, prenom, email, equipe_id, site_id, heberge, commentaire
            FROM agents 
            WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException($"Agent avec IDRH '{idrh}' introuvable.");

        return MapToDto(reader);
    }

    public List<AgentDto> GetAll()
    {
        var list = new List<AgentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT idrh, nom, prenom, email, equipe_id, site_id, heberge, commentaire
            FROM agents 
            ORDER BY nom, prenom";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    public List<AgentDto> GetByEquipe(int equipeId)
    {
        var list = new List<AgentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT idrh, nom, prenom, email, equipe_id, site_id, heberge, commentaire
            FROM agents 
            WHERE equipe_id = @equipeId
            ORDER BY nom, prenom";
        command.Parameters.Add(CreateParameter(command, "@equipeId", equipeId));

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    public List<AgentDto> GetBySite(int siteId)
    {
        var list = new List<AgentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT idrh, nom, prenom, email, equipe_id, site_id, heberge, commentaire
            FROM agents 
            WHERE site_id = @siteId
            ORDER BY nom, prenom";
        command.Parameters.Add(CreateParameter(command, "@siteId", siteId));

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    public bool Exists(string idrh)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM agents WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static AgentDto MapToDto(IDataReader reader)
    {
        return new AgentDto(
            Idrh: reader.GetString(0),
            Nom: reader.IsDBNull(1) ? null : reader.GetString(1),
            Prenom: reader.IsDBNull(2) ? null : reader.GetString(2),
            Email: reader.IsDBNull(3) ? null : reader.GetString(3),
            EquipeId: reader.IsDBNull(4) ? null : reader.GetInt32(4),
            SiteId: reader.IsDBNull(5) ? null : reader.GetInt32(5),
            Heberge: reader.GetInt32(6),
            Commentaire: reader.IsDBNull(7) ? null : reader.GetString(7)
        );
    }

    private static IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
