using GestiParc.Core.DTOs;

namespace GestiParc.Core.Interfaces.Services;

/// <summary>
/// Service pour préparer les données nécessaires à la génération de feuilles de remise PDF
/// </summary>
public interface IDocumentGenerationService
{
    /// <summary>
    /// Récupère toutes les données nécessaires pour générer une feuille de remise pour un agent
    /// </summary>
    /// <param name="agentId">L'identifiant de l'agent</param>
    /// <returns>Modèle contenant toutes les informations pour le document</returns>
    FeuilleRemiseData GetFeuilleRemiseData(string agentId);
}

/// <summary>
/// Modèle de données pour une feuille de remise
/// </summary>
public class FeuilleRemiseData
{
    public AgentDto Agent { get; set; } = null!;
    public string SiteName { get; set; } = "";
    public string EquipeName { get; set; } = "";
    public List<EquipmentRemiseItem> Equipments { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

/// <summary>
/// Représente un équipement dans la feuille de remise
/// </summary>
public class EquipmentRemiseItem
{
    public string TypeName { get; set; } = "";
    public string Name { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string Brand { get; set; } = "";
}
