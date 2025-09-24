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
    private Button btnBack;

    public EquipmentCreateView(Action onBack)
    {
        _onBack = onBack;

        cbType = new ComboBox();
        tbName = new TextBox();
        tbCodeParc = new TextBox();
        tbSerial = new TextBox();
        tbBrand = new TextBox();
        tbComment = new TextBox();
        btnCreate = new Button();
        btnBack = new Button();

        BuildUi();
    }

    private void BuildUi()
    {
        this.Dock = DockStyle.Fill;

        btnBack = new Button { Text = "←", Left = 16, Top = 16, Width = 40, Height = 36 };
        btnBack.Click += (_, __) => _onBack?.Invoke();
        Controls.Add(btnBack);

        var f = new Font("Segoe UI", 11f, FontStyle.Regular);
        var labelFont = new Font("Segoe UI", 10f, FontStyle.Regular);


        // Button
        cbType = new ComboBox { Left = 160, Top = 160, Width = 420, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList, Font = f };
        tbName = new TextBox { Left = 720, Top = 160, Width = 420, Height = 36, Font = f };
        tbCodeParc = new TextBox { Left = 1280, Top = 160, Width = 420, Height = 36, Font = f };

        tbSerial = new TextBox { Left = 160, Top = 240, Width = 420, Height = 36, Font = f };
        tbBrand = new TextBox { Left = 720, Top = 240, Width = 420, Height = 36, Font = f };

        tbComment = new TextBox { Left = 160, Top = 320, Width = 420, Height = 160, Multiline = true, Font = f };

        btnCreate = new Button { Text = "Créer", Left = 1630, Top = 600, Width = 180, Height = 52, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };

        Controls.AddRange([cbType, tbName, tbCodeParc, tbSerial, tbBrand, tbComment, btnCreate]);

        // Labels
        var lblType     = new Label { Text = "Type",             Left = 160,  Top = 132,  Width = 420, Height = 20, Font = labelFont };
        var lblName     = new Label { Text = "Nom",              Left = 720,  Top = 132,  Width = 420, Height = 20, Font = labelFont };
        var lblCodeParc = new Label { Text = "Code parc",        Left = 1280, Top = 132,  Width = 420, Height = 20, Font = labelFont };

        var lblSerial   = new Label { Text = "Numéro de série",  Left = 160,  Top = 212,  Width = 420, Height = 20, Font = labelFont };
        var lblBrand    = new Label { Text = "Marque",           Left = 720,  Top = 212,  Width = 420, Height = 20, Font = labelFont };

        var lblComment  = new Label { Text = "Commentaire",      Left = 160,  Top = 296,  Width = 420, Height = 20, Font = labelFont };

        Controls.AddRange([lblType, lblName, lblCodeParc, lblSerial, lblBrand, lblComment]);

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