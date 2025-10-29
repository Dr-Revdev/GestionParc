using System;
using System.Drawing;  
using System.Windows.Forms;
using ProjetParc.Views.Admin;
using ProjetParc.Views.Agent;
using ProjetParc.Views.Equipment;
using ProjetParc.Views.Loan;
using ProjetParc.Views.Inventory;
using ProjetParc.Views.Settings;

namespace ProjetParc.Views;

/// <summary>
/// Page d'accueil principale de l'application de gestion de parc
/// Fournit l'accès aux différentes fonctionnalités via une interface graphique
/// </summary>
public class WelcomePage : Form
{
    private Panel content;
    private Button btnSetEquipment;
    private Button btnFreeEquipment;
    private Button btnNewMod;
    private Label title;

    /// <summary>
    /// Initialise la page d'accueil et prépare la navigation entre les vues.
    /// Définit la taille de la fenêtre, crée les boutons et attache les handlers.
    /// </summary>
    public WelcomePage()
    {
        Text = "Gestion Parc";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 600);
        BackColor = Theme.Colors.Background;

        // Gestionnaire de fermeture pour confirmer la sauvegarde SharePoint
        FormClosing += OnFormClosing;

        // Création du layout principal
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Colors.Background,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(Theme.Spacing.Large)
        };

        // Configuration des lignes
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // En-tête
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu

        // En-tête avec titre et sous-titre
        var headerPanel = new TableLayoutPanel 
        { 
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = Theme.Colors.Background
        };
        headerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        headerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        
        title = new Label
        {
            Text = "Gestion de Parc",
            Font = Theme.Fonts.H1,
            ForeColor = Theme.Colors.Primary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomCenter
        };
        
        var subtitle = new Label
        {
            Text = "Système de gestion des équipements et des prêts",
            Font = Theme.Fonts.BodyLarge,
            ForeColor = Theme.Colors.TextSecondary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter
        };
        
        headerPanel.Controls.Add(title, 0, 0);
        headerPanel.Controls.Add(subtitle, 0, 1);

        // Panneau de contenu
        content = new Panel 
        { 
            Dock = DockStyle.Fill, 
            BackColor = Theme.Colors.Background 
        };

        // Configuration du layout des boutons
        var buttonLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(Theme.Spacing.XLarge),
            BackColor = Theme.Colors.Background
        };

        // Configuration des colonnes pour les boutons (répartition égale)
        for (int i = 0; i < 3; i++)
        {
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

        // Configuration des boutons avec le thème
        var tileFont = new Font("Segoe UI", 14f, FontStyle.Regular);
        
        btnSetEquipment = new Button
        {
            Text = "Équipements en place\n\nConsulter les prêts actifs",
            Font = tileFont,
            Dock = DockStyle.None,
            Size = new Size(380, 280),
            Anchor = AnchorStyles.None,
            Margin = new Padding(Theme.Spacing.Medium),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        Theme.StylePrimaryButton(btnSetEquipment, setHeight: false);
        
        btnFreeEquipment = new Button
        {
            Text = "Équipements disponibles\n\nVoir le stock libre",
            Font = tileFont,
            Dock = DockStyle.None,
            Size = new Size(380, 280),
            Anchor = AnchorStyles.None,
            Margin = new Padding(Theme.Spacing.Medium),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        Theme.StylePrimaryButton(btnFreeEquipment, setHeight: false);
        
        btnNewMod = new Button
        {
            Text = "Administration\n\nGérer les données",
            Font = tileFont,
            Dock = DockStyle.None,
            Size = new Size(380, 280),
            Anchor = AnchorStyles.None,
            Margin = new Padding(Theme.Spacing.Medium),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        Theme.StyleSecondaryButton(btnNewMod, setHeight: false);

        // Ajout des boutons au layout
        buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
        buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
        buttonLayout.Controls.Add(btnNewMod, 2, 0);

        // Ajout des événements
        btnNewMod.Click += (_, __) => ShowAdminMenu();
        btnFreeEquipment.Click += (_, __) => ShowEquipmentFree();
        btnSetEquipment.Click += (_, __) => ShowMainInventoryPage();

        // Assemblage final
        content.Controls.Add(buttonLayout);
        mainLayout.Controls.Add(headerPanel, 0, 0);
        mainLayout.Controls.Add(content, 0, 1);
        Controls.Add(mainLayout);

        // Barre d'outils modernisée
        var toolStrip = new ToolStrip 
        { 
            Dock = DockStyle.Top,
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = Theme.Colors.Surface,
            Padding = new Padding(Theme.Spacing.Small)
        };
        
        var btnExportTool = new ToolStripButton
        {
            Text = "Export CSV",
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Font = Theme.Fonts.Button,
            ForeColor = Theme.Colors.Primary
        };
        btnExportTool.Click += (s, e) => ShowExportMenu();
        
        toolStrip.Items.Add(btnExportTool);

        // Bouton de sauvegarde SharePoint (visible uniquement si mode SharePoint actif)
        if (Data.Database.SyncManager.IsActive)
        {
            var btnSaveTool = new ToolStripButton
            {
                Text = "💾 Sauvegarder",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = Theme.Fonts.Button,
                ForeColor = Theme.Colors.Primary
            };
            btnSaveTool.Click += (s, e) => SaveToSharePoint();
            
            toolStrip.Items.Add(btnSaveTool);
        }

        Controls.Add(toolStrip);

        // Affiche le panneau d'accueil contenant les trois tuiles
        ShowHome();

    }

    /// <summary>
    /// Affiche l'écran d'accueil avec les tuiles de navigation principales.
    /// Réutilise les boutons créés dans le constructeur pour éviter la recréation.
    /// </summary>
    private void ShowHome()
    {
        content.Controls.Clear();

        var buttonLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(Theme.Spacing.XLarge),
            BackColor = Theme.Colors.Background
        };

        // Configuration des colonnes pour les boutons (répartition égale)
        for (int i = 0; i < 3; i++)
        {
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

        buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
        buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
        buttonLayout.Controls.Add(btnNewMod, 2, 0);

        content.Controls.Add(buttonLayout);
    }

    /// <summary>
    /// Remplace le contenu par la vue d'administration (création / modification).
    /// La vue admin est initialisée avec des callbacks pointant vers les méthodes Show* de cette classe.
    /// </summary>
    private void ShowAdminMenu()
    {
        content.Controls.Clear();

        var admin = new AdminMenuView(onBack: ShowHome, onCreateEquipment: ShowEquipmentCreate, onCreateAgent: ShowAgentCreate, onEditAgent: ShowAgentEdit, onEditEquipment: ShowEquipmentEdit, onSettings: ShowSettings);
        admin.Dock = DockStyle.Fill;
        content.Controls.Add(admin);
    }

    /// <summary>
    /// Affiche la vue des équipements disponibles.
    /// </summary>
    private void ShowEquipmentFree()
    {
        content.Controls.Clear();
        content.Controls.Add(new FreeEquipmentView(onBack: ShowHome) { Dock = DockStyle.Fill });
    }

    /// <summary>
    /// Affiche la vue principale d'inventaire (liste complète des équipements).
    /// </summary>
    private void ShowMainInventoryPage()
    {
        content.Controls.Clear();

        content.Controls.Add(new MainInventoryView(onBack: ShowHome) { Dock = DockStyle.Fill });
    }
    
    /// <summary>
    /// Affiche la vue de création d'équipement.
    /// Utilisée depuis le menu d'administration.
    /// </summary>
    private void ShowEquipmentCreate()
    {
        content.Controls.Clear();
        content.Controls.Add(new EquipmentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }

    /// <summary>
    /// Affiche la vue de création d'agent.
    /// </summary>
    private void ShowAgentCreate()
    {
        content.Controls.Clear();
        content.Controls.Add(new AgentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
    /// <summary>
    /// Affiche la vue d'édition d'agent.
    /// </summary>
    private void ShowAgentEdit()
    {
        content.Controls.Clear();
        content.Controls.Add(new AgentEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
    /// <summary>
    /// Affiche la vue d'édition d'équipement.
    /// </summary>
    private void ShowEquipmentEdit()
    {
        content.Controls.Clear();
        content.Controls.Add(new EquipementEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }

    /// <summary>
    /// Affiche la vue des paramètres (Équipes, Sites, Types d'équipement).
    /// </summary>
    private void ShowSettings()
    {
        content.Controls.Clear();
        content.Controls.Add(new SettingsView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }

    private void ShowExportMenu()
    {
        var exportForm = new Form
        {
            Text = "Export CSV",
            Size = new Size(500, 450),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.Sizable,
            MaximizeBox = true,
            MinimizeBox = true,
            MinimumSize = new Size(450, 400),
            BackColor = Theme.Colors.Background
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(Theme.Spacing.Large),
            BackColor = Theme.Colors.Background
        };

        for (int i = 0; i < 6; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "Sélectionnez le type d'export :",
            Font = Theme.Fonts.H5,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(title, 0, 0);

        var btnExpAgents = new Button
        {
            Text = "Exporter les Agents",
            Dock = DockStyle.Fill,
            Height = Theme.Sizes.ButtonHeightLarge,
            Margin = new Padding(0, Theme.Spacing.Small, 0, Theme.Spacing.Small)
        };
        Theme.StylePrimaryButton(btnExpAgents);
        btnExpAgents.Click += (s, e) =>
        {
            var path = Data.CsvExporter.SelectExportFile("agents.csv");
            if (path != null) Data.CsvExporter.ExportAgents(path);
        };
        layout.Controls.Add(btnExpAgents, 0, 1);

        var btnExpEquip = new Button
        {
            Text = "Exporter les Équipements",
            Dock = DockStyle.Fill,
            Height = Theme.Sizes.ButtonHeightLarge,
            Margin = new Padding(0, Theme.Spacing.Small, 0, Theme.Spacing.Small)
        };
        Theme.StylePrimaryButton(btnExpEquip);
        btnExpEquip.Click += (s, e) =>
        {
            var path = Data.CsvExporter.SelectExportFile("equipements.csv");
            if (path != null) Data.CsvExporter.ExportEquipements(path);
        };
        layout.Controls.Add(btnExpEquip, 0, 2);

        var btnExpPrets = new Button
        {
            Text = "Exporter les Prêts actifs",
            Dock = DockStyle.Fill,
            Height = Theme.Sizes.ButtonHeightLarge,
            Margin = new Padding(0, Theme.Spacing.Small, 0, Theme.Spacing.Small)
        };
        Theme.StylePrimaryButton(btnExpPrets);
        btnExpPrets.Click += (s, e) =>
        {
            var path = Data.CsvExporter.SelectExportFile("prets_actifs.csv");
            if (path != null) Data.CsvExporter.ExportPrets(path);
        };
        layout.Controls.Add(btnExpPrets, 0, 3);

        var separator = new Label
        {
            Text = "ou",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Colors.TextSecondary,
            Font = Theme.Fonts.Body
        };
        layout.Controls.Add(separator, 0, 4);

        var btnExpComplet = new Button
        {
            Text = "Export complet (tous les fichiers)",
            Dock = DockStyle.Fill,
            Height = Theme.Sizes.ButtonHeightLarge,
            Margin = new Padding(0, Theme.Spacing.Small, 0, Theme.Spacing.Small)
        };
        Theme.StyleSecondaryButton(btnExpComplet);
        btnExpComplet.Click += (s, e) =>
        {
            var folder = Data.CsvExporter.SelectExportFolder();
            if (folder != null)
            {
                Data.CsvExporter.ExportComplet(folder);
                exportForm.Close();
            }
        };
        layout.Controls.Add(btnExpComplet, 0, 5);

        exportForm.Controls.Add(layout);
        exportForm.ShowDialog();
    }

    /// <summary>
    /// Sauvegarde manuelle de la base de données vers SharePoint
    /// </summary>
    private void SaveToSharePoint()
    {
        if (!Data.Database.SyncManager.IsActive)
        {
            MessageBox.Show(
                "Le mode SharePoint n'est pas actif.",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return;
        }

        try
        {
            Data.Database.SyncManager.CopyToSharePoint();
            MessageBox.Show(
                "Base de données sauvegardée avec succès sur SharePoint.",
                "Sauvegarde réussie",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Data.SharePointSyncException ex)
        {
            MessageBox.Show(
                $"Erreur lors de la sauvegarde vers SharePoint :\n\n{ex.Message}\n\nVos modifications locales sont conservées.",
                "Erreur de sauvegarde",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    /// <summary>
    /// Gestionnaire de l'événement FormClosing pour confirmer la sauvegarde SharePoint
    /// </summary>
    private void OnFormClosing(object sender, FormClosingEventArgs e)
    {
        // Uniquement si SharePoint est actif
        if (!Data.Database.SyncManager.IsActive)
            return;

        var result = MessageBox.Show(
            "Voulez-vous sauvegarder les modifications vers SharePoint avant de quitter ?",
            "Sauvegarder avant de quitter",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Cancel)
        {
            // Annuler la fermeture
            e.Cancel = true;
            return;
        }

        if (result == DialogResult.Yes)
        {
            try
            {
                // Sauvegarder vers SharePoint
                Data.Database.SyncManager.CopyToSharePoint();
            }
            catch (Data.SharePointSyncException ex)
            {
                var retry = MessageBox.Show(
                    $"Erreur lors de la sauvegarde :\n\n{ex.Message}\n\nVoulez-vous quitter quand même sans sauvegarder ?",
                    "Erreur de sauvegarde",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );

                if (retry == DialogResult.No)
                {
                    // Annuler la fermeture pour réessayer
                    e.Cancel = true;
                    return;
                }
            }
        }

        // Si on arrive ici (Yes avec succès ou No), on peut fermer
        // Le ApplicationExit fera le cleanup (suppression du lock)
    }
}
