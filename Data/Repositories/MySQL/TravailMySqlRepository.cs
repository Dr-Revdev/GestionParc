using System.Collections.Generic;
using System.Data;
using ProjetParc.Data.DTOs;

namespace ProjetParc.Data.Repositories.MySQL;

// Repository pour la table Travail (relation many-to-many Agent-Site)
public class TravailMySqlRepository
{
    // Insère une nouvelle relation agent-site
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

    // Supprime toutes les relations d'un agent
    public void DeleteByAgent(string idrh)
    {
        using var connection = DbFactory.Create();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Travail WHERE idrh = @idrh";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));

        connection.Open();
        command.ExecuteNonQuery();
    }

    // Récupère toutes les relations pour un agent
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

    // Récupère toutes les relations agent-site
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

    // Convertit une ligne SQL en TravailDto
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
