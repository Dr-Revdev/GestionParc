using ProjetParc.Data.DTOs;

namespace ProjetParc.Data.Repositories.Contracts;

/// <summary>
/// Interface du repository pour la gestion des sites
/// </summary>
public interface ISiteRepository
{
    /// <summary>
    /// Insère un nouveau site dans la base de données
    /// </summary>
    int Insert(string name);

    /// <summary>
    /// Met à jour un site existant
    /// </summary>
    void Update(SiteDto site);

    /// <summary>
    /// Supprime un site par son ID
    /// </summary>
    void Delete(int id);

    /// <summary>
    /// Récupère un site par son ID
    /// </summary>
    SiteDto GetById(int id);

    /// <summary>
    /// Récupère tous les sites
    /// </summary>
    List<SiteDto> GetAll();

    /// <summary>
    /// Vérifie si un site existe par son nom
    /// </summary>
    bool ExistsByName(string name);

    /// <summary>
    /// Vérifie si un site est utilisé par des agents
    /// </summary>
    bool IsInUse(int id);
}
