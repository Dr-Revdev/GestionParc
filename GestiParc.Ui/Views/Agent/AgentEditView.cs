using GestiParc.Core.DTOs;
using GestiParc.Ui.Services.Api;
using System.Net.Http;

namespace GestiParc.Ui.Views.Agent;

/// <summary>
/// Écran de modification/suppression des agents. À gauche une liste avec recherche,
/// à droite le formulaire pour modifier les champs. On peut aussi supprimer un agent.
/// </summary>
public class AgentEditView : UserControl
{
    private readonly AgentApiClient _agentApiClient = new AgentApiClient();
    private readonly EquipeApiClient _equipeApiClient = new EquipeApiClient();
    private readonly SiteApiClient _siteApiClient = new SiteApiClient();
    private TextBox tbSearch = null!;
    private Button btnSearch = null!;
    private ListView lvAgents = null!;
    private ListViewColumnSorter lvAgentsSorter = null!;

    private TextBox tbIDRH = null!, tbAgentName = null!, tbFirstName = null!, tbEmail = null!, tbComment = null!;
    private ComboBox cbTeam = null!, cbSite = null!;
    private CheckBox cbxHeberge = null!;

    private Button btnUpdate = null!, btnDelete = null!;
    private readonly Action _onBack;

    /// <summary>
    /// Constructeur - monte l'UI et charge toutes les données (sites, équipes, liste agents)
    /// </summary>
    /// <param name="onBack">Callback retour</param>
    public AgentEditView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();

        btnSearch.Click += btnSearch_Click;
        lvAgents.SelectedIndexChanged += lbAgents_SelectedIndexChanged;
        btnUpdate.Click += btnUpdate_Click;
        btnDelete.Click += btnDelete_Click;

        Load += async (sender, e) => await LoadAgentSiteAsync();
        Load += async (sender, e) => await LoadAgentTeamAsync();
        Load += async (sender, e) => await LoadAgentListAsync();
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
            Theme.ApplyListViewAlternatingRowColors(lvAgents);
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
    private async Task LoadAgentSiteAsync()
    {
        try
        {
            var sites = await _siteApiClient.GetAllAsync();

            var siteItems = sites
                .Select(s => new AgentSiteItem { Id = s.Id, Name = s.Name })
                .OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            cbSite.DataSource = siteItems;
            cbSite.DisplayMember = nameof(AgentSiteItem.Name);
            cbSite.ValueMember = nameof(AgentSiteItem.Id);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les sites. \n\n{ex.Message}",
            "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur  : {ex.Message}", 
            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Remplit la combobox des équipes</summary>
    private async Task LoadAgentTeamAsync()
    {
        try
        {
            var teams = await _equipeApiClient.GetAllAsync();

            var equipeItems = teams
                .Select(t => new AgentTeamItem { Id = t.Id, Name = t.Name })
                .OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            cbTeam.DataSource = equipeItems;
            cbTeam.DisplayMember = nameof(AgentTeamItem.Name);
            cbTeam.ValueMember = nameof(AgentTeamItem.Id);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les équipes. \n\n{ex.Message}",
            "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}",
            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge tous les agents et les affiche dans la liste (triés par nom/prénom)</summary>
    private async Task LoadAgentListAsync()
    {
        try
        {
            // Charger tout en parallèle
            var agentsTask = _agentApiClient.GetAllAsync();
            var sitesTask = _siteApiClient.GetAllAsync();
            var teamsTask = _equipeApiClient.GetAllAsync();
            
            await Task.WhenAll(agentsTask, sitesTask, teamsTask);
            
            var agents = agentsTask.Result;
            var sites = sitesTask.Result;
            var teams = teamsTask.Result;
            
            var sitesDictionary = sites.ToDictionary(s => s.Id, s => s.Name);
            var teamsDictionary = teams.ToDictionary(t => t.Id, t => t.Name);

            // Vider et remplir le ListView
            lvAgents.BeginUpdate();
            lvAgents.Items.Clear();

            var sortedAgents = agents
                .OrderBy(a => a.Nom ?? "")
                .ThenBy(a => a.Prenom ?? "")
                .ThenBy(a => a.Idrh);

            var items = new List<ListViewItem>();
            foreach (var agent in sortedAgents)
            {
                var idrh = string.IsNullOrWhiteSpace(agent.Idrh) ? "-" : agent.Idrh.Trim();
                var nom = string.IsNullOrWhiteSpace(agent.Nom) ? "-" : agent.Nom.Trim();
                var prenom = string.IsNullOrWhiteSpace(agent.Prenom) ? "-" : agent.Prenom.Trim();
                var equipe = agent.EquipeId.HasValue && teamsDictionary.TryGetValue(agent.EquipeId.Value, out var equipeValue) ? equipeValue : "Inconnu";
                var site = agent.SiteId.HasValue && sitesDictionary.TryGetValue(agent.SiteId.Value, out var siteValue) ? siteValue : "Inconnu";
                var nomComplet = $"{nom} {prenom}".Trim();

                var item = new ListViewItem(idrh);
                item.SubItems.AddRange(new[] { nomComplet, equipe, site });
                item.Tag = agent.Idrh;
                items.Add(item);
            }

            if (items.Count > 0)
            {
                lvAgents.Items.AddRange(items.ToArray());
            }

            Theme.ApplyListViewReadability(lvAgents, Theme.Sizes.ColumnWidthMedium);

            lvAgents.EndUpdate();

        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les agents.\n\n : {ex.Message}",
            "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des agents : {ex.Message}",
            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge les agents avec un filtre de recherche (cherche dans IDRH, nom, prénom, email)</summary>
    private async Task LoadAgentListFilteredAsync(string query)
    {
        try
        {
            // Charger tout en parallèle
            var agentsTask = _agentApiClient.GetAllAsync();
            var equipesTask = _equipeApiClient.GetAllAsync();
            var sitesTask = _siteApiClient.GetAllAsync();
            
            await Task.WhenAll(agentsTask, equipesTask, sitesTask);
            
            var agents = agentsTask.Result;
            var equipes = equipesTask.Result;
            var sites = sitesTask.Result;

            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);
            var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);

            // Filtrer si une requête est fournie
            IEnumerable<AgentDto> filteredAgents = agents;
            
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

            lvAgents.BeginUpdate();
            lvAgents.Items.Clear();

            var items = new List<ListViewItem>();
            foreach (var agent in sortedAgents)
            {
                var nom = agent.Nom?.Trim() ?? "";
                var prenom = agent.Prenom?.Trim() ?? "";
                var nomComplet = (nom, prenom) switch { ("", "") => "-", _ => $"{nom} {prenom}".Trim() };
                
                var equipeName = (agent.EquipeId.HasValue && equipeDict.TryGetValue(agent.EquipeId.Value, out var equipeValue))
                    ? equipeValue : "-";
                var siteName = (agent.SiteId.HasValue && siteDict.TryGetValue(agent.SiteId.Value, out var siteValue))
                    ? siteValue : "-";

                var item = new ListViewItem(agent.Idrh);
                item.SubItems.AddRange(new[] { nomComplet, equipeName, siteName });
                item.Tag = agent.Idrh;
                items.Add(item);
            }

            if (items.Count > 0)
            {
                lvAgents.Items.AddRange(items.ToArray());
            }

            Theme.ApplyListViewReadability(lvAgents, Theme.Sizes.ColumnWidthMedium);

            lvAgents.EndUpdate();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de joindre le serveur : {ex.Message}", "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche d'agents : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Récupère un agent depuis l'API et remplit tous les champs du formulaire</summary>
    private async Task LoadAgentByIdAsync(string agentIDRH)
    {
        try
        {
            var agent = await _agentApiClient.GetByIdAsync(agentIDRH);

            tbIDRH.Text = agent?.Idrh ?? "";
            tbAgentName.Text = agent?.Nom ?? "";
            tbFirstName.Text = agent?.Prenom ?? "";
            tbEmail.Text = agent?.Email ?? "";
            tbComment.Text = agent?.Commentaire ?? "";
            cbxHeberge.Checked = agent?.Heberge == 1;

            // Sélectionner le site
            for (int i = 0; i < cbSite.Items.Count; i++)
            {
                if (cbSite.Items[i] is AgentSiteItem s && s.Id == agent?.SiteId)
                {
                    cbSite.SelectedIndex = i;
                    break;
                }
            }

            // Sélectionner l'équipe
            for (int i = 0; i < cbTeam.Items.Count; i++)
            {
                if (cbTeam.Items[i] is AgentTeamItem t && t.Id == agent?.EquipeId)
                {
                    cbTeam.SelectedIndex = i;
                    break;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de joindre le serveur : {ex.Message}", "Erreur réseau",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de l'agent : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Handler du bouton de recherche - applique le filtre</summary>
    private async void btnSearch_Click(object? sender, EventArgs e)
    {
        var q = (tbSearch?.Text ?? "").Trim();
        await LoadAgentListFilteredAsync(q);
    }

    /// <summary>Quand on clique sur un agent dans la liste, on charge ses infos dans le formulaire</summary>
    private async void lbAgents_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lvAgents.SelectedItems.Count > 0)
        {
            var selectedItem = lvAgents.SelectedItems[0];
            var agentId = selectedItem.Tag as string;
            if (agentId != null) await LoadAgentByIdAsync(agentId);
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
    private async Task SaveAgentChangesAsync()
    {
        if (lvAgents.SelectedItems.Count == 0) { MessageBox.Show("Choisir d'abord un agent."); return; }
        if (!ValidateAgentForm(out var msg)) { MessageBox.Show(msg); return; }

        var selectedItem = lvAgents.SelectedItems[0];
        var agentId = selectedItem.Tag as string;
        if (agentId == null) return;

        var existingAgent = await _agentApiClient.GetByIdAsync(agentId);
        if (existingAgent == null)
        {
            MessageBox.Show("Agent introuvable.");
            return;
        }

        int? teamId = (cbTeam.SelectedItem as AgentTeamItem)?.Id;
        int? siteId = (cbSite.SelectedItem as AgentSiteItem)?.Id;

        var agent = new AgentDto(
            Idrh: tbIDRH.Text.Trim(),
            Nom: tbAgentName.Text.Trim(),
            Prenom: tbFirstName.Text.Trim(),
            Email: tbEmail.Text.Trim(),
            EquipeId: teamId,
            SiteId: siteId,
            Heberge: cbxHeberge.Checked ? 1 : 0,
            Commentaire: string.IsNullOrWhiteSpace(tbComment.Text) ? null : tbComment.Text.Trim()
        );

        // Appeler l'API pour la mise à jour
        await _agentApiClient.UpdateAsync(agentId, agent);

        // Recharger la liste pour refléter les modifications
        await LoadAgentListAsync();
    }

    private async void btnUpdate_Click(object? sender, EventArgs e)
    {
        try
        {
            await SaveAgentChangesAsync();
            MessageBox.Show("Agent modifié avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de joindre le serveur : {ex.Message}", "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification de l'agent : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Supprime l'agent (demande confirmation avant)</summary>
    private async Task DeleteSelectedAgentAsync()
    {
        if (lvAgents.SelectedItems.Count == 0)
        { MessageBox.Show("Sélectionne un agent à supprimer."); return; }

        var selectedItem = lvAgents.SelectedItems[0];
        var agentId = selectedItem.Tag as string;
        if (agentId == null) return;
        var agentLabel = $"{selectedItem.SubItems[1].Text} [{selectedItem.Text}]";
        
        var confirm = MessageBox.Show(
            $"Supprimer « {agentLabel} » ?",
            "Confirmer la suppression",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        await _agentApiClient.DeleteAsync(agentId);

        await LoadAgentListFilteredAsync(tbSearch?.Text?.Trim() ?? "");
        tbIDRH.Clear(); 
        tbAgentName.Clear(); 
        tbFirstName.Clear(); 
        tbEmail.Clear(); 
        tbComment.Clear();
        cbxHeberge.Checked = false;
    }

    private async void btnDelete_Click(object? sender, EventArgs e)
    {
        try
        {
            await DeleteSelectedAgentAsync();
            MessageBox.Show("Agent supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de joindre le serveur : {ex.Message}", "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression de l'Agent : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
