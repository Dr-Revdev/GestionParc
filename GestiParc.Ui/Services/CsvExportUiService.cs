using System.Net.Http;
using GestiParc.Core.Interfaces.Services;
using GestiParc.Core.Services;
using GestiParc.Ui.Services.Api;

namespace GestiParc.Ui.Services;

/// <summary>
/// Wrapper UI pour le service d'export CSV - gère les dialogues et notifications utilisateur
/// </summary>
public static class CsvExportUiService
{
    private static readonly AgentApiClient _agentApiClient = new AgentApiClient();
    private static readonly EquipmentApiClient _equipmentApiClient = new EquipmentApiClient();
    private static readonly EquipmentTypeApiClient _equipmentTypeApiClient = new EquipmentTypeApiClient();
    private static readonly EquipeApiClient _equipeApiClient = new EquipeApiClient();
    private static readonly SiteApiClient _siteApiClient = new SiteApiClient();

    private static async Task<ICsvExportService> CreateServiceAsync()
    {
        var agents = await _agentApiClient.GetAllAsync();
        var equipments = await _equipmentApiClient.GetAllAsync();
        var types = await _equipmentTypeApiClient.GetAllAsync();
        var equipes = await _equipeApiClient.GetAllAsync();
        var sites = await _siteApiClient.GetAllAsync();

        return new CsvExportService(agents, equipments, types, equipes, sites);
    }

    /// <summary>
    /// Exporte tous les agents avec dialogue de sélection et notification
    /// </summary>
    public static async Task ExportAgentsAsync()
    {
        var filePath = SelectExportFile("agents.csv");
        if (filePath == null) return;

        try
        {
            var service = await CreateServiceAsync();
            service.ExportAgents(filePath);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Agents", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les données.\n\n{ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    public static async Task ExportEquipmentsAsync()
    {
        var filePath = SelectExportFile("equipments.csv");
        if (filePath == null) return;

        try
        {
            var service = await CreateServiceAsync();
            service.ExportEquipments(filePath);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Équipements",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les données.\n\n{ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    public static async Task ExportLoansAsync()
    {
        var filePath = SelectExportFile("prets.csv");
        if (filePath == null) return;

        try
        {
            var service = await CreateServiceAsync();
            service.ExportLoans(filePath);
            
            MessageBox.Show($"Export réussi !\n{filePath}", "Export Prêts",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les données.\n\n{ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    public static async Task ExportAllAsync()
    {
        var folder = SelectExportFolder();
        if (folder == null) return;

        try
        {
            var service = await CreateServiceAsync();
            service.ExportAll(folder);
            
            MessageBox.Show($"Export complet réussi !\nFichiers créés dans :\n{folder}", 
                "Export Complet", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les données.\n\n{ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
