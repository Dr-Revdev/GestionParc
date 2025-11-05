namespace ProjetParc.Data.DTOs;

// DTO pour la relation Agent-Site (Idrh + SiteId)
public sealed record TravailDto(
    string Idrh,
    int SiteId
);
