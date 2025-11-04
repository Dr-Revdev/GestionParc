using System.Collections.Generic;
using System.Data;
using ProjetParc.Data.DTOs;

namespace ProjetParc.Data.Repositories.MySQL;

/// <summary>
/// Repository pour la gestion de la table Travail (relations Agent-Site) en MySQL
/// </summary>
public class TravailMySqlRepository
{
    /// <summary>
    /// Insère une nouvelle relation agent-site
    /// </summary>
    public void Insert(TravailDto travail)
    {
        using var connection = DbFactory.Create();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Travail (idrh, site_id) 
            VALUES (@idrh, @site_id)";

        command.Parameters.Add(CreateParameter(command, "@idrh", travail.Idrh));
        command.Parameters.Add(CreateParameter(command, "@site_id", travail.SiteId));

        connection.Open();
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Supprime toutes les relations d'un agent spécifique
    /// </summary>
    public void DeleteByAgent(string idrh)
    {
        using var connection = DbFactory.Create();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Travail WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));

        connection.Open();
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Récupère toutes les relations agent-site d'un agent
    /// </summary>
    public List<TravailDto> GetByAgent(string idrh)
    {
        using var connection = DbFactory.Create();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT idrh, site_id FROM Travail WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));

        connection.Open();
        using var reader = command.ExecuteReader();

        var travails = new List<TravailDto>();
        while (reader.Read())
        {
            travails.Add(MapToDto(reader));
        }

        return travails;
    }

    /// <summary>
    /// Récupère toutes les relations agent-site
    /// </summary>
    public List<TravailDto> GetAll()
    {
        using var connection = DbFactory.Create();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT idrh, site_id FROM Travail";

        connection.Open();
        using var reader = command.ExecuteReader();

        var travails = new List<TravailDto>();
        while (reader.Read())
        {
            travails.Add(MapToDto(reader));
        }

        return travails;
    }

    private static TravailDto MapToDto(IDataReader reader)
    {
        return new TravailDto(
            Idrh: reader.GetString(0),
            SiteId: reader.GetInt32(1)
        );
    }

    private static IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? System.DBNull.Value;
        return parameter;
    }
}
