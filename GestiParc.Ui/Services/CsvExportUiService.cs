using System;
using System.Windows.Forms;
using GestiParc.Core.Interfaces.Services;
using GestiParc.Core.Services;
using GestiParc.Infrastructure.Data.Repositories;

namespace GestiParc.Ui.Services;

/// <summary>
/// Wrapper UI pour le service d'export CSV - gère les dialogues et notifications utilisateur
/// </summary>
public static class CsvExportUiService
{
    private static ICsvExportService CreateService()
    {
        return new CsvExportService(
            new AgentMySqlRepository(),
            new EquipmentMySqlRepository(),
            new EquipmentTypeMySqlRepository(),
            new EquipeMySqlRepository(),
            new SiteMySqlRepository()
        );
    }

    /// <summary>
    /// Exporte tous les agents avec dialogue de sélection et notification
    /// </summary>
    public static void ExportAgents()
    {
        var filePath = SelectExportFile("agents.csv");
        if (filePath == null) return;

        try
        {
            var service = CreateService();
            service.ExportAgents(filePath);
            
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
    /// Exporte tous les équipements avec dialogue de sélection et notification
    /// </summary>
    public static void ExportEquipments()
    {
        var filePath = SelectExportFile("equipments.csv");
        if (filePath == null) return;

        try
        {
            var service = CreateService();
            service.ExportEquipments(filePath);
            
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
    /// Exporte tous les prêts actifs avec dialogue de sélection et notification
    /// </summary>
    public static void ExportLoans()
    {
        var filePath = SelectExportFile("prets.csv");
        if (filePath == null) return;

        try
        {
            var service = CreateService();
            service.ExportLoans(filePath);
            
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
    /// Exporte toutes les données (agents, équipements, prêts) dans un dossier
    /// </summary>
    public static void ExportAll()
    {
        var folder = SelectExportFolder();
        if (folder == null) return;

        try
        {
            var service = CreateService();
            service.ExportAll(folder);
            
            MessageBox.Show($"Export complet réussi !\nFichiers créés dans :\n{folder}", 
                "Export Complet", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'export complet :\n{ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Affiche un dialogue de sélection de fichier pour l'export CSV
    /// </summary>
    private static string? SelectExportFile(string defaultName = "export.csv")
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
    private static string? SelectExportFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Sélectionnez le dossier où créer l'export",
            ShowNewFolderButton = true
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
