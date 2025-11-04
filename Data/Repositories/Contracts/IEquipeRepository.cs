using ProjetParc.Data.DTOs;

namespace ProjetParc.Data.Repositories.Contracts;

/// <summary>
/// Interface du repository pour la gestion des équipes
/// </summary>
public interface IEquipeRepository
{
    /// <summary>
    /// Insère une nouvelle équipe dans la base de données
    /// </summary>
    int Insert(string name);

    /// <summary>
    /// Met à jour une équipe existante
    /// </summary>
    void Update(EquipeDto equipe);

    /// <summary>
    /// Supprime une équipe par son ID
    /// </summary>
    void Delete(int id);

    /// <summary>
    /// Récupère une équipe par son ID
    /// </summary>
    EquipeDto GetById(int id);

    /// <summary>
    /// Récupère toutes les équipes
    /// </summary>
    List<EquipeDto> GetAll();

    /// <summary>
    /// Vérifie si une équipe existe par son nom
    /// </summary>
    bool ExistsByName(string name);

    /// <summary>
    /// Vérifie si une équipe est utilisée par des agents
    /// </summary>
    bool IsInUse(int id);
}
