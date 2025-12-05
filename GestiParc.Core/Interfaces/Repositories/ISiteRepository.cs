using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Repositories;

/// <summary>
/// Interface du repository pour la gestion des sites
/// </summary>
public interface ISiteRepository
{
    int Insert(string name);
    void Update(SiteDto site);
    void Delete(int id);
    SiteDto? GetById(int id);
    List<SiteDto> GetAll();
    bool ExistsByName(string name);
    bool IsInUse(int id);
}
