using System;
using System.Windows.Forms;

namespace ProjetParc;

public class WelcomePage : Form
{
    private Button btnSetEquipment;
    private Button btnFreeEquipment;
    private Button btnNewMod;
    private Label title;
    public WelcomePage()
    {
        Text = "Gestion Parc";
        ClientSize = new Size(1920, 1080);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;

        var tileFont = new Font("Segoe UI", 16f, FontStyle.Bold);

        title = new Label { Text = "Gestion de Parc", Font = new Font("Segoe UI", 35f, FontStyle.Bold), Left = 0, Top = 230, Width = 1920, Height = 80, TextAlign = ContentAlignment.MiddleCenter };
        Controls.Add(title);

        btnSetEquipment = new Button { Text = "Equipements en place", Left = 64, Top = 390, Width = 576, Height = 300, Font = tileFont };
        btnFreeEquipment = new Button { Text = "Equipements disponibles", Left = 672, Top = 390, Width = 576, Height = 300, Font = tileFont };
        btnNewMod = new Button { Text = "Modification / Création", Left = 1280, Top = 390, Width = 576, Height = 300, Font = tileFont };
        Controls.AddRange(new Control[] { btnSetEquipment, btnFreeEquipment, btnNewMod });
    }
}
