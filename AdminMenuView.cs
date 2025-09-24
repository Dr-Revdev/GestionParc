using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetParc.Views
{
    public class AdminMenuView : UserControl
    {
        private readonly Action _onBack;
        private readonly Action _onCreateEquipment;
        private Button btnCreateEquipment;
        private Button btnCreateAgent;
        private Button btnModificationEquipment;
        private Button btnModificationAgent;
        private Button btnExange;

        public AdminMenuView(Action onBack, Action onCreateEquipment)
        {
            var tileFont = new Font("Segoe UI", 16f, FontStyle.Bold);

            // Button Back and title label
            _onBack = onBack;
            

            var btnBack = new Button { Text = "← Retour", Left = 24, Top = 24, Width = 120, Height = 36 };
            btnBack.Click += (_, __) => _onBack?.Invoke();

            var title = new Label { Text = "Menu modification / création", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Left = 160, Top = 28 };

            Controls.Add(btnBack);
            Controls.Add(title);

            // Button nav
            btnCreateEquipment = new Button { Text = "Création Equipement", Left = 256, Top = 180, Width = 420, Height = 180, Font = tileFont };
            btnCreateAgent = new Button { Text = "Création Agent", Left = 1244, Top = 180, Width = 420, Height = 180, Font = tileFont };

            btnModificationEquipment = new Button { Text = "Modification Equipement", Left = 256, Top = 408, Width = 420, Height = 180, Font = tileFont };
            btnModificationAgent = new Button { Text = "Modification agent", Left = 1244, Top = 408, Width = 420, Height = 180, Font = tileFont };

            btnExange = new Button { Text = "Echange", Left = 780, Top = 684, Width = 360, Height = 100, Font = tileFont };

            Controls.AddRange(new Control[] { btnCreateEquipment, btnCreateAgent, btnModificationEquipment, btnModificationAgent, btnExange });

            _onCreateEquipment = onCreateEquipment;
            btnCreateEquipment.Click += (_, __) => _onCreateEquipment();

        }
    }
}