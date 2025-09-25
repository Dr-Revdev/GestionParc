using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetParc.Views;

public class AgentEditView : UserControl
{
    private TextBox tbSearch;
    private Button btnSearch;
    private ListBox lbAgents;

    private TextBox tbIDRH, tbAgentName, tbFirstName, tbEmail, tbComment;
    private ComboBox cbTeam, cbSite;
    private CheckBox cbxHeberge;

    private Button btnUpdate, btnDelete;

    private readonly Action _onBack;

    public AgentEditView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 11f);

        SuspendLayout();

        var btnBack = new Button { Text = "← Retour", Left = 24, Top = 24, Width = 120, Height = 36 };
        btnBack.Click += (_, __) => _onBack?.Invoke();
        Controls.Add(btnBack);

        tbSearch = new TextBox { Left = 80, Top = 100, Width = 520, Height = 32 };
        btnSearch = new Button { Left = 610, Top = 100, Width = 36, Height = 32, Text = "🔍" };

        lbAgents = new ListBox { Left = 80, Top = 160, Width = 520, Height = 460 };

        Controls.AddRange([tbSearch, btnSearch, lbAgents]);

        Controls.Add(new Panel { Left = 680, Top = 80, Width = 2, Height = 620, BackColor = Color.Silver });

        tbIDRH = new TextBox { Left = 760, Top = 160, Width = 320, Height = 36 };
        tbAgentName = new TextBox { Left = 1140, Top = 160, Width = 320, Height = 36 };
        tbFirstName = new TextBox { Left = 1520, Top = 160, Width = 320, Height = 36 };

        tbEmail = new TextBox { Left = 760, Top = 240, Width = 320, Height = 36 };
        cbTeam = new ComboBox { Left = 1140, Top = 240, Width = 320, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };
        cbxHeberge = new CheckBox { Left = 1680, Top = 246, Width = 24, Height = 24 };

        tbComment = new TextBox { Left = 760, Top = 320, Width = 320, Height = 160, Multiline = true };
        cbSite = new ComboBox { Left = 1520, Top = 320, Width = 320, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };

        Controls.AddRange([tbIDRH, tbAgentName, tbFirstName, tbEmail, cbTeam, cbxHeberge, tbComment, cbSite]);

        Controls.AddRange
        ([
            new Label { Text = "IDRH", Left = 760,  Top = 132, Width = 320, Height = 20 },
            new Label { Text = "Nom", Left = 1140, Top = 132, Width = 320, Height = 20 },
            new Label { Text = "Prénom", Left = 1520, Top = 132, Width = 320, Height = 20 },
            new Label { Text = "Email", Left = 760,  Top = 212, Width = 320, Height = 20 },
            new Label { Text = "Équipe", Left = 1140, Top = 212, Width = 320, Height = 20 },
            new Label { Text = "Hébergé", Left = 1520, Top = 212, Width = 320, Height = 20 },
            new Label { Text = "Commentaire", Left = 760,  Top = 296, Width = 320, Height = 20 },
            new Label { Text = "Site", Left = 1520, Top = 296, Width = 320, Height = 20 }
        ]);

        btnUpdate = new Button { Text = "Modifier", Left = 1570, Top = 600, Width = 120, Height = 40 };
        btnDelete = new Button { Text = "Supprimer", Left = 1700, Top = 600, Width = 120, Height = 40 };
        Controls.AddRange([btnUpdate, btnDelete]);

        ResumeLayout(false);

        btnSearch.Click += (_, __) => { /* TODO: filtrer lbAgents avec tbSearch.Text */ };
        lbAgents.SelectedIndexChanged += (_, __) => { /* TODO: charger la fiche */ };
        btnUpdate.Click += (_, __) => { /* TODO: maj */ };
        btnDelete.Click += (_, __) => { /* TODO: suppression */ };

        cbTeam.Items.AddRange(["Infra", "Support", "Dev"]);
        if (cbTeam.Items.Count > 0) cbTeam.SelectedIndex = 0;

        cbSite.Items.AddRange(["Siège", "Agence A"]);
        if (cbSite.Items.Count > 0) cbSite.SelectedIndex = 0;

        lbAgents.Items.AddRange(["Dupont, Jean", "Martin, Léa", "Durand, Paul"]);



    }
}
