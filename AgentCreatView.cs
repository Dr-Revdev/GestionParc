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
        BuildUiAgentCreateView();
    }

    private void BuildUiAgentCreateView()
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

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///Tempo pour test///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        cbTeam.Items.AddRange(["Infra", "Support", "Dev", "Réseau"]);
        if (cbTeam.Items.Count > 0) cbTeam.SelectedIndex = 0;

        cbSite.Items.AddRange(["Siège", "Agence A", "Agence B"]);
        if (cbSite.Items.Count > 0) cbSite.SelectedIndex = 0;
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///Tempo pour test///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

}

