using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetParc.Views;

public class EquipementEditView : UserControl
{
    private TextBox tbSearch;
    private Button btnSearch;
    private ListBox lbEquipment;
    private TextBox tbSerialNumber, tbName, tbBrand, tbCodeParc, tbComment;
    private ComboBox cbType;

    private Button btnUpdate, btnDelete;

    private readonly Action _onBack;

    public EquipementEditView(Action onBack)
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
        lbEquipment = new ListBox { Left = 80, Top = 160, Width = 520, Height = 460 };

        Controls.AddRange([tbSearch, btnSearch, lbEquipment]);

        Controls.Add(new Panel { Left = 680, Top = 80, Width = 2, Height = 620, BackColor = Color.Silver });

        cbType = new ComboBox { Left = 760, Top = 160, Width = 320, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };
        tbName = new TextBox { Left = 1140, Top = 160, Width = 320, Height = 36 };
        tbCodeParc = new TextBox { Left = 1520, Top = 160, Width = 320, Height = 36 };

        tbSerialNumber = new TextBox { Left = 760, Top = 240, Width = 320, Height = 36 };
        tbBrand = new TextBox { Left = 1140, Top = 240, Width = 320, Height = 36 };

        tbComment = new TextBox { Left = 760, Top = 320, Width = 320, Height = 160, Multiline = true };

        Controls.AddRange([cbType, tbName, tbCodeParc, tbSerialNumber, tbBrand, tbComment]);

        Controls.AddRange
        ([
            new Label { Text = "Type",             Left = 760,  Top = 132, Width = 320, Height = 20 },
            new Label { Text = "Nom",              Left = 1140, Top = 132, Width = 320, Height = 20 },
            new Label { Text = "Code parc",        Left = 1520, Top = 132, Width = 320, Height = 20 },

            new Label { Text = "Numéro de série",  Left = 760,  Top = 212, Width = 320, Height = 20 },
            new Label { Text = "Marque",           Left = 1140, Top = 212, Width = 320, Height = 20 },

            new Label { Text = "Commentaire",      Left = 760,  Top = 296, Width = 320, Height = 20 }
        ]);

        btnUpdate = new Button { Text = "Modifier", Left = 1570, Top = 600, Width = 120, Height = 40 };
        btnDelete = new Button { Text = "Supprimer", Left = 1700, Top = 600, Width = 120, Height = 40 };
        Controls.AddRange([btnUpdate, btnDelete]);

        ResumeLayout(false);

        btnSearch.Click += (_, __) => { };
        lbEquipment.SelectedIndexChanged += (_, __) => { };
        btnUpdate.Click += (_, __) => { };
        btnDelete.Click += (_, __) => { };

        cbType.Items.AddRange(["PC", "Ecran", "Imprimante", "Dock", "Autre"]);
        if (cbType.Items.Count > 0) cbType.SelectedIndex = 0;

        lbEquipment.Items.AddRange(["Equipement 1", "Equipement 2", "Equipement 3"]);
    }
}