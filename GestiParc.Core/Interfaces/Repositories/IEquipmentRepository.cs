using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Repositories;

/// <summary>
/// Interface du repository pour la gestion des équipements
/// </summary>
public interface IEquipmentRepository
{

    void Insert(EquipmentDto equipment);
    void Update(EquipmentDto equipment);
    void Delete(string idEquipement);
    EquipmentDto? GetById(string idEquipement);
    List<EquipmentDto> GetAll();
    List<EquipmentDto> GetByAgent(string idrh);
    List<EquipmentDto> GetFreeEquipments();
    List<EquipmentDto> GetLoanedEquipments();
    List<EquipmentDto> GetByType(int typeId);
    bool Exists(string idEquipement);
}
