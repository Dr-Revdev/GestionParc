using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ProjetParc.Data;

/// <summary>
/// Classe utilitaire pour exporter les données en fichiers CSV
/// </summary>
public static class CsvExporter
{
    /// <summary>
    /// Exporte tous les agents vers un fichier CSV
    /// </summary>
    public static void ExportAgents(string filePath)
    {
        try
        {
            using var connection = Database.Open();
            using var command = connection.CreateCommand();
            
            command.CommandText = @"
                SELECT 
                    a.idrh as 'IDRH',
                    a.nom as 'Nom',
                    a.prenom as 'Prénom',
                    a.email as 'Email',
                    e.name as 'Équipe',
                    s.name as 'Site',
                    CASE WHEN a.heberge = 1 THEN 'Oui' ELSE 'Non' END as 'Hébergé',
                    a.commentaire as 'Commentaire'
                FROM Agents a
                LEFT JOIN Equipes e ON a.equipe_id = e.id
                LEFT JOIN Sites s ON a.site_id = s.id
                ORDER BY a.nom, a.prenom";

            using var reader = command.ExecuteReader();
            WriteCsv(filePath, reader);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Agents", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export des agents :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Exporte tous les équipements vers un fichier CSV
    /// </summary>
    public static void ExportEquipements(string filePath)
    {
        try
        {
            using var connection = Database.Open();
            using var command = connection.CreateCommand();
            
            command.CommandText = @"
                SELECT 
                    e.id_equipement as 'ID',
                    t.name as 'Type',
                    e.nom as 'Nom',
                    e.code_parc as 'Code Parc',
                    e.numero_serie as 'Numéro de série',
                    e.marque as 'Marque',
                    CASE 
                        WHEN e.etat_pret = 0 THEN 'Disponible'
                        WHEN e.etat_pret = 1 THEN 'En prêt'
                        WHEN e.etat_pret = 2 THEN 'Rendu DSEM'
                        ELSE 'Inconnu'
                    END as 'État',
                    COALESCE(a.nom || ' ' || a.prenom, '') as 'Agent',
                    e.commentaire as 'Commentaire'
                FROM Equipements e
                JOIN equipment_type t ON t.id = e.type_id
                LEFT JOIN Agents a ON a.idrh = e.idrh
                ORDER BY t.name, e.nom";

            using var reader = command.ExecuteReader();
            WriteCsv(filePath, reader);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Équipements", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export des équipements :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Exporte tous les prêts actifs vers un fichier CSV (une ligne par agent, équipements en colonnes)
    /// </summary>
    public static void ExportPrets(string filePath)
    {
        try
        {
            using var connection = Database.Open();
            
            // Récupérer le nombre max d'équipements par agent
            using var cmdMax = connection.CreateCommand();
            cmdMax.CommandText = @"
                SELECT MAX(cnt) FROM (
                    SELECT COUNT(*) as cnt 
                    FROM Equipements 
                    WHERE etat_pret IN (1, 2) AND idrh IS NOT NULL 
                    GROUP BY idrh
                )";
            var maxEquipments = Convert.ToInt32(cmdMax.ExecuteScalar() ?? 0);

            // Récupérer les agents qui ont des équipements
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT
                    a.idrh,
                    a.nom,
                    a.prenom,
                    a.email,
                    e.name as equipe,
                    s.name as site,
                    a.heberge,
                    a.commentaire
                FROM Agents a
                LEFT JOIN Equipes e ON a.equipe_id = e.id
                LEFT JOIN Sites s ON a.site_id = s.id
                WHERE EXISTS (
                    SELECT 1 FROM Equipements eq 
                    WHERE eq.idrh = a.idrh AND eq.etat_pret IN (1, 2)
                )
                ORDER BY a.nom, a.prenom";

            var agents = new List<Dictionary<string, object>>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var agent = new Dictionary<string, object>
                    {
                        ["IDRH Agent"] = reader["idrh"],
                        ["Nom Agent"] = reader["nom"],
                        ["Prénom Agent"] = reader["prenom"],
                        ["Email Agent"] = reader.IsDBNull(3) ? "" : reader["email"],
                        ["Équipe"] = reader.IsDBNull(4) ? "" : reader["equipe"],
                        ["Site"] = reader.IsDBNull(5) ? "" : reader["site"],
                        ["Hébergé"] = reader.GetInt32(6) == 1 ? "Oui" : "Non"
                    };
                    agents.Add(agent);
                }
            }

            // Pour chaque agent, récupérer ses équipements
            foreach (var agent in agents)
            {
                using var cmdEq = connection.CreateCommand();
                cmdEq.CommandText = @"
                    SELECT 
                        t.name as type,
                        eq.nom,
                        eq.code_parc,
                        eq.numero_serie,
                        eq.marque,
                        eq.commentaire
                    FROM Equipements eq
                    JOIN equipment_type t ON t.id = eq.type_id
                    WHERE eq.idrh = $idrh AND eq.etat_pret IN (1, 2)
                    ORDER BY t.name, eq.nom";
                cmdEq.Parameters.AddWithValue("$idrh", agent["IDRH Agent"]);

                int equipIndex = 1;
                using var eqReader = cmdEq.ExecuteReader();
                while (eqReader.Read())
                {
                    agent[$"Type Équipement {equipIndex}"] = eqReader.IsDBNull(0) ? "" : eqReader.GetString(0);
                    agent[$"Nom Équipement {equipIndex}"] = eqReader.IsDBNull(1) ? "" : eqReader.GetString(1);
                    agent[$"Code Parc {equipIndex}"] = eqReader.IsDBNull(2) ? "" : eqReader.GetString(2);
                    agent[$"Numéro de série {equipIndex}"] = eqReader.IsDBNull(3) ? "" : eqReader.GetString(3);
                    agent[$"Marque {equipIndex}"] = eqReader.IsDBNull(4) ? "" : eqReader.GetString(4);
                    agent[$"Commentaire Équipement {equipIndex}"] = eqReader.IsDBNull(5) ? "" : eqReader.GetString(5);
                    equipIndex++;
                }
            }

            // Écrire le CSV
            WriteDynamicCsvForLoans(filePath, agents, maxEquipments);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Prêts", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export des prêts :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Écrit un CSV avec colonnes dynamiques pour les prêts
    /// </summary>
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

    

    /// <summary>
    /// Exporte tout en plusieurs fichiers dans un même dossier
    /// </summary>
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
    /// Écrit les données d'un SqliteDataReader dans un fichier CSV
    /// </summary>
    private static void WriteCsv(string filePath, SqliteDataReader reader)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        
        // Écrire l'en-tête
        var columnNames = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columnNames.Add(reader.GetName(i));
        }
        writer.WriteLine(string.Join(";", columnNames));

        // Écrire les données
        while (reader.Read())
        {
            var values = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();
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
