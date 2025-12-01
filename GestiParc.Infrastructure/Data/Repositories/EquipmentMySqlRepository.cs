using System.Data;
using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;

namespace GestiParc.Infrastructure.Data.Repositories;

// Repository MySQL pour les équipements - Insert, Update, Delete, GetAll, GetById, GetByAgent
public sealed class EquipmentMySqlRepository : IEquipmentRepository
{
    // Ajoute un nouvel équipement dans la table equipements
    public void Insert(EquipmentDto equipment)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO equipements (id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem)
            VALUES (@id, @typeId, @nom, @codeParc, @numeroSerie, @marque, @commentaire, @etatPret, @idrh, @dateRendu)";

        command.Parameters.Add(CreateParameter(command, "@id", equipment.IdEquipement));
        command.Parameters.Add(CreateParameter(command, "@typeId", equipment.TypeId));
        command.Parameters.Add(CreateParameter(command, "@nom", (object?)equipment.Nom));
        command.Parameters.Add(CreateParameter(command, "@codeParc", (object?)equipment.CodeParc));
        command.Parameters.Add(CreateParameter(command, "@numeroSerie", (object?)equipment.NumeroSerie));
        command.Parameters.Add(CreateParameter(command, "@marque", (object?)equipment.Marque));
        command.Parameters.Add(CreateParameter(command, "@commentaire", (object?)equipment.Commentaire));
        command.Parameters.Add(CreateParameter(command, "@etatPret", equipment.EtatPret));
        command.Parameters.Add(CreateParameter(command, "@idrh", equipment.Idrh));
        command.Parameters.Add(CreateParameter(command, "@dateRendu", equipment.DateRenduDsem));

        command.ExecuteNonQuery();
    }

    // Modifie les infos d'un équipement (type, nom, code parc, état prêt, agent affecté...)
    public void Update(EquipmentDto equipment)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE equipements 
            SET type_id = @typeId, nom = @nom, code_parc = @codeParc, 
                numero_serie = @numeroSerie, marque = @marque, commentaire = @commentaire,
                etat_pret = @etatPret, idrh = @idrh, date_rendu_dsem = @dateRendu
            WHERE id_equipement = @id";

        command.Parameters.Add(CreateParameter(command, "@id", equipment.IdEquipement));
        command.Parameters.Add(CreateParameter(command, "@typeId", equipment.TypeId));
        command.Parameters.Add(CreateParameter(command, "@nom", (object?)equipment.Nom));
        command.Parameters.Add(CreateParameter(command, "@codeParc", (object?)equipment.CodeParc));
        command.Parameters.Add(CreateParameter(command, "@numeroSerie", (object?)equipment.NumeroSerie));
        command.Parameters.Add(CreateParameter(command, "@marque", (object?)equipment.Marque));
        command.Parameters.Add(CreateParameter(command, "@commentaire", (object?)equipment.Commentaire));
        command.Parameters.Add(CreateParameter(command, "@etatPret", equipment.EtatPret));
        command.Parameters.Add(CreateParameter(command, "@idrh", equipment.Idrh));
        command.Parameters.Add(CreateParameter(command, "@dateRendu", equipment.DateRenduDsem));

        command.ExecuteNonQuery();
    }

    // Supprime un équipement par son ID
    public void Delete(string idEquipement)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM equipements WHERE id_equipement = @id";
        command.Parameters.Add(CreateParameter(command, "@id", idEquipement));
        command.ExecuteNonQuery();
    }

    // Récupère un équipement par son ID (lance une exception si introuvable)
    public EquipmentDto GetById(string idEquipement)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem
            FROM equipements 
            WHERE id_equipement = @id";
        command.Parameters.Add(CreateParameter(command, "@id", idEquipement));

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException($"Équipement avec ID '{idEquipement}' introuvable.");

        return MapToDto(reader);
    }

    // Récupère tous les équipements triés par nom/code parc
    public List<EquipmentDto> GetAll()
    {
        var list = new List<EquipmentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem
            FROM equipements 
            ORDER BY nom, code_parc";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    // Récupère tous les équipements affectés à un agent (par son IDRH)
    public List<EquipmentDto> GetByAgent(string idrh)
    {
        var list = new List<EquipmentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem
            FROM equipements 
            WHERE idrh = @idrh
            ORDER BY nom, code_parc";
        command.Parameters.Add(CreateParameter(command, "@idrh", idrh));

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    // Récupère tous les équipements libres (etat_pret = 0)
    public List<EquipmentDto> GetFreeEquipments()
    {
        var list = new List<EquipmentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem
            FROM equipements 
            WHERE etat_pret = 0
            ORDER BY nom, code_parc";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    // Récupère tous les équipements prêtés (etat_pret = 1)
    public List<EquipmentDto> GetLoanedEquipments()
    {
        var list = new List<EquipmentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem
            FROM equipements 
            WHERE etat_pret = 1
            ORDER BY nom, code_parc";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    // Récupère tous les équipements d'un type donné (ex: tous les PC portables)
    public List<EquipmentDto> GetByType(int typeId)
    {
        var list = new List<EquipmentDto>();
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire, etat_pret, idrh, date_rendu_dsem
            FROM equipements 
            WHERE type_id = @typeId
            ORDER BY nom, code_parc";
        command.Parameters.Add(CreateParameter(command, "@typeId", typeId));

        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(MapToDto(reader));

        return list;
    }

    // Vérifie si un équipement existe avec cet ID
    public bool Exists(string idEquipement)
    {
        using var connection = DbFactory.Create();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM equipements WHERE id_equipement = @id";
        command.Parameters.Add(CreateParameter(command, "@id", idEquipement));
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Convertit une ligne SQL en EquipmentDto
    private static EquipmentDto MapToDto(IDataReader reader)
    {
        return new EquipmentDto(
            IdEquipement: reader.GetString(0),
            TypeId: reader.GetInt32(1),
            Nom: reader.IsDBNull(2) ? null : reader.GetString(2),
            CodeParc: reader.IsDBNull(3) ? null : reader.GetString(3),
            NumeroSerie: reader.IsDBNull(4) ? null : reader.GetString(4),
            Marque: reader.IsDBNull(5) ? null : reader.GetString(5),
            Commentaire: reader.IsDBNull(6) ? null : reader.GetString(6),
            EtatPret: reader.GetInt32(7),
            Idrh: reader.IsDBNull(8) ? null : reader.GetString(8),
            DateRenduDsem: reader.IsDBNull(9) ? null : reader.GetString(9)
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
