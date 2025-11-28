using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Repositories;

public interface IEquipmentTypeRepository
{
    List<EquipmentTypeDto> GetAll();
}
