using ProjetParc.Data.DTOs;

namespace ProjetParc.Data.Repositories.Contracts;

/// <summary>
/// Interface du repository pour la gestion des agents
/// </summary>
public interface IAgentRepository
{
    /// <summary>
    /// Insère un nouvel agent dans la base de données
    /// </summary>
    void Insert(AgentDto agent);

    /// <summary>
    /// Met à jour un agent existant
    /// </summary>
    void Update(AgentDto agent);

    /// <summary>
    /// Supprime un agent par son IDRH
    /// </summary>
    void Delete(string idrh);

    /// <summary>
    /// Récupère un agent par son IDRH
    /// </summary>
    AgentDto GetById(string idrh);

    /// <summary>
    /// Récupère tous les agents
    /// </summary>
    List<AgentDto> GetAll();

    /// <summary>
    /// Récupère les agents d'une équipe
    /// </summary>
    List<AgentDto> GetByEquipe(int equipeId);

    /// <summary>
    /// Récupère les agents d'un site
    /// </summary>
    List<AgentDto> GetBySite(int siteId);

    /// <summary>
    /// Vérifie si un agent existe
    /// </summary>
    bool Exists(string idrh);
}
