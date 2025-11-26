using ProjetParc.Data.Models;

namespace ProjetParc.Data
{
    /// <summary>
    /// Gère la session utilisateur courante dans l'application
    /// </summary>
    public static class SessionManager
    {
        /// <summary>
        /// L'utilisateur actuellement connecté (null si non connecté)
        /// </summary>
        public static Utilisateur UtilisateurCourant { get; set; }

        /// <summary>
        /// Vérifie si un utilisateur est connecté
        /// </summary>
        public static bool EstConnecte => UtilisateurCourant != null;

        /// <summary>
        /// Vérifie si l'utilisateur connecté est administrateur
        /// </summary>
        public static bool EstAdmin => EstConnecte && UtilisateurCourant.EstAdmin;

        /// <summary>
        /// Déconnecte l'utilisateur courant
        /// </summary>
        public static void Deconnecter()
        {
            UtilisateurCourant = null;
        }
    }
}
