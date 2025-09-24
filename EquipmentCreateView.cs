using System.Drawing;
using System.Windows.Forms;

namespace ProjetParc.Views;

public class EquipmentCreateView : UserControl
{
    private readonly Action _onBack;

    private ComboBox cbType;
    private TextBox tbName;
    private TextBox tbCodeParc;
    private TextBox tbSerial;
    private TextBox tbBrand;
    private TextBox tbComment;
    private Button btnCreate;

    public EquipmentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUiEquipmentCreateView();
    }

    private void BuildUiEquipmentCreateView()
    {

        Dock = DockStyle.Fill;

        var btnBack = new Button { Text = "← Retour", Left = 24, Top = 24, Width = 120, Height = 36 };
        btnBack.Click += (_, __) => _onBack?.Invoke();
        Controls.Add(btnBack);

        Font = new Font("Segoe UI", 11f, FontStyle.Regular);


        // Button
        cbType = new ComboBox { Left = 160, Top = 160, Width = 420, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };
        tbName = new TextBox { Left = 720, Top = 160, Width = 420, Height = 36 };
        tbCodeParc = new TextBox { Left = 1280, Top = 160, Width = 420, Height = 36 };

        tbSerial = new TextBox { Left = 160, Top = 240, Width = 420, Height = 36 };
        tbBrand = new TextBox { Left = 720, Top = 240, Width = 420, Height = 36 };

        tbComment = new TextBox { Left = 160, Top = 320, Width = 420, Height = 160, Multiline = true };

        btnCreate = new Button { Text = "Créer", Left = 1630, Top = 600, Width = 180, Height = 52, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };

        Controls.AddRange([cbType, tbName, tbCodeParc, tbSerial, tbBrand, tbComment, btnCreate]);

        // Labels
        var lblType = new Label { Text = "Type", Left = 160, Top = 132, Width = 420, Height = 20 };
        var lblName = new Label { Text = "Nom", Left = 720, Top = 132, Width = 420, Height = 20 };
        var lblCodeParc = new Label { Text = "Code parc", Left = 1280, Top = 132, Width = 420, Height = 20 };

        var lblSerial = new Label { Text = "Numéro de série", Left = 160, Top = 212, Width = 420, Height = 20 };
        var lblBrand = new Label { Text = "Marque", Left = 720, Top = 212, Width = 420, Height = 20 };

        var lblComment = new Label { Text = "Commentaire", Left = 160, Top = 296, Width = 420, Height = 20 };

        Controls.AddRange([lblType, lblName, lblCodeParc, lblSerial, lblBrand, lblComment]);

        cbType.TabIndex = 0; tbName.TabIndex = 1; tbCodeParc.TabIndex = 2; tbSerial.TabIndex = 3; tbBrand.TabIndex = 4; tbComment.TabIndex = 5; btnCreate.TabIndex = 6;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///Tempo pour test///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        cbType.Items.AddRange(["PC", "Ecran", "Imprimante", "Dock", "Autre"]);
        if (cbType.Items.Count > 0) cbType.SelectedIndex = 0; 

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                    ///Tempo pour test///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}