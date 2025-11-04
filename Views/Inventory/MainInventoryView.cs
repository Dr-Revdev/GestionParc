using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using ProjetParc.Data;
using ProjetParc.Views.Loan;

namespace ProjetParc.Views.Inventory;

/// <summary>
/// Vue principale de l'inventaire permettant de gérer et visualiser tous les équipements et les prêts
/// </summary>
public class MainInventoryView : UserControl
{
    private readonly Action _onBack;
    private Button btnNewLoan;
    private ListView lvEquipments;
    private ListViewColumnSorter lvEquipmentsSorter;
    private Label lblTitle;
    private TabControl detailsTabControl;

    /// <summary>
    /// Initialise une nouvelle instance de la vue d'inventaire principal
    /// </summary>
    /// <param name="onBack">Action à exécuter lors du retour à la vue précédente</param>
    public MainInventoryView(Action onBack)
    {
        _onBack = onBack;
        InitializeComponent();
        LoadEquipments();
        LoadLoans();
    }

    /// <summary>
    /// Initialise les composants de l'interface utilisateur
    /// </summary>
    private void InitializeComponent()
    {
        try
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            Font = Theme.Fonts.Body;
            BackColor = Theme.Colors.Background;
            Padding = new Padding(Theme.Spacing.Large);

            // TableLayoutPanel principal pour organiser la vue
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Padding = new Padding(0),
                BackColor = Theme.Colors.Background
            };

            // Configuration des lignes
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // En-tête
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Liste des équipements
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Détails (TabControl)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10)); // Espacement
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Boutons en bas

            // En-tête avec bouton retour et titre
            var headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, Theme.Spacing.Medium),
                BackColor = Theme.Colors.Background
            };

            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Espace futur

            var btnBack = new Button { 
                Text = "← Retour", 
                Height = Theme.Sizes.ButtonHeightLarge,
                Dock = DockStyle.Left,
                Width = Theme.Sizes.ButtonWidth,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Font = Theme.Fonts.Button
            };
            Theme.StyleOutlineButton(btnBack);
            btnBack.Click += (_, __) => _onBack?.Invoke();

            lblTitle = new Label 
            { 
                Text = "Équipements en place",
                Font = Theme.Fonts.H3,
                ForeColor = Theme.Colors.Primary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(Theme.Spacing.Medium, 0, 0, 0)
            };

            headerPanel.Controls.Add(btnBack, 0, 0);
            headerPanel.Controls.Add(lblTitle, 1, 0);

            // Liste des équipements
            lvEquipments = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Theme.Colors.Surface,
                ForeColor = Theme.Colors.TextPrimary,
                Font = Theme.Fonts.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            lvEquipments.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Agent", Width = 200 },
                new ColumnHeader { Text = "Équipements", Width = 800 }
            });
            
            // Configuration du tri par colonnes
            lvEquipmentsSorter = new ListViewColumnSorter();
            lvEquipments.ListViewItemSorter = lvEquipmentsSorter;
            lvEquipments.ColumnClick += (s, e) => {
                lvEquipmentsSorter.SetSortColumn(e.Column);
                lvEquipments.Sort();
            };
            
            lvEquipments.SelectedIndexChanged += LvEquipments_SelectedIndexChanged;

            // Menu contextuel
            var contextMenu = new ContextMenuStrip();
            var menuItemFeuilleRemise = new ToolStripMenuItem("Générer feuille de remise");
            menuItemFeuilleRemise.Click += OnContextMenu_FeuilleRemise;
            contextMenu.Items.Add(menuItemFeuilleRemise);
            lvEquipments.ContextMenuStrip = contextMenu;

            // TabControl pour les détails
            detailsTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = Theme.Fonts.Body
            };
            
            // Onglet par défaut (vide au démarrage)
            var defaultTab = new TabPage("Sélectionnez une ligne")
            {
                BackColor = Theme.Colors.Surface
            };
            var defaultLabel = new Label
            {
                Text = "Sélectionnez une ligne dans le tableau ci-dessus pour voir les détails",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Theme.Colors.TextSecondary,
                Font = Theme.Fonts.BodyLarge
            };
            defaultTab.Controls.Add(defaultLabel);
            detailsTabControl.TabPages.Add(defaultTab);

            // Panel des boutons en bas
            var bottomButtonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Theme.Colors.Background
            };

            bottomButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Espace
            bottomButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // Bouton diagnostic
            bottomButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // Bouton nouveau prêt

            var btnDiag = new Button
            {
                Text = "Diagnostic",
                Height = Theme.Sizes.ButtonHeight,
                Dock = DockStyle.Fill,
                Margin = new Padding(Theme.Spacing.Small)
            };
            Theme.StyleOutlineButton(btnDiag);
            btnDiag.Click += (_, __) => ShowDbDiagnostic();

            btnNewLoan = new Button
            {
                Text = "Nouveau prêt",
                Height = Theme.Sizes.ButtonHeight,
                Dock = DockStyle.Fill,
                Margin = new Padding(Theme.Spacing.Small)
            };
            Theme.StylePrimaryButton(btnNewLoan);
            btnNewLoan.Click += (_, __) => ShowLoanCreationDialog();

            bottomButtonsPanel.Controls.Add(new Label { BackColor = Theme.Colors.Background }, 0, 0); // Filler
            bottomButtonsPanel.Controls.Add(btnDiag, 1, 0);
            bottomButtonsPanel.Controls.Add(btnNewLoan, 2, 0);

            // Assemblage final
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(lvEquipments, 0, 1);
            mainLayout.Controls.Add(detailsTabControl, 0, 2);
            mainLayout.Controls.Add(new Label { BackColor = Theme.Colors.Background }, 0, 3); // Espacement
            mainLayout.Controls.Add(bottomButtonsPanel, 0, 4);

            Controls.Add(mainLayout);
            ResumeLayout(false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'initialisation : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Charge tous les équipements depuis la base de données
    /// </summary>
    private void LoadEquipments(string searchFilter = null)
    {
        try
        {
            lvEquipments.Items.Clear();
            
            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();
            var agentRepo = new Data.Repositories.MySQL.AgentMySqlRepository();

            var equipments = equipmentRepo.GetAll();
            var types = typeRepo.GetAll();
            var agents = agentRepo.GetAll();

            // Créer des dictionnaires pour les JOINs
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
            var agentDict = agents.ToDictionary(a => a.Idrh, a => $"{a.Nom} {a.Prenom}");

            // Filtrer les équipements (pas en état prêt = 1)
            var filteredEquipments = equipments.Where(e => e.EtatPret != 1);

            // Appliquer le filtre de recherche si fourni
            if (!string.IsNullOrWhiteSpace(searchFilter))
            {
                var q = searchFilter.ToLower();
                filteredEquipments = filteredEquipments.Where(e =>
                    (typeDict.ContainsKey(e.TypeId) && typeDict[e.TypeId].ToLower().Contains(q)) ||
                    (e.Nom?.ToLower().Contains(q) ?? false) ||
                    (e.CodeParc?.ToLower().Contains(q) ?? false) ||
                    (e.NumeroSerie?.ToLower().Contains(q) ?? false) ||
                    (e.Marque?.ToLower().Contains(q) ?? false)
                );
            }

            // Trier par type, puis nom
            var sortedEquipments = filteredEquipments
                .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
                .ThenBy(e => e.Nom ?? "");

            foreach (var eq in sortedEquipments)
            {
                var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";
                string etatLabel = eq.EtatPret switch
                {
                    0 => "Disponible",
                    1 => "Prêt",
                    2 => "DSEM",
                    _ => "Inconnu"
                };
                var agentName = string.IsNullOrEmpty(eq.Idrh) || !agentDict.ContainsKey(eq.Idrh) 
                    ? string.Empty 
                    : agentDict[eq.Idrh];

                var item = new ListViewItem(typeName);
                item.SubItems.AddRange(new[]
                {
                    eq.Nom ?? "",
                    eq.CodeParc ?? "",
                    eq.NumeroSerie ?? "",
                    eq.Marque ?? "",
                    agentName,
                    etatLabel
                });
                lvEquipments.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    

    /// <summary>
    /// Affiche la fenêtre de création d'un nouveau prêt
    /// </summary>
    private void ShowLoanCreationDialog()
    {
        var dialog = new LoanCreationView();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadEquipments();
            LoadLoans();
        }
    }

    /// <summary>
    /// Charge la liste des prêts en cours depuis la base de données
    /// </summary>
    private void LoadLoans()
    {
        try
        {
            lvEquipments.Items.Clear();
            lvEquipments.Columns.Clear();

            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();
            var agentRepo = new Data.Repositories.MySQL.AgentMySqlRepository();

            var equipments = equipmentRepo.GetAll().Where(e => e.EtatPret == 1 && !string.IsNullOrEmpty(e.Idrh)).ToList();
            var types = typeRepo.GetAll();
            var agents = agentRepo.GetAll();

            // Dictionnaire pour les types
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
            
            // Grouper les équipements par agent
            var equipmentsByAgent = equipments.GroupBy(e => e.Idrh).ToList();
            
            // Déterminer le nombre maximum d'équipements par agent
            var maxEquipments = equipmentsByAgent.Any() ? equipmentsByAgent.Max(g => g.Count()) : 0;

            // Configurer les colonnes
            lvEquipments.Columns.Add(new ColumnHeader { Text = "Agent", Width = 200 });
            for (int i = 1; i <= maxEquipments; i++)
            {
                lvEquipments.Columns.Add(new ColumnHeader { Text = $"Équipement {i}", Width = 250 });
            }

            // Créer un dictionnaire des agents
            var agentDict = agents.ToDictionary(a => a.Idrh, a => $"{a.Nom} {a.Prenom}");

            // Charger les agents qui ont des prêts
            var agentsWithLoans = equipmentsByAgent
                .Select(g => new { 
                    Idrh = g.Key, 
                    Name = agentDict.ContainsKey(g.Key) ? agentDict[g.Key] : g.Key,
                    Equipments = g.OrderBy(e => e.IdEquipement).ToList()
                })
                .OrderBy(a => a.Name)
                .ToList();

            // Pour chaque agent, charger ses équipements dans l'ordre
            foreach (var agent in agentsWithLoans)
            {
                var item = new ListViewItem(agent.Name) { Tag = agent.Idrh };

                foreach (var eq in agent.Equipments)
                {
                    var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";
                    var equipmentName = eq.Nom ?? eq.CodeParc ?? eq.NumeroSerie ?? "Sans nom";
                    var equipmentDisplay = $"{typeName} - {equipmentName} ({eq.CodeParc ?? "N/A"})";
                    item.SubItems.Add(equipmentDisplay);
                }

                // Remplir les colonnes restantes avec des cellules vides
                while (item.SubItems.Count < lvEquipments.Columns.Count)
                {
                    item.SubItems.Add(string.Empty);
                }

                lvEquipments.Items.Add(item);
            }

            if (lvEquipments.Items.Count == 0)
            {
                lvEquipments.Columns.Clear();
                lvEquipments.Columns.Add(new ColumnHeader { Text = "État", Width = 200 });
                var item = new ListViewItem("Aucun prêt en cours") { ForeColor = System.Drawing.Color.Gray };
                lvEquipments.Items.Add(item);
            }

            // S'assurer que l'événement n'est pas ajouté plusieurs fois
            lvEquipments.DoubleClick -= OnLoanDoubleClick;
            lvEquipments.DoubleClick += OnLoanDoubleClick;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des prêts : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnLoanDoubleClick(object sender, EventArgs e)
    {
        if (lvEquipments.SelectedItems.Count > 0 && lvEquipments.SelectedItems[0].Tag is string agentId)
        {
            OpenLoanEditor(agentId);
        }
    }

    private void OpenLoanEditor(string agentId)
    {
        var dialog = new LoanCreationView
        {
            SelectedAgentId = agentId
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadEquipments();
            LoadLoans();
        }
    }

    /// <summary>
    /// Gère la sélection d'une ligne dans le ListView pour afficher les détails
    /// </summary>
    private void LvEquipments_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvEquipments.SelectedItems.Count == 0)
        {
            return;
        }

        var selectedItem = lvEquipments.SelectedItems[0];
        var agentId = selectedItem.Tag as string;

        if (string.IsNullOrEmpty(agentId))
        {
            return;
        }

        // Vider et recréer les onglets
        detailsTabControl.TabPages.Clear();

        try
        {
            var agentRepo = new Data.Repositories.MySQL.AgentMySqlRepository();
            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var siteRepo = new Data.Repositories.MySQL.SiteMySqlRepository();
            var equipeRepo = new Data.Repositories.MySQL.EquipeMySqlRepository();
            var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();

            // Récupérer l'agent
            var agent = agentRepo.GetById(agentId);
            if (agent == null)
            {
                MessageBox.Show("Agent introuvable.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Récupérer les données liées
            var sites = siteRepo.GetAll();
            var equipes = equipeRepo.GetAll();
            var types = typeRepo.GetAll();

            var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);
            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

            // === ONGLET AGENT ===
            var agentTab = new TabPage("Agent")
            {
                BackColor = Theme.Colors.Surface,
                Padding = new Padding(Theme.Spacing.Medium)
            };

            var agentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                AutoSize = true,
                BackColor = Theme.Colors.Surface
            };

            // Configuration des colonnes
            agentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // Labels
            agentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Valeurs

            // Configuration des lignes
            for (int i = 0; i < 8; i++)
            {
                agentPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            }

            var siteName = agent.SiteId.HasValue && siteDict.ContainsKey(agent.SiteId.Value) 
                ? siteDict[agent.SiteId.Value] 
                : "Non assigné";
            var equipeName = agent.EquipeId.HasValue && equipeDict.ContainsKey(agent.EquipeId.Value)
                ? equipeDict[agent.EquipeId.Value]
                : "Non assignée";

            AddDetailRow(agentPanel, 0, "Nom :", agent.Nom ?? "");
            AddDetailRow(agentPanel, 1, "Prénom :", agent.Prenom ?? "");
            AddDetailRow(agentPanel, 2, "IDRH :", agent.Idrh ?? "");
            AddDetailRow(agentPanel, 3, "Email :", agent.Email ?? "");
            AddDetailRow(agentPanel, 4, "Site :", siteName);
            AddDetailRow(agentPanel, 5, "Équipe :", equipeName);

            agentTab.Controls.Add(agentPanel);
            detailsTabControl.TabPages.Add(agentTab);

            // === ONGLETS ÉQUIPEMENTS ===
            var agentEquipments = equipmentRepo.GetByAgent(agentId)
                .Where(e => e.EtatPret == 1)
                .OrderBy(e => e.IdEquipement)
                .ToList();

            int equipmentIndex = 1;
            foreach (var eq in agentEquipments)
            {
                var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";

                var equipTab = new TabPage($"Équipement {equipmentIndex}")
                {
                    BackColor = Theme.Colors.Surface,
                    Padding = new Padding(Theme.Spacing.Medium)
                };

                var equipPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 7,
                    AutoSize = true,
                    BackColor = Theme.Colors.Surface
                };

                // Configuration des colonnes
                equipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // Labels
                equipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Valeurs

                // Configuration des lignes
                for (int i = 0; i < 7; i++)
                {
                    equipPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                }

                string etatLabel = eq.EtatPret switch
                {
                    0 => "Disponible",
                    1 => "En prêt",
                    2 => "Rendu DSEM",
                    _ => "Inconnu"
                };

                AddDetailRow(equipPanel, 0, "Type :", typeName);
                AddDetailRow(equipPanel, 1, "Nom :", eq.Nom ?? "");
                AddDetailRow(equipPanel, 2, "Code Parc :", eq.CodeParc ?? "");
                AddDetailRow(equipPanel, 3, "N° Série :", eq.NumeroSerie ?? "");
                AddDetailRow(equipPanel, 4, "Marque :", eq.Marque ?? "");
                AddDetailRow(equipPanel, 5, "État :", etatLabel);
                
                // Afficher la date de rendu DSEM si elle existe
                if (eq.EtatPret == 2 && !string.IsNullOrEmpty(eq.DateRenduDsem))
                {
                    AddDetailRow(equipPanel, 6, "Date rendu DSEM :", eq.DateRenduDsem);
                }

                equipTab.Controls.Add(equipPanel);
                detailsTabControl.TabPages.Add(equipTab);

                equipmentIndex++;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des détails : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Ajoute une ligne de détail (label + valeur) dans un TableLayoutPanel
    /// </summary>
    private void AddDetailRow(TableLayoutPanel panel, int row, string labelText, string valueText)
    {
        var label = new Label
        {
            Text = labelText,
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(Theme.Spacing.Small)
        };

        var value = new Label
        {
            Text = valueText,
            Font = Theme.Fonts.Body,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(Theme.Spacing.Small),
            BackColor = Theme.Colors.SurfaceHover
        };

        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(value, 1, row);
    }

    /// <summary>
    /// Affiche un diagnostic rapide de la base (counts par état)
    /// </summary>
    private void ShowDbDiagnostic()
    {
        try
        {
            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var equipments = equipmentRepo.GetAll();

            var available = equipments.Count(e => e.EtatPret == 0);
            var loaned = equipments.Count(e => e.EtatPret == 1);
            var dsem = equipments.Count(e => e.EtatPret == 2);
            var total = equipments.Count();

            MessageBox.Show($"Equipements: total={total}\nDisponible={available}\nPrêt={loaned}\nDSEM={dsem}", "Diagnostic DB");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur diagnostic DB: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Génère une feuille de remise pour l'agent spécifié
    /// </summary>
    /// <param name="agentId">ID de l'agent</param>
    private void GenerateFeuilleRemise(string agentId)
    {
        try
        {
            var generator = new FeuilleRemiseGenerator();
            generator.GenerateFeuilleRemise(agentId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la génération de la feuille de remise : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Gestionnaire du menu contextuel pour générer une feuille de remise
    /// </summary>
    private void OnContextMenu_FeuilleRemise(object sender, EventArgs e)
    {
        if (lvEquipments.SelectedItems.Count > 0 && lvEquipments.SelectedItems[0].Tag is string agentId)
        {
            GenerateFeuilleRemise(agentId);
        }
        else
        {
            MessageBox.Show("Veuillez sélectionner un prêt pour générer la feuille de remise.", 
                          "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}