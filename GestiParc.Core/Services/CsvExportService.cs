using System.Text;
using GestiParc.Core.Interfaces.Services;
using GestiParc.Core.DTOs;

namespace GestiParc.Core.Services;

/// <summary>
/// Service d'export de données au format CSV (logique métier pure, sans dépendances UI)
/// </summary>
public class CsvExportService : ICsvExportService
{
    private readonly List<AgentDto> _agents;
    private readonly List<EquipmentDto> _equipments;
    private readonly List<EquipmentTypeDto> _equipmentTypes;
    private readonly List<EquipeDto> _equipes;
    private readonly List<SiteDto> _sites;

    public CsvExportService(
        List<AgentDto> agents,
        List<EquipmentDto> equipments,
        List<EquipmentTypeDto> equipmentTypes,
        List<EquipeDto> equipes,
        List<SiteDto> sites)
    {
        _agents = agents ?? throw new ArgumentNullException(nameof(agents));
        _equipments = equipments ?? throw new ArgumentNullException(nameof(equipments));
        _equipmentTypes = equipmentTypes ?? throw new ArgumentNullException(nameof(equipmentTypes));
        _equipes = equipes ?? throw new ArgumentNullException(nameof(equipes));
        _sites = sites ?? throw new ArgumentNullException(nameof(sites));
    }

    public void ExportAgents(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Le chemin du fichier ne peut pas être vide", nameof(filePath));

        var agents = _agents;
        var equipes = _equipes;
        var sites = _sites;

        var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);
        var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);

        var data = agents
            .OrderBy(a => a.Nom)
            .ThenBy(a => a.Prenom)
            .Select(a => new Dictionary<string, object>
            {
                ["IDRH"] = a.Idrh ?? "",
                ["Nom"] = a.Nom ?? "",
                ["Prénom"] = a.Prenom ?? "",
                ["Email"] = a.Email ?? "",
                ["Équipe"] = a.EquipeId.HasValue && equipeDict.ContainsKey(a.EquipeId.Value) 
                    ? equipeDict[a.EquipeId.Value] : "",
                ["Site"] = a.SiteId.HasValue && siteDict.ContainsKey(a.SiteId.Value) 
                    ? siteDict[a.SiteId.Value] : "",
                ["Hébergé"] = a.Heberge == 1 ? "Oui" : "Non",
                ["Commentaire"] = a.Commentaire ?? ""
            })
            .ToList();

        WriteCsvFromDictionary(filePath, data);
    }

    public void ExportEquipments(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Le chemin du fichier ne peut pas être vide", nameof(filePath));

        var equipments = _equipments;
        var types = _equipmentTypes;
        var agents = _agents;

        var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
        var agentDict = agents.ToDictionary(a => a.Idrh, a => $"{a.Nom} {a.Prenom}");

        var data = equipments
            .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
            .ThenBy(e => e.Nom ?? "")
            .Select(e => new Dictionary<string, object>
            {
                ["ID"] = e.IdEquipement ?? "",
                ["Type"] = typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "",
                ["Nom"] = e.Nom ?? "",
                ["Code Parc"] = e.CodeParc ?? "",
                ["Numéro de série"] = e.NumeroSerie ?? "",
                ["Marque"] = e.Marque ?? "",
                ["État"] = e.EtatPret switch
                {
                    0 => "Disponible",
                    1 => "En prêt",
                    2 => "Rendu DSEM",
                    _ => "Inconnu"
                },
                ["Agent"] = !string.IsNullOrEmpty(e.Idrh) && agentDict.ContainsKey(e.Idrh) 
                    ? agentDict[e.Idrh] : "",
                ["Commentaire"] = e.Commentaire ?? ""
            })
            .ToList();

        WriteCsvFromDictionary(filePath, data);
    }

    public void ExportLoans(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Le chemin du fichier ne peut pas être vide", nameof(filePath));

        var agents = _agents;
        var equipments = _equipments;
        var types = _equipmentTypes;
        var equipes = _equipes;

        var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
        var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);

        // Grouper les équipements par agent
        var equipmentsByAgent = equipments
            .Where(e => !string.IsNullOrEmpty(e.Idrh) && e.EtatPret == 1)
            .GroupBy(e => e.Idrh!)
            .ToDictionary(g => g.Key, g => g.ToList());

        var data = agents
            .Where(a => equipmentsByAgent.ContainsKey(a.Idrh))
            .OrderBy(a => a.Nom)
            .ThenBy(a => a.Prenom)
            .Select(a =>
            {
                var row = new Dictionary<string, object>
                {
                    ["IDRH"] = a.Idrh ?? "",
                    ["Nom"] = a.Nom ?? "",
                    ["Prénom"] = a.Prenom ?? "",
                    ["Email"] = a.Email ?? "",
                    ["Équipe"] = a.EquipeId.HasValue && equipeDict.ContainsKey(a.EquipeId.Value) 
                        ? equipeDict[a.EquipeId.Value] : ""
                };

                // Ajouter les équipements prêtés
                if (a.Idrh == null || !equipmentsByAgent.ContainsKey(a.Idrh))
                {
                    row["Nombre total"] = 0;
                    return row;
                }

                var agentEquipments = equipmentsByAgent[a.Idrh];
                for (int i = 0; i < agentEquipments.Count; i++)
                {
                    var eq = agentEquipments[i];
                    var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";
                    row[$"Équipement {i + 1}"] = $"{typeName} - {eq.CodeParc ?? eq.Nom}";
                }

                row["Nombre total"] = agentEquipments.Count;

                return row;
            })
            .ToList();

        WriteCsvFromDictionary(filePath, data);
    }

    public void ExportAll(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Le chemin du dossier ne peut pas être vide", nameof(folderPath));

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        ExportAgents(Path.Combine(folderPath, $"agents_{timestamp}.csv"));
        ExportEquipments(Path.Combine(folderPath, $"equipements_{timestamp}.csv"));
        ExportLoans(Path.Combine(folderPath, $"prets_{timestamp}.csv"));
    }

    private static void WriteCsvFromDictionary(string filePath, List<Dictionary<string, object>> data)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        
        if (data.Count == 0) return;

        // Ecrire l'en-tete
        // IMPORTANT: l'export peut produire des lignes avec des colonnes differentes (ex: ExportLoans equipement 1..N).
        // On prend l'union des cles dans l'ordre d'apparition.
        var headers = new List<string>();
        foreach (var row in data)
        {
            foreach (var key in row.Keys)
            {
                if (!headers.Contains(key))
                    headers.Add(key);
            }
        }
        writer.WriteLine(string.Join(";", headers));

        // Écrire les données
        foreach (var row in data)
        {
            var values = new List<string>();
            foreach (var header in headers)
            {
                var value = row.ContainsKey(header) ? row[header]?.ToString() ?? "" : "";
                // Échapper les guillemets et entourer de guillemets si contient ; ou "
                if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
                {
                    value = "\"" + value.Replace("\"", "\"\"") + "\"";
                }
                values.Add(value);
            }
            writer.WriteLine(string.Join(";", values));
        }
    }
}
