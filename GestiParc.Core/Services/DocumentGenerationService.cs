using GestiParc.Core.DTOs;
using GestiParc.Core.Interfaces.Repositories;
using GestiParc.Core.Interfaces.Services;

namespace GestiParc.Core.Services;

/// <summary>
/// Service pour préparer les données de génération de documents (logique métier pure)
/// </summary>
public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly IAgentRepository _agentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentTypeRepository _typeRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IEquipeRepository _equipeRepository;

    public DocumentGenerationService(
        IAgentRepository agentRepository,
        IEquipmentRepository equipmentRepository,
        IEquipmentTypeRepository typeRepository,
        ISiteRepository siteRepository,
        IEquipeRepository equipeRepository)
    {
        _agentRepository = agentRepository;
        _equipmentRepository = equipmentRepository;
        _typeRepository = typeRepository;
        _siteRepository = siteRepository;
        _equipeRepository = equipeRepository;
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
        var agent = _agentRepository.GetById(agentId);
        if (agent == null)
        {
            throw new InvalidOperationException($"Agent non trouvé : {agentId}");
        }

        // Récupérer les équipements en prêt (EtatPret == 1)
        var equipments = _equipmentRepository.GetByAgent(agentId)
            .Where(e => e.EtatPret == 1)
            .ToList();

        // Récupérer les données de référence
        var types = _typeRepository.GetAll();
        var sites = _siteRepository.GetAll();
        var equipes = _equipeRepository.GetAll();

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
