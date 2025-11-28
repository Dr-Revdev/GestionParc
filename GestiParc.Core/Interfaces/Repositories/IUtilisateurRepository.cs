using GestiParc.Core.Domain.Entities;

namespace GestiParc.Core.Interfaces.Repositories;

/// <summary>
/// Interface du repository pour la gestion des utilisateurs
/// </summary>
public interface IUtilisateurRepository
{
    Utilisateur? Authentifier(string username, string password);
    Utilisateur? GetById(int id);
    List<Utilisateur> GetAll();
    void Insert(Utilisateur utilisateur);
    void Update(Utilisateur utilisateur);
    void Delete(int id);
    bool ExistsByUsername(string username);
}