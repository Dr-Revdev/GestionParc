namespace ProjetParc.Data.DTOs;

/// <summary>
/// DTO représentant une relation Agent-Site (table Travail)
/// </summary>
public sealed record TravailDto(
    string Idrh,
    int SiteId
);
