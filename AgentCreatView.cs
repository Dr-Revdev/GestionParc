using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetParc.Views;

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

    public AgentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadAgentSite();
        LoadAgentTeam();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;

        SuspendLayout();
        var btnBack = new Button { Text = "← Retour", Left = 24, Top = 24, Width = 120, Height = 36 };
        btnBack.Click += (_, __) => _onBack?.Invoke();
        Controls.Add(btnBack);

        tbIDRH = new TextBox { Left = 160, Top = 160, Width = 420, Height = 36 };
        tbAgentName = new TextBox { Left = 720, Top = 160, Width = 420, Height = 36 };
        tbFirstName = new TextBox { Left = 1280, Top = 160, Width = 420, Height = 36 };

        tbEmail = new TextBox { Left = 160, Top = 240, Width = 420, Height = 36 };
        cbTeam = new ComboBox { Left = 720, Top = 240, Width = 420, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };
        cbxHeberge = new CheckBox { Left = 1480, Top = 246, Width = 24, Height = 24 };

        tbComment = new TextBox { Left = 160, Top = 320, Width = 420, Height = 160, Multiline = true };
        cbSite = new ComboBox { Left = 1280, Top = 320, Width = 420, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };

        btnCreate = new Button { Text = "Créer", Left = 1630, Top = 600, Width = 180, Height = 52, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };
        Controls.AddRange([tbIDRH, tbAgentName, tbFirstName, tbEmail, cbTeam, cbxHeberge, tbComment, cbSite, btnCreate]);

        // Libellés (locaux)
        var lblIDRH = new Label { Text = "IDRH", Left = 160, Top = 132, Width = 420, Height = 20 };
        var lblNom = new Label { Text = "Nom", Left = 720, Top = 132, Width = 420, Height = 20 };
        var lblPrenom = new Label { Text = "Prénom", Left = 1280, Top = 132, Width = 420, Height = 20 };

        var lblEmail = new Label { Text = "Email", Left = 160, Top = 212, Width = 420, Height = 20 };
        var lblEquipe = new Label { Text = "Équipe", Left = 720, Top = 212, Width = 420, Height = 20 };
        var lblHeb = new Label { Text = "Hébergé", Left = 1280, Top = 212, Width = 180, Height = 20 };

        var lblCom = new Label { Text = "Commentaire", Left = 160, Top = 296, Width = 420, Height = 20 };
        var lblSite = new Label { Text = "Site", Left = 1280, Top = 296, Width = 420, Height = 20 };
        Controls.AddRange([lblIDRH, lblNom, lblPrenom, lblEmail, lblEquipe, lblHeb, lblCom, lblSite]);

        ResumeLayout(false);

        // Tab order (gauche→droite, haut→bas)
        tbIDRH.TabIndex = 0; tbAgentName.TabIndex = 1; tbFirstName.TabIndex = 2; tbEmail.TabIndex = 3;
        cbTeam.TabIndex = 4; cbxHeberge.TabIndex = 5; tbComment.TabIndex = 6; cbSite.TabIndex = 7; btnCreate.TabIndex = 8;

    }

    private sealed class AgentSiteItem
    {
        public string siteName { get; set; }
        public override string ToString() => siteName;
    }

    private sealed class AgentTeamItem
    {
        public string teamName { get; set; }
        public override string ToString() => teamName;
    }

    private void LoadAgentSite()
    {
        using var connection = Database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT nom_site FROM Sites;";

        using var reader = command.ExecuteReader();
        var agentSiteItems = new List<AgentSiteItem>();
        while (reader.Read())
        {
            var siteItem = new AgentSiteItem
            {
                siteName = reader.GetString(0)
            };
            agentSiteItems.Add(siteItem);
        }

        cbSite.DataSource = agentSiteItems;
        cbSite.DisplayMember = nameof(AgentSiteItem.siteName);
        cbSite.ValueMember = nameof(AgentSiteItem.siteName);
    }

    private void LoadAgentTeam()
    {
        using var connection = Database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT nom_equipe FROM Equipes;";

        using var reader = command.ExecuteReader();
        var agentTeamItems = new List<AgentTeamItem>();
        while (reader.Read())
        {
            var teamItem = new AgentTeamItem
            {
                teamName = reader.GetString(0)
            };
            agentTeamItems.Add(teamItem);
        }

        cbTeam.DataSource = agentTeamItems;
        cbTeam.DisplayMember = nameof(AgentTeamItem.teamName);
        cbTeam.ValueMember = nameof(AgentTeamItem.teamName);
    }

    private bool ValidateTeamForm(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(tbAgentName.Text))
        {
            errorMessage = "Le nom de l'agent est obligatoire.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(tbFirstName.Text))
        {
            errorMessage = "Le prénom de l'agent est obligatoire.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(tbIDRH.Text))
        {
            errorMessage = "L'IDRH de l'agent est obligatoire.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(tbEmail.Text))
        {
            errorMessage = "L'Email de l'agent est obligatoire.";
            return false;
        }
        if (cbSite.SelectedItem is not AgentSiteItem)
        {
            errorMessage = "Sélectionner un site.";
            return false;
        }
        if (cbTeam.SelectedItem is not AgentTeamItem)
        {
            errorMessage = "Sélectionner une équipe.";
            return false;
        }

        errorMessage = "";
        return true;
    }
    private static object ToDbNullable(string s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();

    private void InsertAgent()
    {
        if (!ValidateTeamForm(out var errorMessage))
        {
            MessageBox.Show(errorMessage);
            return;
        }
    }

}

