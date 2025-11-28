using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Repositories;

/// <summary>
/// Interface du repository pour la gestion des équipes
/// </summary>
public interface IEquipeRepository
{
    int Insert(string name);
    void Update(EquipeDto equipe);
    void Delete(int id);
    EquipeDto GetById(int id);
    List<EquipeDto> GetAll();
    bool ExistsByName(string name);
    bool IsInUse(int id);
}
