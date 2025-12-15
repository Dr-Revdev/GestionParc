using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Services;

namespace GestiParc.Core.Services;

/// <summary>
/// Service pour préparer les données de génération de documents (logique métier pure)
/// </summary>
public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly List<AgentDto> _agents;
    private readonly List<EquipmentDto> _equipments;
    private readonly List<EquipmentTypeDto> _types;
    private readonly List<SiteDto> _sites;
    private readonly List<EquipeDto> _equipes;

    public DocumentGenerationService(
        List<AgentDto> agents,
        List<EquipmentDto> equipments,
        List<EquipmentTypeDto> types,
        List<SiteDto> sites,
        List<EquipeDto> equipes)
    {
        _agents = agents ?? throw new ArgumentNullException(nameof(agents));
        _equipments = equipments ?? throw new ArgumentNullException(nameof(equipments));
        _types = types ?? throw new ArgumentNullException(nameof(types));
        _sites = sites ?? throw new ArgumentNullException(nameof(sites));
        _equipes = equipes ?? throw new ArgumentNullException(nameof(equipes));
    }

    /// <summary>
    /// Récupère toutes les données nécessaires pour générer une feuille de remise pour un agent
    /// </summary>
    public FeuilleRemiseData GetFeuilleRemiseData(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("L'identifiant de l'agent est requis", nameof(agentId));
        }

        // Récupérer l'agent
        var agent = _agents.FirstOrDefault(a => a.Idrh == agentId);
        if (agent == null)
        {
            throw new InvalidOperationException($"Agent non trouvé : {agentId}");
        }

        // Récupérer les équipements en prêt (EtatPret == 1)
        var equipments = _equipments
            .Where(e => e.Idrh == agentId && e.EtatPret == 1)
            .ToList();

        // Récupérer les données de référence
        var types = _types;
        var sites = _sites;
        var equipes = _equipes;

        // Créer les dictionnaires pour les lookups
        var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
        var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);
        var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);

        // Construire le résultat
        var siteName = agent.SiteId.HasValue && siteDict.ContainsKey(agent.SiteId.Value)
            ? siteDict[agent.SiteId.Value]
            : "Non assigné";

        var equipeName = agent.EquipeId.HasValue && equipeDict.ContainsKey(agent.EquipeId.Value)
            ? equipeDict[agent.EquipeId.Value]
            : "Non assignée";

        // Convertir les équipements en items de remise, triés par type puis nom
        var equipmentItems = equipments
            .Select(eq => new EquipmentRemiseItem
            {
                TypeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "",
                Name = eq.Nom ?? eq.CodeParc ?? "Sans nom",
                SerialNumber = eq.NumeroSerie ?? "N/A",
                Brand = eq.Marque ?? "N/A"
            })
            .OrderBy(item => item.TypeName)
            .ThenBy(item => item.Name)
            .ToList();

        return new FeuilleRemiseData
        {
            Agent = agent,
            SiteName = siteName,
            EquipeName = equipeName,
            Equipments = equipmentItems,
            GeneratedDate = DateTime.Now
        };
    }
}
