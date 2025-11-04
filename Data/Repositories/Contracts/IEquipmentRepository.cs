using ProjetParc.Data.DTOs;

namespace ProjetParc.Data.Repositories.Contracts;

/// <summary>
/// Interface du repository pour la gestion des équipements
/// </summary>
public interface IEquipmentRepository
{
    /// <summary>
    /// Insère un nouvel équipement dans la base de données
    /// </summary>
    void Insert(EquipmentDto equipment);

    /// <summary>
    /// Met à jour un équipement existant
    /// </summary>
    void Update(EquipmentDto equipment);

    /// <summary>
    /// Supprime un équipement par son ID
    /// </summary>
    void Delete(string idEquipement);

    /// <summary>
    /// Récupère un équipement par son ID
    /// </summary>
    EquipmentDto GetById(string idEquipement);

    /// <summary>
    /// Récupère tous les équipements
    /// </summary>
    List<EquipmentDto> GetAll();

    /// <summary>
    /// Récupère les équipements assignés à un agent
    /// </summary>
    List<EquipmentDto> GetByAgent(string idrh);

    /// <summary>
    /// Récupère les équipements libres (non prêtés)
    /// </summary>
    List<EquipmentDto> GetFreeEquipments();

    /// <summary>
    /// Récupère les équipements en prêt (etat_pret = 1)
    /// </summary>
    List<EquipmentDto> GetLoanedEquipments();

    /// <summary>
    /// Récupère les équipements par type
    /// </summary>
    List<EquipmentDto> GetByType(int typeId);

    /// <summary>
    /// Vérifie si un équipement existe
    /// </summary>
    bool Exists(string idEquipement);
}
