using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProjetParc.Data;

// Helper pour générer des fichiers CSV - agents, équipements, prêts ou export complet
public static class CsvExporter
{
    // Exporte tous les agents dans un CSV (nom, prénom, équipe, site...)
    public static void ExportAgents(string filePath)
    {
        try
        {
            var agentRepo = new Repositories.MySQL.AgentMySqlRepository();
            var equipeRepo = new Repositories.MySQL.EquipeMySqlRepository();
            var siteRepo = new Repositories.MySQL.SiteMySqlRepository();

            var agents = agentRepo.GetAll();
            var equipes = equipeRepo.GetAll();
            var sites = siteRepo.GetAll();

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
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Agents", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export des agents :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Exporte tous les équipements dans un CSV (code parc, type, agent affecté...)
    public static void ExportEquipements(string filePath)
    {
        try
        {
            var equipmentRepo = new Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Repositories.MySQL.EquipmentTypeMySqlRepository();
            var agentRepo = new Repositories.MySQL.AgentMySqlRepository();

            var equipments = equipmentRepo.GetAll();
            var types = typeRepo.GetAll();
            var agents = agentRepo.GetAll();

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
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Équipements", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export des équipements :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Exporte les prêts actifs - une ligne par agent, chaque équipement prend une colonne
    public static void ExportPrets(string filePath)
    {
        try
        {
            var agentRepo = new Repositories.MySQL.AgentMySqlRepository();
            var equipmentRepo = new Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Repositories.MySQL.EquipmentTypeMySqlRepository();
            var equipeRepo = new Repositories.MySQL.EquipeMySqlRepository();
            var siteRepo = new Repositories.MySQL.SiteMySqlRepository();

            var allAgents = agentRepo.GetAll();
            var allEquipments = equipmentRepo.GetAll().Where(e => e.EtatPret == 1 || e.EtatPret == 2).ToList();
            var types = typeRepo.GetAll();
            var equipes = equipeRepo.GetAll();
            var sites = siteRepo.GetAll();

            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);
            var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);

            // Grouper les équipements par agent
            var equipmentsByAgent = allEquipments
                .Where(e => !string.IsNullOrEmpty(e.Idrh))
                .GroupBy(e => e.Idrh)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
                                                 .ThenBy(e => e.Nom ?? "")
                                                 .ToList());

            // Calculer le nombre max d'équipements par agent
            var maxEquipments = equipmentsByAgent.Any() ? equipmentsByAgent.Max(kvp => kvp.Value.Count) : 0;

            // Filtrer les agents qui ont des équipements
            var agentsWithEquipments = allAgents
                .Where(a => equipmentsByAgent.ContainsKey(a.Idrh))
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .ToList();

            var agentsData = new List<Dictionary<string, object>>();

            foreach (var agent in agentsWithEquipments)
            {
                var agentData = new Dictionary<string, object>
                {
                    ["IDRH Agent"] = agent.Idrh ?? "",
                    ["Nom Agent"] = agent.Nom ?? "",
                    ["Prénom Agent"] = agent.Prenom ?? "",
                    ["Email Agent"] = agent.Email ?? "",
                    ["Équipe"] = agent.EquipeId.HasValue && equipeDict.ContainsKey(agent.EquipeId.Value) 
                        ? equipeDict[agent.EquipeId.Value] : "",
                    ["Site"] = agent.SiteId.HasValue && siteDict.ContainsKey(agent.SiteId.Value) 
                        ? siteDict[agent.SiteId.Value] : "",
                    ["Hébergé"] = agent.Heberge == 1 ? "Oui" : "Non"
                };

                // Ajouter les équipements de l'agent
                var agentEquipments = equipmentsByAgent[agent.Idrh];
                int equipIndex = 1;
                foreach (var eq in agentEquipments)
                {
                    var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "";
                    agentData[$"Type Équipement {equipIndex}"] = typeName;
                    agentData[$"Nom Équipement {equipIndex}"] = eq.Nom ?? "";
                    agentData[$"Code Parc {equipIndex}"] = eq.CodeParc ?? "";
                    agentData[$"Numéro de série {equipIndex}"] = eq.NumeroSerie ?? "";
                    agentData[$"Marque {equipIndex}"] = eq.Marque ?? "";
                    agentData[$"Commentaire Équipement {equipIndex}"] = eq.Commentaire ?? "";
                    equipIndex++;
                }

                agentsData.Add(agentData);
            }

            // Écrire le CSV
            WriteDynamicCsvForLoans(filePath, agentsData, maxEquipments);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Prêts", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export des prêts :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Méthode interne pour écrire un CSV avec colonnes dynamiques (nombre d'équipements variable selon les agents)
    private static void WriteDynamicCsvForLoans(string filePath, List<Dictionary<string, object>> data, int maxEquipments)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        
        if (data.Count == 0) return;

        // Construire l'en-tête
        var headers = new List<string> 
        { 
            "IDRH Agent", "Nom Agent", "Prénom Agent", "Email Agent", 
            "Équipe", "Site", "Hébergé"
        };

        for (int i = 1; i <= maxEquipments; i++)
        {
            headers.Add($"Type Équipement {i}");
            headers.Add($"Nom Équipement {i}");
            headers.Add($"Code Parc {i}");
            headers.Add($"Numéro de série {i}");
            headers.Add($"Marque {i}");
            headers.Add($"Commentaire Équipement {i}");
        }

        writer.WriteLine(string.Join(";", headers));

        // Écrire les données
        foreach (var row in data)
        {
            var values = new List<string>();
            foreach (var header in headers)
            {
                var value = row.ContainsKey(header) ? row[header]?.ToString() ?? "" : "";
                if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
                {
                    value = "\"" + value.Replace("\"", "\"\"") + "\"";
                }
                values.Add(value);
            }
            writer.WriteLine(string.Join(";", values));
        }
    }

    

    // Exporte tout en un seul coup - crée un dossier avec 3 CSV (agents, équipements, prêts)
    public static void ExportComplet(string folderPath)
    {
        try
        {
            // Créer un dossier avec timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var exportFolder = Path.Combine(folderPath, $"Export_GestionParc_{timestamp}");
            Directory.CreateDirectory(exportFolder);

            // Export des différentes sections
            ExportAgents(Path.Combine(exportFolder, "Agents.csv"));
            ExportEquipements(Path.Combine(exportFolder, "Equipements.csv"));
            ExportPrets(Path.Combine(exportFolder, "Prets_Actifs.csv"));

            // Créer un fichier README explicatif
            var readmePath = Path.Combine(exportFolder, "README.txt");
            File.WriteAllText(readmePath, 
                $"Export GestionParc - {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                "================================================\n\n" +
                "Agents.csv : Liste complète des agents\n" +
                "Equipements.csv : Liste complète des équipements\n" +
                "Prets_Actifs.csv : Prêts en cours et équipements rendus DSEM\n\n" +
                "Encodage : UTF-8 avec BOM\n" +
                "Séparateur : point-virgule (;)\n");

            MessageBox.Show($"Export complet réussi !\n\nDossier : {exportFolder}\n\n" +
                "3 fichiers CSV créés :\n" +
                "- Agents.csv\n" +
                "- Equipements.csv\n" +
                "- Prets_Actifs.csv", 
                "Export Complet", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Ouvrir le dossier dans l'explorateur
            System.Diagnostics.Process.Start("explorer.exe", exportFolder);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export complet :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Écrit les données d'une liste de dictionnaires dans un fichier CSV
    /// </summary>
    private static void WriteCsvFromDictionary(string filePath, List<Dictionary<string, object>> data)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        
        if (data.Count == 0) return;

        // Écrire l'en-tête
        var headers = data[0].Keys.ToList();
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

    /// <summary>
    /// Affiche un dialogue de sélection de fichier pour l'export
    /// </summary>
    public static string SelectExportFile(string defaultName = "export.csv")
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
            FileName = defaultName,
            DefaultExt = "csv",
            AddExtension = true,
            Title = "Enregistrer l'export CSV"
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }

    /// <summary>
    /// Affiche un dialogue de sélection de dossier pour l'export complet
    /// </summary>
    public static string SelectExportFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Sélectionnez le dossier où créer l'export",
            ShowNewFolderButton = true
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
