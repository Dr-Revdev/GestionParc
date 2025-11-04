namespace ProjetParc.Data.DTOs;

/// <summary>
/// DTO représentant un agent
/// </summary>
public sealed record AgentDto(
    string Idrh,
    string Nom,
    string Prenom,
    string Email,
    int? EquipeId,
    int? SiteId,
    int Heberge,
    string Commentaire
);
