namespace ProjetParc.Data.Repositories.Contracts
{

    // Petit DTO pour le binding (DisplayMember/ValueMember)
    public sealed record EquipmentTypeDto(int Id, string Name);

    public interface IEquipmentTypeRepository
    {
        List<EquipmentTypeDto> GetAll();
        // (on ajoutera plus tard : GetById, Insert, Update, Delete si besoin)
    }
}