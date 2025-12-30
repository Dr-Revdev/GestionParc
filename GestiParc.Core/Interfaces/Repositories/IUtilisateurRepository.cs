using GestiParc.Core.Domain.Entities;

namespace GestiParc.Core.Interfaces.Repositories;

/// <summary>
/// Interface du repository pour la gestion des utilisateurs
/// </summary>
public interface IUtilisateurRepository
{
    Utilisateur? Authentifier(string username, string password);
    Utilisateur? GetByUsername(string username);
    Utilisateur? GetById(int id);
    List<Utilisateur> GetAll();

    int Create(Utilisateur utilisateur, string password);
    bool CheckPassword(int userId, string password);
    void ChangePassword(int userId, string newPassword);

    void SetRole(int userId, string role);
    void SetActif(int userId, bool actif);

    void Insert(Utilisateur utilisateur);
    void Update(Utilisateur utilisateur);
    void Delete(int id);
    bool ExistsByUsername(string username);
}