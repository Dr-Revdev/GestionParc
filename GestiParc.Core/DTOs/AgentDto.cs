namespace GestiParc.Core.DTOs;

// DTO pour un agent (Idrh, Nom, Prenom, Email, EquipeId, SiteId...)
public sealed record AgentDto(
    string Idrh,
    string? Nom,
    string? Prenom,
    string? Email,
    int? EquipeId,
    int? SiteId,
    int Heberge,
    string? Commentaire
);
