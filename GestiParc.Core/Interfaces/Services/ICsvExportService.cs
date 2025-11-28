namespace GestiParc.Core.Interfaces.Services;

/// <summary>
/// Service d'export de données au format CSV
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Génère un CSV contenant tous les agents
    /// </summary>
    /// <param name="filePath">Chemin du fichier CSV à créer</param>
    void ExportAgents(string filePath);

    /// <summary>
    /// Génère un CSV contenant tous les équipements
    /// </summary>
    /// <param name="filePath">Chemin du fichier CSV à créer</param>
    void ExportEquipments(string filePath);

    /// <summary>
    /// Génère un CSV contenant les prêts actifs
    /// </summary>
    /// <param name="filePath">Chemin du fichier CSV à créer</param>
    void ExportLoans(string filePath);

    /// <summary>
    /// Génère un export complet (agents, équipements, prêts) dans un dossier
    /// </summary>
    /// <param name="folderPath">Chemin du dossier où créer les fichiers</param>
    void ExportAll(string folderPath);
}
