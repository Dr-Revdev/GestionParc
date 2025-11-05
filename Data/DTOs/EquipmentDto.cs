namespace ProjetParc.Data.DTOs;

// DTO pour un équipement (IdEquipement, TypeId, Nom, CodeParc, NumeroSerie...)
public sealed record EquipmentDto(
    string IdEquipement,
    int TypeId,
    string Nom,
    string CodeParc,
    string NumeroSerie,
    string Marque,
    string Commentaire,
    int EtatPret,
    string Idrh,
    string DateRenduDsem
);
