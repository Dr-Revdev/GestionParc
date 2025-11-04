namespace ProjetParc.Data.DTOs;

/// <summary>
/// DTO représentant un équipement
/// </summary>
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
