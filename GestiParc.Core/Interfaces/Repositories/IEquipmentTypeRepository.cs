using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Repositories;

public interface IEquipmentTypeRepository
{
    List<EquipmentTypeDto> GetAll();
    EquipmentTypeDto? GetById(int id);
    void Update(EquipmentTypeDto equipmentType);
    int Insert(string name);
    void Delete(int id);
}
