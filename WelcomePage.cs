using System;
using System.Drawing;  
using System.Windows.Forms;
using ProjetParc.Views;  

namespace ProjetParc;

public class WelcomePage : Form
{
    private Panel content;
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

        content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        Controls.Add(content);

        var tileFont = new Font("Segoe UI", 16f, FontStyle.Bold);

        btnSetEquipment = new Button { Text = "Equipements en place", Left = 64, Top = 390, Width = 576, Height = 300, Font = tileFont };
        btnFreeEquipment = new Button { Text = "Equipements disponibles", Left = 672, Top = 390, Width = 576, Height = 300, Font = tileFont };
        btnNewMod = new Button { Text = "Modification / Création", Left = 1280, Top = 390, Width = 576, Height = 300, Font = tileFont };

        title = new Label { Text = "Gestion de Parc", Font = new Font("Segoe UI", 35f, FontStyle.Bold), Left = 0, Top = 230, Width = 1920, Height = 80, TextAlign = ContentAlignment.MiddleCenter };
        Controls.Add(title);

        btnNewMod.Click += (_, __) => ShowAdminMenu();

        ShowHome();

    }

    private void ShowHome()
    {
        content.Controls.Clear();

        content.Controls.AddRange([btnSetEquipment, btnFreeEquipment, btnNewMod]);

    }

    private void ShowAdminMenu()
    {
        content.Controls.Clear();

        var admin = new AdminMenuView(onBack: ShowHome, onCreateEquipment: ShowEquipmentCreate, onCreateAgent: ShowAgentCreate, onEditAgent: ShowAgentEdit, onEditEquipment: ShowEquipmentEdit);
        admin.Dock = DockStyle.Fill;
        content.Controls.Add(admin);
    }
    private void ShowEquipmentCreate()
    {
        content.Controls.Clear();
        content.Controls.Add(new EquipmentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }

    private void ShowAgentCreate()
    {
        content.Controls.Clear();
        content.Controls.Add(new AgentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
    private void ShowAgentEdit()
    {
        content.Controls.Clear();
        content.Controls.Add(new AgentEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
    private void ShowEquipmentEdit()
    {
        content.Controls.Clear();
        content.Controls.Add(new EquipementEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
    }
}
