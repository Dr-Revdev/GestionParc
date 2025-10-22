using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetParc.Views.Admin;
    /// <summary>
    /// Vue du menu d'administration permettant d'accéder aux fonctionnalités de création et modification
    /// des équipements et des agents
    /// </summary>
    public class AdminMenuView : UserControl
    {
        private readonly Action _onBack;
        private readonly Action _onCreateEquipment;
        private readonly Action _onCreateAgent;
        private readonly Action _onEditAgent;
        private readonly Action _onEditEquipment;
        private Button btnCreateEquipment;
        private Button btnCreateAgent;
        private Button btnModificationEquipment;
        private Button btnModificationAgent;
        private Button btnExange;

        /// <summary>
        /// Initialise une nouvelle instance du menu d'administration
        /// </summary>
        /// <param name="onBack">Action à exécuter pour revenir à la vue précédente</param>
        /// <param name="onCreateEquipment">Action à exécuter pour créer un équipement</param>
        /// <param name="onCreateAgent">Action à exécuter pour créer un agent</param>
        /// <param name="onEditAgent">Action à exécuter pour modifier un agent</param>
        /// <param name="onEditEquipment">Action à exécuter pour modifier un équipement</param>
        public AdminMenuView(Action onBack, Action onCreateEquipment, Action onCreateAgent, Action onEditAgent, Action onEditEquipment)
        {
            _onBack = onBack;
            _onCreateEquipment = onCreateEquipment;
            _onCreateAgent = onCreateAgent;
            _onEditAgent = onEditAgent;
            _onEditEquipment = onEditEquipment;

            // Configuration de base
            Dock = DockStyle.Fill;
            Padding = new Padding(20);

            // Layout principal
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };

            // Configuration des lignes du layout principal
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // En-tête
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70)); // Zone des boutons principaux
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Zone du bouton échange

            // En-tête
            var headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 20)
            };
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre

            var btnBack = new Button 
            { 
                Text = "← Retour",
                Height = 36,
                Width = 120,
                Dock = DockStyle.Left
            };
            btnBack.Click += (_, __) => _onBack?.Invoke();

            var title = new Label 
            { 
                Text = "Menu modification / création",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            headerPanel.Controls.Add(btnBack, 0, 0);
            headerPanel.Controls.Add(title, 1, 0);

            // Zone des boutons principaux
            var buttonLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                Margin = new Padding(0)
            };

            // Configuration des lignes et colonnes pour les boutons principaux
            for (int i = 0; i < 2; i++)
            {
                buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            }

            var tileFont = new Font("Segoe UI", 14f, FontStyle.Bold);

            // Création des boutons principaux
            btnCreateEquipment = CreateMenuButton("Création Equipement", tileFont);
            btnCreateAgent = CreateMenuButton("Création Agent", tileFont);
            btnModificationEquipment = CreateMenuButton("Modification Equipement", tileFont);
            btnModificationAgent = CreateMenuButton("Modification agent", tileFont);
            btnExange = CreateMenuButton("Echange", tileFont);

            // Ajout des boutons au layout avec espacement
            buttonLayout.Controls.Add(btnCreateEquipment, 0, 0);
            buttonLayout.Controls.Add(btnCreateAgent, 1, 0);
            buttonLayout.Controls.Add(btnModificationEquipment, 0, 1);
            buttonLayout.Controls.Add(btnModificationAgent, 1, 1);

            // Zone du bouton échange
            var exchangePanel = new Panel { Dock = DockStyle.Fill };
            btnExange.Dock = DockStyle.None;
            btnExange.Anchor = AnchorStyles.None;
            btnExange.Width = 280;
            btnExange.Height = 80;
            exchangePanel.Controls.Add(btnExange);
            
            // Centrer le bouton échange
            btnExange.Location = new Point(
                (exchangePanel.ClientSize.Width - btnExange.Width) / 2,
                (exchangePanel.ClientSize.Height - btnExange.Height) / 2
            );

            // Assemblage final
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(buttonLayout, 0, 1);
            mainLayout.Controls.Add(exchangePanel, 0, 2);

            Controls.Add(mainLayout);

            // Ajout des gestionnaires d'événements
            btnCreateEquipment.Click += (_, __) => _onCreateEquipment();
            btnCreateAgent.Click += (_, __) => _onCreateAgent();
            btnModificationAgent.Click += (_, __) => _onEditAgent();
            btnModificationEquipment.Click += (_, __) => _onEditEquipment();
            btnExange.Click += (_, __) => ShowExchangeDialog();
        }

        /// <summary>
        /// Affiche le dialogue d'échange d'équipements entre agents
        /// </summary>
        private void ShowExchangeDialog()
        {
            var dialog = new Equipment.EquipmentExchangeView();
            dialog.ShowDialog();
        }

        private static Button CreateMenuButton(string text, Font font)
        {
            return new Button
            {
                Text = text,
                Font = font,
                Dock = DockStyle.Fill,
                Margin = new Padding(20),
                AutoSize = false
            };

        }
    }