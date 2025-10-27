using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;

namespace ProjetParc.Views.Agent;

/// <summary>
/// Vue d'édition des agents : recherche, modification et suppression.
/// Présente une liste d'agents et un formulaire pour éditer les champs sélectionnés.
/// </summary>
public class AgentEditView : UserControl
{
    private TextBox tbSearch;
    private Button btnSearch;
    private ListView lvAgents;
    private ListViewColumnSorter lvAgentsSorter;

    private TextBox tbIDRH, tbAgentName, tbFirstName, tbEmail, tbComment;
    private ComboBox cbTeam, cbSite;
    private CheckBox cbxHeberge;

    private Button btnUpdate, btnDelete;
    private readonly Action _onBack;

    /// <summary>
    /// Initialise la vue d'édition des agents et charge les données initiales.
    /// </summary>
    /// <param name="onBack">Callback pour revenir à la vue précédente.</param>
    public AgentEditView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadAgentSite();
        LoadAgentTeam();
        LoadAgentList();

        btnSearch.Click += btnSearch_Click;
        lvAgents.SelectedIndexChanged += lbAgents_SelectedIndexChanged;
        btnUpdate.Click += (_, __) => SaveAgentChanges();
        btnDelete.Click += (_, __) => DeleteSelectedAgent();
    }

    /// <summary>
    /// Construit l'interface utilisateur (liste, champs, boutons) pour l'édition d'agent.
    /// </summary>
    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;

        // Layout principal : une ligne pour le bouton retour, une ligne pour le contenu
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20),
            BackColor = Theme.Colors.Background,
            RowStyles = {
                new RowStyle(SizeType.Absolute, 45),    // Bouton retour
                new RowStyle(SizeType.Percent, 100)     // Contenu
            }
        };
        Controls.Add(mainLayout);

        // Bouton retour
        var btnBack = new Button { Text = "← Retour", Width = Theme.Sizes.ButtonWidth, Anchor = AnchorStyles.Left };
        Theme.StyleSecondaryButton(btnBack);
        btnBack.Click += (_, __) => _onBack?.Invoke();
        mainLayout.Controls.Add(btnBack, 0, 0);

        // Panel de contenu : liste à gauche, séparateur, formulaire à droite
        TableLayoutPanel contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(0),
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 30),  // Liste des agents
                new ColumnStyle(SizeType.Absolute, 2),  // Séparateur
                new ColumnStyle(SizeType.Percent, 70)   // Formulaire d'édition
            }
        };
        mainLayout.Controls.Add(contentLayout, 0, 1);

        // Panel gauche (recherche et liste)
        TableLayoutPanel leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 0, 20, 0),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 50),    // Barre de recherche
                new RowStyle(SizeType.Percent, 100)     // Liste
            }
        };
        contentLayout.Controls.Add(leftPanel, 0, 0);

        // Barre de recherche
        TableLayoutPanel searchPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 10),
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 100),
                new ColumnStyle(SizeType.Absolute, 40)
            }
        };
        tbSearch = new TextBox { Dock = DockStyle.Fill, Font = Theme.Fonts.Body };
        Theme.StyleTextBox(tbSearch);
        btnSearch = new Button { Text = "🔍", Width = Theme.Sizes.SearchButtonSize, Height = Theme.Sizes.SearchButtonSize, Dock = DockStyle.Right };
        Theme.StyleSecondaryButton(btnSearch, setHeight: false);
        btnSearch.Font = new Font("Segoe UI", 12f);
        searchPanel.Controls.Add(tbSearch, 0, 0);
        searchPanel.Controls.Add(btnSearch, 1, 0);
        leftPanel.Controls.Add(searchPanel, 0, 0);

        // Liste des agents avec colonnes
        lvAgents = new ListView
        {
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Surface,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        lvAgents.Columns.Add("IDRH", 100);
        lvAgents.Columns.Add("Nom Prénom", 200);
        lvAgents.Columns.Add("Equipe", 120);
        lvAgents.Columns.Add("Site", 120);
        
        // Configuration du tri par colonnes
        lvAgentsSorter = new ListViewColumnSorter();
        lvAgents.ListViewItemSorter = lvAgentsSorter;
        lvAgents.ColumnClick += (s, e) => {
            lvAgentsSorter.SetSortColumn(e.Column);
            lvAgents.Sort();
        };
        
        leftPanel.Controls.Add(lvAgents, 0, 1);

        // Séparateur vertical
        var separator = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Colors.Border };
        contentLayout.Controls.Add(separator, 1, 0);

        // Panel droit (formulaire d'édition)
        TableLayoutPanel formPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 8,
            Padding = new Padding(20),
            BackColor = Theme.Colors.Surface,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 33.33f),
                new ColumnStyle(SizeType.Percent, 33.33f),
                new ColumnStyle(SizeType.Percent, 33.33f)
            },
            RowStyles = {
                new RowStyle(SizeType.Absolute, 30),  // Label ligne 1
                new RowStyle(SizeType.Absolute, 45),  // Input ligne 1
                new RowStyle(SizeType.Absolute, 30),  // Label ligne 2
                new RowStyle(SizeType.Absolute, 45),  // Input ligne 2
                new RowStyle(SizeType.Absolute, 30),  // Label ligne 3
                new RowStyle(SizeType.Absolute, 150), // Input ligne 3 (commentaire - hauteur fixe)
                new RowStyle(SizeType.Percent, 100),  // Espacement flexible
                new RowStyle(SizeType.Absolute, 60)   // Boutons
            }
        };
        contentLayout.Controls.Add(formPanel, 2, 0);

        // Première ligne : IDRH, Nom, Prénom
        tbIDRH = new TextBox { Height = Theme.Sizes.InputHeight, ReadOnly = true };
        Theme.StyleTextBox(tbIDRH);
        AddFormRow(formPanel, 0, "IDRH", tbIDRH);
        
        tbAgentName = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbAgentName);
        AddFormRow(formPanel, 0, "Nom", tbAgentName, 1);
        
        tbFirstName = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbFirstName);
        AddFormRow(formPanel, 0, "Prénom", tbFirstName, 2);

        // Deuxième ligne : Email, Équipe, Hébergé
        tbEmail = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbEmail);
        AddFormRow(formPanel, 2, "Email", tbEmail);
        
        cbTeam = new ComboBox { Height = Theme.Sizes.InputHeight, DropDownStyle = ComboBoxStyle.DropDownList };
        Theme.StyleComboBox(cbTeam);
        AddFormRow(formPanel, 2, "Équipe", cbTeam, 1);
        
        var hebergePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Colors.Surface };
        var lblHeb = new Label { Text = "Hébergé", AutoSize = true, Padding = new Padding(0, 5, 10, 0), Font = Theme.Fonts.Label, ForeColor = Theme.Colors.TextSecondary };
        cbxHeberge = new CheckBox { AutoSize = true };
        hebergePanel.Controls.AddRange(new Control[] { lblHeb, cbxHeberge });
        formPanel.Controls.Add(hebergePanel, 2, 2);
        formPanel.SetRowSpan(hebergePanel, 2);

        // Troisième ligne : Commentaire (2 colonnes), Site
        tbComment = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };
        Theme.StyleTextBox(tbComment);
        AddFormRow(formPanel, 4, "Commentaire", tbComment, 0, 2);
        
        cbSite = new ComboBox { Height = Theme.Sizes.InputHeight, DropDownStyle = ComboBoxStyle.DropDownList };
        Theme.StyleComboBox(cbSite);
        AddFormRow(formPanel, 4, "Site", cbSite, 2);

        // Boutons Modifier/Supprimer
        FlowLayoutPanel buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = Theme.Colors.Surface,
            WrapContents = false,
            Padding = new Padding(0)
        };
        formPanel.Controls.Add(buttonPanel, 0, 7);
        formPanel.SetColumnSpan(buttonPanel, 3);

        btnDelete = new Button { Text = "Supprimer", Width = Theme.Sizes.ButtonWidth, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0, 0, 10, 0) };
        Theme.StyleDangerButton(btnDelete);
        
        btnUpdate = new Button { Text = "Modifier", Width = Theme.Sizes.ButtonWidth, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0) };
        Theme.StylePrimaryButton(btnUpdate);
        
        buttonPanel.Controls.AddRange(new Control[] { btnUpdate, btnDelete });

        ResumeLayout(false);
    }

    /// <summary>Représentation d'un site (id, nom) utilisée par la ComboBox.</summary>
    private sealed class AgentSiteItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }
    /// <summary>Représentation d'une équipe (id, nom) utilisée par la ComboBox.</summary>
    private sealed class AgentTeamItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }

    /// <summary>Convertit une chaîne vide en <see cref="DBNull.Value"/>.</summary>
    private static object ToDbNullable(string s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();
    /// <summary>Convertit un bool en int (0/1).</summary>
    private static int ToBit(bool b) => b ? 1 : 0;

    /// <summary>Charge la liste des sites depuis la table <c>Sites</c>.</summary>
    private void LoadAgentSite()
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = "SELECT id, name FROM Sites ORDER BY name;";
            using var r = command.ExecuteReader();

            var items = new List<AgentSiteItem>();
            while (r.Read()) items.Add(new AgentSiteItem { Id = r.GetInt32(0), Name = r.GetString(1) });

            cbSite.DataSource = items;
            cbSite.DisplayMember = nameof(AgentSiteItem.Name);
            cbSite.ValueMember = nameof(AgentSiteItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des sites : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge la liste des équipes depuis la table <c>Equipes</c>.</summary>
    private void LoadAgentTeam()
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = "SELECT id, name FROM Equipes ORDER BY name;";
            using var r = command.ExecuteReader();

            var items = new List<AgentTeamItem>();
            while (r.Read()) items.Add(new AgentTeamItem { Id = r.GetInt32(0), Name = r.GetString(1) });

            cbTeam.DataSource = items;
            cbTeam.DisplayMember = nameof(AgentTeamItem.Name);
            cbTeam.ValueMember = nameof(AgentTeamItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipes : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge la liste complète des agents et l'affiche dans le ListView.</summary>
    private void LoadAgentList()
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = @"
                SELECT 
                    a.idrh, 
                    TRIM(COALESCE(a.nom,'')) AS n, 
                    TRIM(COALESCE(a.prenom,'')) AS p,
                    COALESCE(e.name, '-') AS equipe,
                    COALESCE(s.name, '-') AS site
                FROM ""Agents"" a
                LEFT JOIN ""Equipes"" e ON a.equipe_id = e.id
                LEFT JOIN ""Sites"" s ON a.site_id = s.id
                ORDER BY n, p, a.idrh;";
            using var r = command.ExecuteReader();

            lvAgents.Items.Clear();
            while (r.Read())
            {
                var id = r.IsDBNull(0) ? "" : r.GetString(0);
                var n = r.IsDBNull(1) ? "" : r.GetString(1);
                var p = r.IsDBNull(2) ? "" : r.GetString(2);
                var equipe = r.GetString(3);
                var site = r.GetString(4);
                
                var nomComplet = (n, p) switch { ("", "") => "-", _ => $"{n} {p}".Trim() };
                
                var item = new ListViewItem(id);
                item.SubItems.AddRange(new[] { nomComplet, equipe, site });
                item.Tag = id;
                lvAgents.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de la liste d'agents : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge la liste des agents en appliquant un filtre optionnel.</summary>
    private void LoadAgentListFiltered(string query)
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();

            if (string.IsNullOrWhiteSpace(query))
            {
                command.CommandText = @"
                    SELECT 
                        a.idrh, 
                        TRIM(COALESCE(a.nom,'')), 
                        TRIM(COALESCE(a.prenom,'')),
                        COALESCE(e.name, '-'),
                        COALESCE(s.name, '-')
                    FROM ""Agents"" a
                    LEFT JOIN ""Equipes"" e ON a.equipe_id = e.id
                    LEFT JOIN ""Sites"" s ON a.site_id = s.id
                    ORDER BY 2, 3, 1;";
            }
            else
            {
                command.CommandText = @"
                    SELECT 
                        a.idrh, 
                        TRIM(COALESCE(a.nom,'')), 
                        TRIM(COALESCE(a.prenom,'')),
                        COALESCE(e.name, '-'),
                        COALESCE(s.name, '-')
                    FROM ""Agents"" a
                    LEFT JOIN ""Equipes"" e ON a.equipe_id = e.id
                    LEFT JOIN ""Sites"" s ON a.site_id = s.id
                    WHERE a.idrh LIKE $p OR a.nom LIKE $p OR a.prenom LIKE $p OR a.email LIKE $p
                    ORDER BY 2, 3, 1;";
                command.Parameters.AddWithValue("$p", $"%{query}%");
            }

            using var r = command.ExecuteReader();
            lvAgents.Items.Clear();
            while (r.Read())
            {
                var id = r.IsDBNull(0) ? "" : r.GetString(0);
                var n = r.IsDBNull(1) ? "" : r.GetString(1);
                var p = r.IsDBNull(2) ? "" : r.GetString(2);
                var equipe = r.GetString(3);
                var site = r.GetString(4);
                
                var nomComplet = (n, p) switch { ("", "") => "-", _ => $"{n} {p}".Trim() };
                
                var item = new ListViewItem(id);
                item.SubItems.AddRange(new[] { nomComplet, equipe, site });
                item.Tag = id;
                lvAgents.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche d'agents : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge les détails d'un agent identifié par son IDRH et renseigne le formulaire.</summary>
    private void LoadAgentById(string agentIDRH)
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = @"SELECT idrh, nom, prenom, email, equipe_id, heberge, commentaire, site_id
                                    FROM ""Agents"" WHERE idrh = $IDRH;";
            command.Parameters.AddWithValue("$IDRH", agentIDRH);

            using var r = command.ExecuteReader();
            if (!r.Read()) { MessageBox.Show("Agent introuvable."); return; }

            tbIDRH.Text = r.IsDBNull(0) ? "" : r.GetString(0);
            tbAgentName.Text = r.IsDBNull(1) ? "" : r.GetString(1);
            tbFirstName.Text = r.IsDBNull(2) ? "" : r.GetString(2);
            tbEmail.Text = r.IsDBNull(3) ? "" : r.GetString(3);

            int? teamId = r.IsDBNull(4) ? null : r.GetInt32(4);
            bool heberge = !r.IsDBNull(5) && r.GetInt32(5) == 1;
            string comment = r.IsDBNull(6) ? "" : r.GetString(6);
            int? siteId = r.IsDBNull(7) ? null : r.GetInt32(7);

            tbComment.Text = comment;
            cbxHeberge.Checked = heberge;

            // select site by ID
            if (siteId.HasValue)
            {
                for (int i = 0; i < cbSite.Items.Count; i++)
                    if (cbSite.Items[i] is AgentSiteItem s && s.Id == siteId.Value) { cbSite.SelectedIndex = i; break; }
            }
            else cbSite.SelectedIndex = -1;

            // select team by ID
            if (teamId.HasValue)
            {
                for (int i = 0; i < cbTeam.Items.Count; i++)
                    if (cbTeam.Items[i] is AgentTeamItem t && t.Id == teamId.Value) { cbTeam.SelectedIndex = i; break; }
            }
            else cbTeam.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de l'agent : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Gestionnaire du bouton recherche : filtre la liste des agents.</summary>
    private void btnSearch_Click(object sender, EventArgs e)
        => LoadAgentListFiltered((tbSearch?.Text ?? "").Trim());

    /// <summary>Quand l'utilisateur change la sélection, charge les détails de l'agent sélectionné.</summary>
    private void lbAgents_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvAgents.SelectedItems.Count > 0)
        {
            var selectedItem = lvAgents.SelectedItems[0];
            var agentId = (string)selectedItem.Tag;
            LoadAgentById(agentId);
        }
    }

    /// <summary>Valide le formulaire d'édition pour s'assurer des champs minimaux requis.</summary>
    private bool ValidateAgentForm(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(tbIDRH.Text)) { errorMessage = "L'IDRH est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbAgentName.Text)) { errorMessage = "Le nom est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbFirstName.Text)) { errorMessage = "Le prénom est obligatoire."; return false; }
        errorMessage = ""; return true;
    }

    /// <summary>Enregistre les modifications de l'agent dans la base.</summary>
    private void SaveAgentChanges()
    {
        if (lvAgents.SelectedItems.Count == 0) { MessageBox.Show("Choisir d'abord un agent."); return; }
        if (!ValidateAgentForm(out var msg)) { MessageBox.Show(msg); return; }

        int? teamId = (cbTeam.SelectedItem as AgentTeamItem)?.Id;
        int? siteId = (cbSite.SelectedItem as AgentSiteItem)?.Id;

        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = @"UPDATE ""Agents"" 
                                SET nom = $name, prenom = $firstName, email = $email,
                                    equipe_id = $teamId, heberge = $heberge,
                                    commentaire = $comment, site_id = $siteId
                                WHERE idrh = $id;";

        command.Parameters.AddWithValue("$id", tbIDRH.Text.Trim());
        command.Parameters.AddWithValue("$name", tbAgentName.Text.Trim());
        command.Parameters.AddWithValue("$firstName", tbFirstName.Text.Trim());
        command.Parameters.AddWithValue("$email", tbEmail.Text.Trim());
        command.Parameters.AddWithValue("$teamId", teamId is null ? (object)DBNull.Value : teamId.Value);
        command.Parameters.AddWithValue("$heberge", ToBit(cbxHeberge.Checked));
        command.Parameters.AddWithValue("$comment", ToDbNullable(tbComment.Text));
        command.Parameters.AddWithValue("$siteId", siteId is null ? (object)DBNull.Value : siteId.Value);

        try
        {
            var rows = command.ExecuteNonQuery();
            if (rows == 0) { MessageBox.Show("Aucune modification (agent introuvable ?)."); return; }

            MessageBox.Show("Modifications enregistrées.");

            // Recharger la liste pour refléter les modifications
            LoadAgentList();
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Erreur SQL : " + ex.Message);
        }
    }

    /// <summary>Supprime l'agent sélectionné après confirmation utilisateur.</summary>
    private void DeleteSelectedAgent()
    {
        if (lvAgents.SelectedItems.Count == 0)
        { MessageBox.Show("Sélectionne un agent à supprimer."); return; }

        var selectedItem = lvAgents.SelectedItems[0];
        var agentId = (string)selectedItem.Tag;
        var agentLabel = $"{selectedItem.SubItems[1].Text} [{selectedItem.Text}]";
        
        var confirm = MessageBox.Show(
            $"Supprimer « {agentLabel} » ?",
            "Confirmer la suppression",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = @"DELETE FROM ""Agents"" WHERE idrh = $id;";
            command.Parameters.AddWithValue("$id", agentId);
            var rows = command.ExecuteNonQuery();

            if (rows == 0) { MessageBox.Show("Agent introuvable."); return; }

            LoadAgentListFiltered(tbSearch?.Text?.Trim() ?? "");
            tbIDRH.Clear(); tbAgentName.Clear(); tbFirstName.Clear(); tbEmail.Clear(); tbComment.Clear();
            cbxHeberge.Checked = false;
            MessageBox.Show("Agent supprimé.");
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Erreur SQL : " + ex.Message);
        }
    }

    /// <summary>
    /// Ajoute une ligne de formulaire avec un label et un contrôle dans le TableLayoutPanel
    /// </summary>
    private void AddFormRow(TableLayoutPanel panel, int row, string labelText, Control control, int col = 0, int colSpan = 1)
    {
        var label = new Label 
        { 
            Text = labelText, 
            Dock = DockStyle.Fill,
            Padding = new Padding(5, 0, 0, 5),
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary
        };
        panel.Controls.Add(label, col, row);

        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(5, 0, 15, 15);
        panel.Controls.Add(control, col, row + 1);
        if (colSpan > 1)
        {
            panel.SetColumnSpan(control, colSpan);
        }
    }
}
