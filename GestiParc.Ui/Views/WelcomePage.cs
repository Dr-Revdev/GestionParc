using System;
using System.Drawing;  
using System.Windows.Forms;
using GestiParc.Ui.Views.Admin;
using GestiParc.Ui.Views.Agent;
using GestiParc.Ui.Views.Equipment;
using GestiParc.Ui.Views.Loan;
using GestiParc.Ui.Views.Inventory;
using GestiParc.Ui.Views.Settings;
using GestiParc.Ui.Services;

namespace GestiParc.Ui.Views;

/// <summary>
/// Page d'accueil de l'application - permet d'accéder aux différentes sections
/// (inventaire, équipements dispo, admin)
/// </summary>
public class WelcomePage : Form
{
    private Panel content = null!;
    private Button btnSetEquipment = null!;
    private Button btnFreeEquipment = null!;
    private Button btnNewMod = null!;
    private Label title = null!;

    public WelcomePage()
    {
        Text = "GestiParc";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 600);
        BackColor = Theme.Colors.Background;
        
        // Icône de l'application
        try
        {
            Icon = new Icon("GestionParc.ico");
        }
        catch { /* Icône non trouvée, utiliser l'icône par défaut */ }

        FormClosing += OnFormClosing;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Colors.Background,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(Theme.Spacing.Large)
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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
            Text = "GestiParc",
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

        content = new Panel 
        { 
            Dock = DockStyle.Fill, 
            BackColor = Theme.Colors.Background 
        };

        var buttonLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(Theme.Spacing.XLarge),
            BackColor = Theme.Colors.Background
        };

        for (int i = 0; i < 3; i++)
        {
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

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

        buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
        buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
        buttonLayout.Controls.Add(btnNewMod, 2, 0);

        btnNewMod.Click += (_, __) => ShowAdminMenu();
        btnFreeEquipment.Click += (_, __) => ShowEquipmentFree();
        btnSetEquipment.Click += (_, __) => ShowMainInventoryPage();

        content.Controls.Add(buttonLayout);
        mainLayout.Controls.Add(headerPanel, 0, 0);
        mainLayout.Controls.Add(content, 0, 1);
        Controls.Add(mainLayout);

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

        Controls.Add(toolStrip);

        ShowHome();
    }

    /// <summary>
    /// Retour à l'écran d'accueil - affiche les 3 gros boutons principaux
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

        for (int i = 0; i < 3; i++)
        {
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

        buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
        buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
        buttonLayout.Controls.Add(btnNewMod, 2, 0);

        content.Controls.Add(buttonLayout);
    }

    private void ShowAdminMenu()
    {
        content.Controls.Clear();

        var admin = new AdminMenuView(onBack: ShowHome, onCreateEquipment: ShowEquipmentCreate, onCreateAgent: ShowAgentCreate, onEditAgent: ShowAgentEdit, onEditEquipment: ShowEquipmentEdit, onSettings: ShowSettings);
        admin.Dock = DockStyle.Fill;
        content.Controls.Add(admin);
    }

    private void ShowEquipmentFree()
    {
        content.Controls.Clear();
        content.Controls.Add(new FreeEquipmentView(onBack: ShowHome) { Dock = DockStyle.Fill });
    }

    private void ShowMainInventoryPage()
    {
        content.Controls.Clear();

        content.Controls.Add(new MainInventoryView(onBack: ShowHome) { Dock = DockStyle.Fill });
    }
    
    private void ShowEquipmentCreate()
    {
        content.Controls.Clear();
        content.Controls.Add(new EquipmentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }

    private void ShowAgentCreate()
    {
        content.Controls.Clear();
        content.Controls.Add(new AgentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
    
    private void ShowAgentEdit()
    {
        content.Controls.Clear();
        content.Controls.Add(new AgentEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
    
    private void ShowEquipmentEdit()
    {
        content.Controls.Clear();
        content.Controls.Add(new EquipementEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }

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
            CsvExportUiService.ExportAgents();
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
            CsvExportUiService.ExportEquipments();
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
            CsvExportUiService.ExportLoans();
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
            CsvExportUiService.ExportAll();
            exportForm.Close();
        };
        layout.Controls.Add(btnExpComplet, 0, 5);

        exportForm.Controls.Add(layout);
        exportForm.ShowDialog();
    }

    /// <summary>
    /// Demande confirmation avant de fermer l'appli
    /// </summary>
    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        var result = MessageBox.Show(
            "Voulez-vous vraiment quitter l'application ?",
            "Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.No)
        {
            e.Cancel = true;
        }
    }
}
