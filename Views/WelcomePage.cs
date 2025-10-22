using System;
using System.Drawing;  
using System.Windows.Forms;
using ProjetParc.Views.Admin;
using ProjetParc.Views.Agent;
using ProjetParc.Views.Equipment;
using ProjetParc.Views.Loan;
using ProjetParc.Views.Inventory;
using System.Windows.Forms.Integration;

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

        // Création du layout principal
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(20)
        };

        // Configuration des lignes
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // En-tête
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu

        // En-tête avec titre
        var headerPanel = new TableLayoutPanel 
        { 
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 1
        };
        
        title = new Label
        {
            Text = "Gestion de Parc",
            Font = new Font("Segoe UI", 28f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        headerPanel.Controls.Add(title, 0, 0);

        // Panneau de contenu
        content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // Configuration du layout des boutons
        var buttonLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(10)
        };

        // Configuration des colonnes pour les boutons (répartition égale)
        for (int i = 0; i < 3; i++)
        {
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

        // Configuration des boutons
        var tileFont = new Font("Segoe UI", 16f, FontStyle.Bold);
        btnSetEquipment = new Button
        {
            Text = "Equipements en place",
            Font = tileFont,
            Dock = DockStyle.Fill,
            Margin = new Padding(10)
        };
        btnFreeEquipment = new Button
        {
            Text = "Equipements disponibles",
            Font = tileFont,
            Dock = DockStyle.Fill,
            Margin = new Padding(10)
        };
        btnNewMod = new Button
        {
            Text = "Modification / Création",
            Font = tileFont,
            Dock = DockStyle.Fill,
            Margin = new Padding(10)
        };

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
            Padding = new Padding(50)
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

        var admin = new AdminMenuView(onBack: ShowHome, onCreateEquipment: ShowEquipmentCreate, onCreateAgent: ShowAgentCreate, onEditAgent: ShowAgentEdit, onEditEquipment: ShowEquipmentEdit);
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
}
