using System.Data;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;

namespace GestiParc.Infrastructure.Data.Repositories;

// Repository MySQL pour les agents - Insert, Update, Delete, GetAll, GetById
public sealed class AgentMySqlRepository : IAgentRepository
{
    // Ajoute un nouvel agent dans la table agents
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
        command.Parameters.Add(CreateParameter(command, "@equipeId", (object?)agent.EquipeId));
        command.Parameters.Add(CreateParameter(command, "@siteId", (object?)agent.SiteId));
        command.Parameters.Add(CreateParameter(command, "@heberge", agent.Heberge));
        command.Parameters.Add(CreateParameter(command, "@commentaire", agent.Commentaire));

        command.ExecuteNonQuery();
    }

    // Modifie les infos d'un agent existant (nom, prenom, email, equipe, site...)
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
        command.Parameters.Add(CreateParameter(command, "@equipeId", (object?)agent.EquipeId));
        command.Parameters.Add(CreateParameter(command, "@siteId", (object?)agent.SiteId));
        command.Parameters.Add(CreateParameter(command, "@heberge", agent.Heberge));
        command.Parameters.Add(CreateParameter(command, "@commentaire", agent.Commentaire));

        command.ExecuteNonQuery();
    }

    // Supprime un agent par son IDRH
    public void Delete(string idrh)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM agents WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));
        command.ExecuteNonQuery();
    }

    // Récupère un agent par son IDRH (lance une exception si introuvable)
    public AgentDto? GetById(string idrh)
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
            return null;

        return MapToDto(reader);
    }

    // Récupère tous les agents triés par nom/prenom
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

    // Récupère tous les agents d'une équipe
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

    // Récupère tous les agents d'un site
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

    // Vérifie si un agent existe avec cet IDRH
    public bool Exists(string idrh)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM agents WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Convertit une ligne SQL en AgentDto
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

    // Helper pour créer un paramètre SQL (gère les null avec DBNull.Value)
    private static IDbDataParameter CreateParameter(IDbCommand command, string name, object? value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        return param;
    }
}
