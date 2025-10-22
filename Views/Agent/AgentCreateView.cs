using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;

namespace ProjetParc.Views.Agent;

/// <summary>
/// Vue de création d'un agent. Permet de saisir les informations personnelles,
/// sélectionner le site et l'équipe, puis d'insérer l'agent en base.
/// </summary>
public class AgentCreateView : UserControl
{
    private readonly Action _onBack;
    private TextBox tbIDRH;
    private TextBox tbAgentName;
    private TextBox tbFirstName;
    private TextBox tbEmail;
    private ComboBox cbTeam;
    private CheckBox cbxHeberge;
    private TextBox tbComment;
    private ComboBox cbSite;
    private Button btnCreate;

    /// <summary>
    /// Constructeur : initialise l'UI et charge les listes (sites, équipes).
    /// </summary>
    /// <param name="onBack">Callback pour revenir à la vue précédente.</param>
    public AgentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadAgentSite();
        LoadAgentTeam();

        btnCreate.Click += btnCreate_Click;
    }

    /// <summary>
    /// Construit l'interface utilisateur (champs, labels, boutons) pour la création d'agent.
    /// </summary>
    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 11f, FontStyle.Regular);

        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 45),  // Bouton retour
                new RowStyle(SizeType.Percent, 100),  // Formulaire
                new RowStyle(SizeType.Absolute, 80)   // Bouton créer
            }
        };
        Controls.Add(mainLayout);

        // Bouton retour
        var btnBack = new Button { Text = "← Retour", Width = 120, Height = 36, Anchor = AnchorStyles.Left };
        btnBack.Click += (_, __) => _onBack?.Invoke();
        mainLayout.Controls.Add(btnBack, 0, 0);

        // Panel du formulaire
        TableLayoutPanel formLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 8,
            Padding = new Padding(10),
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 33.33f),
                new ColumnStyle(SizeType.Percent, 33.33f),
                new ColumnStyle(SizeType.Percent, 33.33f)
            }
        };
        mainLayout.Controls.Add(formLayout, 0, 1);

        // Première ligne : IDRH, Nom, Prénom
        AddFormRow(formLayout, 0, "IDRH", tbIDRH = new TextBox { Height = 36 });
        AddFormRow(formLayout, 0, "Nom", tbAgentName = new TextBox { Height = 36 }, 1);
        AddFormRow(formLayout, 0, "Prénom", tbFirstName = new TextBox { Height = 36 }, 2);

        // Deuxième ligne : Email, Équipe, Hébergé
        AddFormRow(formLayout, 2, "Email", tbEmail = new TextBox { Height = 36 });
        AddFormRow(formLayout, 2, "Équipe", cbTeam = new ComboBox { Height = 36, DropDownStyle = ComboBoxStyle.DropDownList }, 1);
        
        var hebergePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var lblHeb = new Label { Text = "Hébergé", AutoSize = true, Padding = new Padding(0, 5, 10, 0) };
        cbxHeberge = new CheckBox { AutoSize = true };
        hebergePanel.Controls.AddRange(new Control[] { lblHeb, cbxHeberge });
        formLayout.Controls.Add(hebergePanel, 2, 2);
        formLayout.SetRowSpan(hebergePanel, 2);

        // Troisième ligne : Commentaire (2 colonnes), Site
        AddFormRow(formLayout, 4, "Commentaire", tbComment = new TextBox { Height = 160, Multiline = true }, 0, 2);
        AddFormRow(formLayout, 4, "Site", cbSite = new ComboBox { Height = 36, DropDownStyle = ComboBoxStyle.DropDownList }, 2);

        // Bouton créer (en bas)
        btnCreate = new Button
        {
            Text = "Créer",
            Height = 52,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Dock = DockStyle.Right,
            Width = 180
        };
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

    /// <summary>Convertit un bool en int (0/1) pour stockage en base.</summary>
    private static int ToBit(bool b) => b ? 1 : 0;

    /// <summary>Convertit une chaîne vide en DBNull pour insertion SQL.</summary>
    private static object ToDbNullable(string s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();

    
    private sealed class AgentSiteItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }
    private sealed class AgentTeamItem { public int Id { get; set; } public string Name { get; set; } = ""; public override string ToString() => Name; }

    /// <summary>Charge la liste des sites depuis la table <c>Sites</c>.</summary>
    private void LoadAgentSite()
    {
        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT id, name FROM Sites ORDER BY name;";

        using var reader = command.ExecuteReader();
        var items = new List<AgentSiteItem>();
        while (reader.Read())
            items.Add(new AgentSiteItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });

        cbSite.DataSource = items;
        cbSite.DisplayMember = nameof(AgentSiteItem.Name);
        cbSite.ValueMember   = nameof(AgentSiteItem.Id);
    }

    /// <summary>Charge la liste des équipes depuis la table <c>Equipes</c>.</summary>
    private void LoadAgentTeam()
    {
        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT id, name FROM Equipes ORDER BY name;";

        using var reader = command.ExecuteReader();
        var items = new List<AgentTeamItem>();
        while (reader.Read())
            items.Add(new AgentTeamItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });

        cbTeam.DataSource = items;
        cbTeam.DisplayMember = nameof(AgentTeamItem.Name);
        cbTeam.ValueMember   = nameof(AgentTeamItem.Id);
    }

    /// <summary>Valide le formulaire de création d'agent (champs obligatoires).</summary>
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

    /// <summary>Insère l'agent dans la base et effectue les insert/delete nécessaires pour la table Travail.</summary>
    private void InsertAgent()
    {
        if (!ValidateTeamForm(out var errorMessage)) { MessageBox.Show(errorMessage); return; }

        var siteId = ((AgentSiteItem)cbSite.SelectedItem).Id;
        var teamId = ((AgentTeamItem)cbTeam.SelectedItem).Id;
        var hebergeValue = ToBit(cbxHeberge.Checked);

        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT INTO ""Agents""
                (idrh, nom, prenom, email, equipe_id, heberge, commentaire, site_id)
            VALUES ($idrh, $nom, $prenom, $email, $equipeId, $heberge, $comment, $siteId);";

        command.Parameters.AddWithValue("$idrh", tbIDRH.Text.Trim());
        command.Parameters.AddWithValue("$nom", tbAgentName.Text.Trim());
        command.Parameters.AddWithValue("$prenom", tbFirstName.Text.Trim());
        command.Parameters.AddWithValue("$email", tbEmail.Text.Trim());
        command.Parameters.AddWithValue("$equipeId", teamId);
        command.Parameters.AddWithValue("$heberge", hebergeValue);
        command.Parameters.AddWithValue("$comment", ToDbNullable(tbComment.Text));
        command.Parameters.AddWithValue("$siteId", siteId);

        try
        {
            command.ExecuteNonQuery();
            MessageBox.Show("Agent créé");

            using (var tx = connexion.BeginTransaction())
            {
                using (var deleteCmd = connexion.CreateCommand())
                {
                    deleteCmd.Transaction = tx;
                    deleteCmd.CommandText = @"DELETE FROM ""Travail"" WHERE idrh = $idrh;";
                    deleteCmd.Parameters.AddWithValue("$idrh", tbIDRH.Text.Trim());
                    deleteCmd.ExecuteNonQuery();
                }
                using (var insertCmd = connexion.CreateCommand())
                {
                    insertCmd.Transaction = tx;
                    insertCmd.CommandText = @"INSERT INTO ""Travail"" (idrh, site_id) VALUES ($idrh, $site_id);";
                    insertCmd.Parameters.AddWithValue("$idrh", tbIDRH.Text.Trim());
                    insertCmd.Parameters.AddWithValue("$site_id", siteId);
                    insertCmd.ExecuteNonQuery();
                }
                tx.Commit();
            }

            // reset UI
            tbIDRH.Clear(); tbFirstName.Clear(); tbAgentName.Clear(); tbEmail.Clear(); tbComment.Clear();
            if (cbSite.Items.Count > 0) cbSite.SelectedIndex = 0;
            if (cbTeam.Items.Count > 0) cbTeam.SelectedIndex = 0;
            cbxHeberge.Checked = false;
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Erreur SQL : " + ex.Message);
        }
    }

    /// <summary>Gestionnaire du clic sur le bouton Créer : délègue vers <see cref="InsertAgent"/>.</summary>
    private void btnCreate_Click(object sender, EventArgs e) => InsertAgent();

    /// <summary>
    /// Ajoute une ligne de formulaire avec un label et un contrôle dans le TableLayoutPanel
    /// </summary>
    private void AddFormRow(TableLayoutPanel panel, int row, string labelText, Control control, int col = 0, int colSpan = 1)
    {
        var label = new Label 
        { 
            Text = labelText, 
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, 5)
        };
        panel.Controls.Add(label, col, row);

        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 0, 10, 10);
        panel.Controls.Add(control, col, row + 1);
        if (colSpan > 1)
        {
            panel.SetColumnSpan(control, colSpan);
        }
    }
}
