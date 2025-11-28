using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestiParc.Ui.Data;
using GestiParc.Ui.Services;
using GestiParc.Infrastructure.Data.Repositories;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Views.Agent;

/// <summary>
/// Formulaire pour créer un nouvel agent. On remplit tous les champs (IDRH, nom, prénom, email...)
/// et on sauvegarde dans la base MySQL (table Agents avec son site via site_id)
/// </summary>
public class AgentCreateView : UserControl
{
    private readonly Action _onBack;
    private TextBox tbIDRH = null!;
    private TextBox tbAgentName = null!;
    private TextBox tbFirstName = null!;
    private TextBox tbEmail = null!;
    private ComboBox cbTeam = null!;
    private CheckBox cbxHeberge = null!;
    private TextBox tbComment = null!;
    private ComboBox cbSite = null!;
    private Button btnCreate = null!;

    /// <summary>
    /// Constructeur : monte toute l'interface et charge les listes déroulantes (sites et équipes)
    /// </summary>
    /// <param name="onBack">Callback pour retourner à l'écran précédent</param>
    public AgentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadAgentSite();
        LoadAgentTeam();

        btnCreate.Click += btnCreate_Click;
    }

    /// <summary>
    /// Construit toute l'interface - bouton retour en haut, formulaire au milieu avec 3 colonnes,
    /// bouton Créer en bas. Le formulaire a 8 champs (IDRH, nom, prénom, email, équipe, hébergé, commentaire, site)
    /// </summary>
    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;

        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(Theme.Spacing.Large),
            BackColor = Theme.Colors.Background,
            RowStyles = {
                new RowStyle(SizeType.Absolute, 60),  // Bouton retour
                new RowStyle(SizeType.Percent, 100),  // Formulaire
                new RowStyle(SizeType.Absolute, 80)   // Bouton créer
            }
        };
        Controls.Add(mainLayout);

        // Bouton retour
        var btnBack = new Button 
        { 
            Text = "← Retour", 
            Width = Theme.Sizes.ButtonWidth, 
            Height = Theme.Sizes.ButtonHeightLarge, 
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0),
            Font = Theme.Fonts.Button
        };
        Theme.StyleOutlineButton(btnBack);
        btnBack.Click += (_, __) => _onBack?.Invoke();
        mainLayout.Controls.Add(btnBack, 0, 0);

        // Panel du formulaire
        TableLayoutPanel formLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 8,
            Padding = new Padding(Theme.Spacing.Large),
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
                new RowStyle(SizeType.Absolute, 10)   // Espacement
            }
        };
        mainLayout.Controls.Add(formLayout, 0, 1);

        // Première ligne : IDRH, Nom, Prénom
        tbIDRH = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbIDRH);
        AddFormRow(formLayout, 0, "IDRH", tbIDRH);
        
        tbAgentName = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbAgentName);
        AddFormRow(formLayout, 0, "Nom", tbAgentName, 1);
        
        tbFirstName = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbFirstName);
        AddFormRow(formLayout, 0, "Prénom", tbFirstName, 2);

        // Deuxième ligne : Email, Équipe, Hébergé
        tbEmail = new TextBox { Height = Theme.Sizes.InputHeight };
        Theme.StyleTextBox(tbEmail);
        AddFormRow(formLayout, 2, "Email", tbEmail);
        
        cbTeam = new ComboBox { Height = Theme.Sizes.InputHeight, DropDownStyle = ComboBoxStyle.DropDownList };
        Theme.StyleComboBox(cbTeam);
        AddFormRow(formLayout, 2, "Équipe", cbTeam, 1);
        
        var hebergePanel = new FlowLayoutPanel 
        { 
            Dock = DockStyle.Fill, 
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Theme.Colors.Surface
        };
        var lblHeb = new Label 
        { 
            Text = "Hébergé", 
            AutoSize = true, 
            Padding = new Padding(0, 5, 10, 0),
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary
        };
        cbxHeberge = new CheckBox { AutoSize = true };
        hebergePanel.Controls.AddRange(new Control[] { lblHeb, cbxHeberge });
        formLayout.Controls.Add(hebergePanel, 2, 2);
        formLayout.SetRowSpan(hebergePanel, 2);

        // Troisième ligne : Commentaire (2 colonnes), Site
        tbComment = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };
        Theme.StyleTextBox(tbComment);
        AddFormRow(formLayout, 4, "Commentaire", tbComment, 0, 2);
        
        cbSite = new ComboBox { Height = Theme.Sizes.InputHeight, DropDownStyle = ComboBoxStyle.DropDownList };
        Theme.StyleComboBox(cbSite);
        AddFormRow(formLayout, 4, "Site", cbSite, 2);

        // Bouton créer (en bas)
        btnCreate = new Button
        {
            Text = "Créer",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Right,
            Width = Theme.Sizes.ButtonWidth
        };
        Theme.StylePrimaryButton(btnCreate);
        mainLayout.Controls.Add(btnCreate, 0, 2);

        // Définir l'ordre de tabulation
        tbIDRH.TabIndex = 0;
        tbAgentName.TabIndex = 1;
        tbFirstName.TabIndex = 2;
        tbEmail.TabIndex = 3;
        cbTeam.TabIndex = 4;
        cbxHeberge.TabIndex = 5;
        tbComment.TabIndex = 6;
        cbSite.TabIndex = 7;
        btnCreate.TabIndex = 8;

        
    }

    private sealed class AgentSiteItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }
    private sealed class AgentTeamItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }

    /// <summary>Remplit la liste déroulante des sites à partir de la table Sites</summary>
    private void LoadAgentSite()
    {
        try
        {
            var repo = new SiteMySqlRepository();
            var sites = repo.GetAll();

            var items = new List<AgentSiteItem>();
            foreach (var site in sites)
            {
                items.Add(new AgentSiteItem { Id = site.Id, Name = site.Name });
            }

            cbSite.DataSource = items;
            cbSite.DisplayMember = nameof(AgentSiteItem.Name);
            cbSite.ValueMember   = nameof(AgentSiteItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des sites : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Remplit la liste déroulante des équipes à partir de la table Equipes</summary>
    private void LoadAgentTeam()
    {
        try
        {
            var repo = new EquipeMySqlRepository();
            var equipes = repo.GetAll();

            var items = new List<AgentTeamItem>();
            foreach (var equipe in equipes)
            {
                items.Add(new AgentTeamItem { Id = equipe.Id, Name = equipe.Name });
            }

            cbTeam.DataSource = items;
            cbTeam.DisplayMember = nameof(AgentTeamItem.Name);
            cbTeam.ValueMember   = nameof(AgentTeamItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipes : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Vérifie que tous les champs obligatoires sont bien remplis avant de créer l'agent</summary>
    private bool ValidateTeamForm(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(tbAgentName.Text)) { errorMessage = "Le nom de l'agent est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbFirstName.Text)) { errorMessage = "Le prénom de l'agent est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbIDRH.Text)) { errorMessage = "L'IDRH de l'agent est obligatoire."; return false; }
        if (string.IsNullOrWhiteSpace(tbEmail.Text)) { errorMessage = "L'Email de l'agent est obligatoire."; return false; }
        if (cbSite.SelectedItem is not AgentSiteItem) { errorMessage = "Sélectionner un site."; return false; }
        if (cbTeam.SelectedItem is not AgentTeamItem) { errorMessage = "Sélectionner une équipe."; return false; }
        errorMessage = ""; return true;
    }

    /// <summary>
    /// Sauvegarde l'agent en base de données avec son site directement via agents.site_id
    /// </summary>
    private void InsertAgent()
    {
        if (!ValidateTeamForm(out var errorMessage)) { MessageBox.Show(errorMessage); return; }

        var siteId = ((AgentSiteItem?)cbSite.SelectedItem)?.Id ?? 0;
        var teamId = ((AgentTeamItem?)cbTeam.SelectedItem)?.Id ?? 0;
        var hebergeValue = cbxHeberge.Checked ? 1 : 0;

        try
        {
            // Créer le DTO pour l'agent
            var agent = new AgentDto(
                Idrh: tbIDRH.Text.Trim(),
                Nom: tbAgentName.Text.Trim(),
                Prenom: tbFirstName.Text.Trim(),
                Email: tbEmail.Text.Trim(),
                EquipeId: teamId,
                SiteId: siteId,
                Heberge: hebergeValue,
                Commentaire: string.IsNullOrWhiteSpace(tbComment.Text) ? null : tbComment.Text.Trim()
            );

            // Insérer l'agent via le repository
            var agentRepo = new AgentMySqlRepository();
            agentRepo.Insert(agent);

            MessageBox.Show("Agent créé");

            // Reset UI
            tbIDRH.Clear(); 
            tbFirstName.Clear(); 
            tbAgentName.Clear(); 
            tbEmail.Clear(); 
            tbComment.Clear();
            if (cbSite.Items.Count > 0) cbSite.SelectedIndex = 0;
            if (cbTeam.Items.Count > 0) cbTeam.SelectedIndex = 0;
            cbxHeberge.Checked = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la création de l'agent : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Handler du bouton Créer - appelle InsertAgent()</summary>
    private void btnCreate_Click(object? sender, EventArgs e) => InsertAgent();

    /// <summary>
    /// Méthode utilitaire pour ajouter un champ dans le formulaire (label + contrôle)
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
