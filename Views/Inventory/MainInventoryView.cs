using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
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
            Font = new Font("Segoe UI", 11f);
            Padding = new Padding(20);

            // TableLayoutPanel principal pour organiser la vue
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Padding = new Padding(0)
            };

            // Configuration des lignes
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // En-tête
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Liste des équipements
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Détails (TabControl)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10)); // Espacement
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Boutons en bas

            // En-tête avec bouton retour et titre
            var headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };

            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Espace futur

            var btnBack = new Button { 
                Text = "← Retour", 
                Height = 36,
                Dock = DockStyle.Left,
                Width = 120
            };
            btnBack.Click += (_, __) => _onBack?.Invoke();

            lblTitle = new Label 
            { 
                Text = "Equipements en place",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            headerPanel.Controls.Add(btnBack, 0, 0);
            headerPanel.Controls.Add(lblTitle, 1, 0);

            // Liste des équipements
            lvEquipments = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvEquipments.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Agent", Width = 200 },
                new ColumnHeader { Text = "Équipements", Width = 800 }
            });
            lvEquipments.SelectedIndexChanged += LvEquipments_SelectedIndexChanged;

            // TabControl pour les détails
            detailsTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f)
            };
            
            // Onglet par défaut (vide au démarrage)
            var defaultTab = new TabPage("Sélectionnez une ligne")
            {
                BackColor = Color.White
            };
            var defaultLabel = new Label
            {
                Text = "Sélectionnez une ligne dans le tableau ci-dessus pour voir les détails",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 11f, FontStyle.Italic)
            };
            defaultTab.Controls.Add(defaultLabel);
            detailsTabControl.TabPages.Add(defaultTab);

            // Panel des boutons en bas
            var bottomButtonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };

            bottomButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Espace
            bottomButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Bouton diagnostic
            bottomButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton nouveau prêt

            var btnDiag = new Button
            {
                Text = "Diag DB",
                Height = 32,
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };
            btnDiag.Click += (_, __) => ShowDbDiagnostic();

            btnNewLoan = new Button
            {
                Text = "Nouveau prêt",
                Height = 32,
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };
            btnNewLoan.Click += (_, __) => ShowLoanCreationDialog();

            bottomButtonsPanel.Controls.Add(new Label(), 0, 0); // Filler
            bottomButtonsPanel.Controls.Add(btnDiag, 1, 0);
            bottomButtonsPanel.Controls.Add(btnNewLoan, 2, 0);

            // Assemblage final
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(lvEquipments, 0, 1);
            mainLayout.Controls.Add(detailsTabControl, 0, 2);
            mainLayout.Controls.Add(new Label(), 0, 3); // Espacement
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
            using var connection = Database.Open();
            using var command = connection.CreateCommand();

            if (string.IsNullOrWhiteSpace(searchFilter))
            {
                command.CommandText = @"
                    SELECT e.type_id, t.name, e.nom, e.code_parc, e.numero_serie, e.marque, e.etat_pret, e.idrh, a.nom || ' ' || a.prenom as agent_name
                    FROM Equipements e
                    LEFT JOIN Agents a ON a.idrh = e.idrh
                    JOIN equipment_type t ON t.id = e.type_id
                    WHERE e.etat_pret != 1
                    ORDER BY t.name, e.nom";
            }
            else
            {
                     command.CommandText = @"
                          SELECT e.type_id, t.name, e.nom, e.code_parc, e.numero_serie, e.marque, e.etat_pret, e.idrh, a.nom || ' ' || a.prenom as agent_name
                          FROM Equipements e
                          LEFT JOIN Agents a ON a.idrh = e.idrh
                          JOIN equipment_type t ON t.id = e.type_id
                          WHERE t.name LIKE $search 
                              OR e.nom LIKE $search
                              OR e.code_parc LIKE $search
                              OR e.numero_serie LIKE $search
                              OR e.marque LIKE $search
                          ORDER BY t.name, e.nom";
                command.Parameters.AddWithValue("$search", $"%{searchFilter}%");
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new ListViewItem(reader.GetString(1)); // Type
                var etat = reader.GetInt32(6);
                string etatLabel = etat switch
                {
                    0 => "Disponible",
                    1 => "Prêt",
                    2 => "DSEM",
                    _ => "Inconnu"
                };
                var agentName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                item.SubItems.AddRange(new[]
                {
                    reader.GetString(2),  // Nom
                    reader.GetString(3),  // Code Parc
                    reader.GetString(4),  // N° Série
                    reader.GetString(5),  // Marque
                    agentName,             // Agent
                    etatLabel              // État
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

            using var connection = Database.Open();
            
            // Première passe : déterminer le nombre maximum d'équipements par agent
            using (var countCommand = connection.CreateCommand())
            {
                countCommand.CommandText = @"
                    SELECT COALESCE(MAX(equipment_count), 0)
                    FROM (
                        SELECT COUNT(*) as equipment_count
                        FROM Equipements e
                        WHERE e.etat_pret = 1 AND e.idrh IS NOT NULL
                        GROUP BY e.idrh
                    )";
                var result = countCommand.ExecuteScalar();
                var maxEquipments = result == DBNull.Value ? 0 : Convert.ToInt32(result);

                // Configurer les colonnes
                lvEquipments.Columns.Add(new ColumnHeader { Text = "Agent", Width = 200 });
                for (int i = 1; i <= maxEquipments; i++)
                {
                    lvEquipments.Columns.Add(new ColumnHeader { Text = $"Équipement {i}", Width = 250 });
                }
            }

            // Deuxième passe : charger les données
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    a.idrh,
                    a.nom || ' ' || a.prenom as agent_name,
                    GROUP_CONCAT(t.name || ' - ' || e.nom || ' (' || e.code_parc || ')', '||') as equipments
                FROM Equipements e
                JOIN Agents a ON a.idrh = e.idrh
                JOIN equipment_type t ON t.id = e.type_id
                WHERE e.etat_pret = 1
                GROUP BY a.idrh, a.nom, a.prenom
                ORDER BY a.nom, a.prenom";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var agentId = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                var agentName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var equipments = reader.IsDBNull(2) ? new string[0] : reader.GetString(2).Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                var item = new ListViewItem(agentName) { Tag = agentId };
                
                // Ajouter chaque équipement dans sa propre colonne
                foreach (var eq in equipments)
                {
                    item.SubItems.Add(eq.Trim());
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
            using var connection = Database.Open();

            // === ONGLET AGENT ===
            var agentTab = new TabPage("👤 Agent")
            {
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            var agentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                AutoSize = true
            };

            // Configuration des colonnes
            agentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // Labels
            agentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Valeurs

            // Configuration des lignes
            for (int i = 0; i < 8; i++)
            {
                agentPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            }

            // Requête pour récupérer les infos de l'agent
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT 
                        a.nom, 
                        a.prenom, 
                        a.idrh, 
                        a.email,
                        s.name as site_name,
                        e.name as equipe_name,
                        a.heberge,
                        a.commentaire
                    FROM Agents a
                    LEFT JOIN Sites s ON a.site_id = s.id
                    LEFT JOIN Equipes e ON a.equipe_id = e.id
                    WHERE a.idrh = $agentId";
                cmd.Parameters.AddWithValue("$agentId", agentId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var nom = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    var prenom = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var idrh = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var email = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var site = reader.IsDBNull(4) ? "Non assigné" : reader.GetString(4);
                    var equipe = reader.IsDBNull(5) ? "Non assignée" : reader.GetString(5);
                    var heberge = reader.GetInt32(6);
                    var commentaire = reader.IsDBNull(7) ? "" : reader.GetString(7);

                    AddDetailRow(agentPanel, 0, "Nom :", nom);
                    AddDetailRow(agentPanel, 1, "Prénom :", prenom);
                    AddDetailRow(agentPanel, 2, "IDRH :", idrh);
                    AddDetailRow(agentPanel, 3, "Email :", email);
                    AddDetailRow(agentPanel, 4, "Site :", site);
                    AddDetailRow(agentPanel, 5, "Équipe :", equipe);
                }
            }

            agentTab.Controls.Add(agentPanel);
            detailsTabControl.TabPages.Add(agentTab);

            // === ONGLETS ÉQUIPEMENTS ===
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT 
                        e.code_parc,
                        t.name as type_equipement,
                        e.nom,
                        e.numero_serie,
                        e.marque,
                        e.etat_pret
                    FROM Equipements e
                    JOIN equipment_type t ON t.id = e.type_id
                    WHERE e.idrh = $agentId AND e.etat_pret = 1
                    ORDER BY t.name, e.nom";
                cmd.Parameters.AddWithValue("$agentId", agentId);

                using var reader = cmd.ExecuteReader();
                int equipmentIndex = 1;

                while (reader.Read())
                {
                    var codeparc = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    var type = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var nom = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var numeroSerie = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var marque = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    var etat = reader.GetInt32(5);

                    var equipTab = new TabPage($"🖥️ Équipement {equipmentIndex}")
                    {
                        BackColor = Color.White,
                        Padding = new Padding(15)
                    };

                    var equipPanel = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 2,
                        RowCount = 6,
                        AutoSize = true
                    };

                    // Configuration des colonnes
                    equipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // Labels
                    equipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Valeurs

                    // Configuration des lignes
                    for (int i = 0; i < 6; i++)
                    {
                        equipPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                    }

                    string etatLabel = etat switch
                    {
                        0 => "Disponible",
                        1 => "En prêt",
                        2 => "Rendu DSEM",
                        _ => "Inconnu"
                    };

                    AddDetailRow(equipPanel, 0, "Type :", type);
                    AddDetailRow(equipPanel, 1, "Nom :", nom);
                    AddDetailRow(equipPanel, 2, "Code Parc :", codeparc);
                    AddDetailRow(equipPanel, 3, "N° Série :", numeroSerie);
                    AddDetailRow(equipPanel, 4, "Marque :", marque);
                    AddDetailRow(equipPanel, 5, "État :", etatLabel);

                    equipTab.Controls.Add(equipPanel);
                    detailsTabControl.TabPages.Add(equipTab);

                    equipmentIndex++;
                }
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
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5)
        };

        var value = new Label
        {
            Text = valueText,
            Font = new Font("Segoe UI", 10f),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5),
            BackColor = Color.FromArgb(245, 245, 245)
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
            using var connection = Database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT
                  SUM(CASE WHEN etat_pret = 0 THEN 1 ELSE 0 END) as available,
                  SUM(CASE WHEN etat_pret = 1 THEN 1 ELSE 0 END) as loaned,
                  SUM(CASE WHEN etat_pret = 2 THEN 1 ELSE 0 END) as dsem,
                  COUNT(*) as total
                FROM Equipements";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var available = reader.GetInt32(0);
                var loaned = reader.GetInt32(1);
                var dsem = reader.GetInt32(2);
                var total = reader.GetInt32(3);
                MessageBox.Show($"Equipements: total={total}\nDisponible={available}\nPrêt={loaned}\nDSEM={dsem}", "Diagnostic DB");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur diagnostic DB: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}