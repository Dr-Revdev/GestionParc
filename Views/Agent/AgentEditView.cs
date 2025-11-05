using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProjetParc.Data;

namespace ProjetParc.Views.Agent;

/// <summary>
/// Écran de modification/suppression des agents. À gauche une liste avec recherche,
/// à droite le formulaire pour modifier les champs. On peut aussi supprimer un agent.
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
    /// Constructeur - monte l'UI et charge toutes les données (sites, équipes, liste agents)
    /// </summary>
    /// <param name="onBack">Callback retour</param>
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
    /// Monte toute l'interface - split en 2 parties (30% liste / 70% formulaire)
    /// avec une barre de recherche au-dessus de la liste
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

    /// <summary>Classe pour les items des listes déroulantes Site</summary>
    private sealed class AgentSiteItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }
    /// <summary>Classe pour les items des listes déroulantes Equipe</summary>
    private sealed class AgentTeamItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }

    /// <summary>Remplit la combobox des sites</summary>
    private void LoadAgentSite()
    {
        try
        {
            var repo = new Data.Repositories.MySQL.SiteMySqlRepository();
            var sites = repo.GetAll();

            var items = new List<AgentSiteItem>();
            foreach (var site in sites)
            {
                items.Add(new AgentSiteItem { Id = site.Id, Name = site.Name });
            }

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

    /// <summary>Remplit la combobox des équipes</summary>
    private void LoadAgentTeam()
    {
        try
        {
            var repo = new Data.Repositories.MySQL.EquipeMySqlRepository();
            var equipes = repo.GetAll();

            var items = new List<AgentTeamItem>();
            foreach (var equipe in equipes)
            {
                items.Add(new AgentTeamItem { Id = equipe.Id, Name = equipe.Name });
            }

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

    /// <summary>Charge tous les agents et les affiche dans la liste (triés par nom/prénom)</summary>
    private void LoadAgentList()
    {
        try
        {
            var agentRepo = new Data.Repositories.MySQL.AgentMySqlRepository();
            var equipeRepo = new Data.Repositories.MySQL.EquipeMySqlRepository();
            var siteRepo = new Data.Repositories.MySQL.SiteMySqlRepository();

            var agents = agentRepo.GetAll();
            var equipes = equipeRepo.GetAll();
            var sites = siteRepo.GetAll();

            // Créer des dictionnaires pour les JOINs en mémoire
            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);
            var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);

            lvAgents.Items.Clear();

            // Trier par nom, prénom, idrh
            var sortedAgents = agents
                .OrderBy(a => a.Nom ?? "")
                .ThenBy(a => a.Prenom ?? "")
                .ThenBy(a => a.Idrh);

            foreach (var agent in sortedAgents)
            {
                var nom = agent.Nom?.Trim() ?? "";
                var prenom = agent.Prenom?.Trim() ?? "";
                var nomComplet = (nom, prenom) switch { ("", "") => "-", _ => $"{nom} {prenom}".Trim() };
                
                var equipeName = (agent.EquipeId.HasValue && equipeDict.ContainsKey(agent.EquipeId.Value)) 
                    ? equipeDict[agent.EquipeId.Value] : "-";
                var siteName = (agent.SiteId.HasValue && siteDict.ContainsKey(agent.SiteId.Value)) 
                    ? siteDict[agent.SiteId.Value] : "-";

                var item = new ListViewItem(agent.Idrh);
                item.SubItems.AddRange(new[] { nomComplet, equipeName, siteName });
                item.Tag = agent.Idrh;
                lvAgents.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de la liste d'agents : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge les agents avec un filtre de recherche (cherche dans IDRH, nom, prénom, email)</summary>
    private void LoadAgentListFiltered(string query)
    {
        try
        {
            var agentRepo = new Data.Repositories.MySQL.AgentMySqlRepository();
            var equipeRepo = new Data.Repositories.MySQL.EquipeMySqlRepository();
            var siteRepo = new Data.Repositories.MySQL.SiteMySqlRepository();

            var agents = agentRepo.GetAll();
            var equipes = equipeRepo.GetAll();
            var sites = siteRepo.GetAll();

            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);
            var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);

            // Filtrer si une requête est fournie
            IEnumerable<Data.DTOs.AgentDto> filteredAgents = agents;
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.ToLower();
                filteredAgents = agents.Where(a =>
                    (a.Idrh?.ToLower().Contains(q) ?? false) ||
                    (a.Nom?.ToLower().Contains(q) ?? false) ||
                    (a.Prenom?.ToLower().Contains(q) ?? false) ||
                    (a.Email?.ToLower().Contains(q) ?? false)
                );
            }

            // Trier
            var sortedAgents = filteredAgents
                .OrderBy(a => a.Nom ?? "")
                .ThenBy(a => a.Prenom ?? "")
                .ThenBy(a => a.Idrh);

            lvAgents.Items.Clear();

            foreach (var agent in sortedAgents)
            {
                var nom = agent.Nom?.Trim() ?? "";
                var prenom = agent.Prenom?.Trim() ?? "";
                var nomComplet = (nom, prenom) switch { ("", "") => "-", _ => $"{nom} {prenom}".Trim() };
                
                var equipeName = (agent.EquipeId.HasValue && equipeDict.ContainsKey(agent.EquipeId.Value)) 
                    ? equipeDict[agent.EquipeId.Value] : "-";
                var siteName = (agent.SiteId.HasValue && siteDict.ContainsKey(agent.SiteId.Value)) 
                    ? siteDict[agent.SiteId.Value] : "-";

                var item = new ListViewItem(agent.Idrh);
                item.SubItems.AddRange(new[] { nomComplet, equipeName, siteName });
                item.Tag = agent.Idrh;
                lvAgents.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche d'agents : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Récupère un agent depuis la base et remplit tous les champs du formulaire</summary>
    private void LoadAgentById(string agentIDRH)
    {
        try
        {
            var repo = new Data.Repositories.MySQL.AgentMySqlRepository();
            var agent = repo.GetById(agentIDRH);

            tbIDRH.Text = agent.Idrh ?? "";
            tbAgentName.Text = agent.Nom ?? "";
            tbFirstName.Text = agent.Prenom ?? "";
            tbEmail.Text = agent.Email ?? "";
            tbComment.Text = agent.Commentaire ?? "";
            cbxHeberge.Checked = agent.Heberge == 1;

            // Sélectionner le site
            for (int i = 0; i < cbSite.Items.Count; i++)
            {
                if (cbSite.Items[i] is AgentSiteItem s && s.Id == agent.SiteId)
                {
                    cbSite.SelectedIndex = i;
                    break;
                }
            }

            // Sélectionner l'équipe
            for (int i = 0; i < cbTeam.Items.Count; i++)
            {
                if (cbTeam.Items[i] is AgentTeamItem t && t.Id == agent.EquipeId)
                {
                    cbTeam.SelectedIndex = i;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de l'agent : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Handler du bouton de recherche - applique le filtre</summary>
    private void btnSearch_Click(object sender, EventArgs e)
        => LoadAgentListFiltered((tbSearch?.Text ?? "").Trim());

    /// <summary>Quand on clique sur un agent dans la liste, on charge ses infos dans le formulaire</summary>
    private void lbAgents_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvAgents.SelectedItems.Count > 0)
        {
            var selectedItem = lvAgents.SelectedItems[0];
            var agentId = (string)selectedItem.Tag;
            LoadAgentById(agentId);
        }
    }

    /// <summary>Vérifie que l'IDRH, nom et prénom sont bien remplis</summary>
    private bool ValidateAgentForm(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(tbIDRH.Text)) { errorMessage = "L'IDRH est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbAgentName.Text)) { errorMessage = "Le nom est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbFirstName.Text)) { errorMessage = "Le prénom est obligatoire."; return false; }
        errorMessage = ""; return true;
    }

    /// <summary>Sauvegarde les modifs de l'agent en base (UPDATE)</summary>
    private void SaveAgentChanges()
    {
        if (lvAgents.SelectedItems.Count == 0) { MessageBox.Show("Choisir d'abord un agent."); return; }
        if (!ValidateAgentForm(out var msg)) { MessageBox.Show(msg); return; }

        int? teamId = (cbTeam.SelectedItem as AgentTeamItem)?.Id;
        int? siteId = (cbSite.SelectedItem as AgentSiteItem)?.Id;

        try
        {
            // Créer un DTO avec les valeurs modifiées
            var agent = new Data.DTOs.AgentDto(
                Idrh: tbIDRH.Text.Trim(),
                Nom: tbAgentName.Text.Trim(),
                Prenom: tbFirstName.Text.Trim(),
                Email: tbEmail.Text.Trim(),
                EquipeId: teamId,
                SiteId: siteId,
                Heberge: cbxHeberge.Checked ? 1 : 0,
                Commentaire: string.IsNullOrWhiteSpace(tbComment.Text) ? null : tbComment.Text.Trim()
            );

            // Appeler le repository pour la mise à jour
            var repo = new Data.Repositories.MySQL.AgentMySqlRepository();
            repo.Update(agent);

            MessageBox.Show("Modifications enregistrées.");

            // Recharger la liste pour refléter les modifications
            LoadAgentList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Supprime l'agent (demande confirmation avant)</summary>
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
            var repo = new Data.Repositories.MySQL.AgentMySqlRepository();
            repo.Delete(agentId);

            LoadAgentListFiltered(tbSearch?.Text?.Trim() ?? "");
            tbIDRH.Clear(); 
            tbAgentName.Clear(); 
            tbFirstName.Clear(); 
            tbEmail.Clear(); 
            tbComment.Clear();
            cbxHeberge.Checked = false;
            MessageBox.Show("Agent supprimé.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Helper pour ajouter un champ dans le formulaire (label + contrôle)
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
