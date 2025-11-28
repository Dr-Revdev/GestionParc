using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestiParc.Ui.Views.Admin;
    /// <summary>
    /// Menu d'administration - 4 gros boutons pour créer/modifier agents et équipements
    /// + un bouton pour faire des échanges d'équipement entre agents
    /// </summary>
    public class AdminMenuView : UserControl
    {
        private readonly Action _onBack;
        private readonly Action _onCreateEquipment;
        private readonly Action _onCreateAgent;
        private readonly Action _onEditAgent;
        private readonly Action _onEditEquipment;
        private readonly Action _onSettings;
        private Button btnCreateEquipment = null!;
        private Button btnCreateAgent = null!;
        private Button btnModificationEquipment = null!;
        private Button btnModificationAgent = null!;
        private Button btnExange = null!;
        private Button btnSettings = null!;

        /// <summary>
        /// Constructeur - prend en paramètre toutes les callbacks pour naviguer
        /// </summary>
        /// <param name="onBack">Callback pour retourner à l'accueil</param>
        /// <param name="onCreateEquipment">Callback pour aller vers la création d'équipement</param>
        /// <param name="onCreateAgent">Callback création agent</param>
        /// <param name="onEditAgent">Callback modification agent</param>
        /// <param name="onEditEquipment">Callback modification équipement</param>
        /// <param name="onSettings">Callback vers les paramètres (sites, équipes, types)</param>
        public AdminMenuView(Action onBack, Action onCreateEquipment, Action onCreateAgent, Action onEditAgent, Action onEditEquipment, Action onSettings)
        {
            _onBack = onBack;
            _onCreateEquipment = onCreateEquipment;
            _onCreateAgent = onCreateAgent;
            _onEditAgent = onEditAgent;
            _onEditEquipment = onEditEquipment;
            _onSettings = onSettings;

            // Configuration de base
            Dock = DockStyle.Fill;
            BackColor = Theme.Colors.Background;
            Padding = new Padding(Theme.Spacing.Large);

            // Layout principal
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Theme.Colors.Background
            };

            // Configuration des lignes du layout principal
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // En-tête (augmenté pour laisser de l'espace)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70)); // Zone des boutons principaux
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Zone du bouton échange

            // En-tête
            var headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, Theme.Spacing.Large),
                BackColor = Theme.Colors.Background
            };
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton paramètres

            var btnBack = new Button 
            { 
                Text = "← Retour",
                Width = Theme.Sizes.ButtonWidth,
                Height = Theme.Sizes.ButtonHeight,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Font = Theme.Fonts.Button
            };
            Theme.StyleOutlineButton(btnBack, setHeight: false);
            btnBack.Click += (_, __) => _onBack?.Invoke();

            var title = new Label 
            { 
                Text = "Administration",
                Font = Theme.Fonts.H3,
                ForeColor = Theme.Colors.Primary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(Theme.Spacing.Medium, 0, 0, 0)
            };

            btnSettings = new Button
            {
                Text = "⚙ Paramètres",
                Width = Theme.Sizes.ButtonWidth,
                Height = Theme.Sizes.ButtonHeight,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Font = Theme.Fonts.Button
            };
            Theme.StyleSecondaryButton(btnSettings, setHeight: false);
            btnSettings.Click += (_, __) => _onSettings?.Invoke();

            headerPanel.Controls.Add(btnBack, 0, 0);
            headerPanel.Controls.Add(title, 1, 0);
            headerPanel.Controls.Add(btnSettings, 2, 0);

            // Zone des boutons principaux
            var buttonLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                Margin = new Padding(0),
                BackColor = Theme.Colors.Background
            };

            // Configuration des lignes et colonnes pour les boutons principaux
            for (int i = 0; i < 2; i++)
            {
                buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            }

            var tileFont = new Font("Segoe UI", 14f, FontStyle.Regular);

            // Création des boutons principaux
            btnCreateEquipment = CreateMenuButton("Création Équipement", tileFont);
            Theme.StylePrimaryButton(btnCreateEquipment, setHeight: false);
            
            btnCreateAgent = CreateMenuButton("Création Agent", tileFont);
            Theme.StylePrimaryButton(btnCreateAgent, setHeight: false);
            
            btnModificationEquipment = CreateMenuButton("Modification Équipement", tileFont);
            Theme.StyleSecondaryButton(btnModificationEquipment, setHeight: false);
            
            btnModificationAgent = CreateMenuButton("Modification Agent", tileFont);
            Theme.StyleSecondaryButton(btnModificationAgent, setHeight: false);
            
            btnExange = CreateMenuButton("Échange", tileFont);
            Theme.StyleOutlineButton(btnExange, setHeight: false);

            // Ajout des boutons au layout avec espacement
            buttonLayout.Controls.Add(btnCreateEquipment, 0, 0);
            buttonLayout.Controls.Add(btnCreateAgent, 1, 0);
            buttonLayout.Controls.Add(btnModificationEquipment, 0, 1);
            buttonLayout.Controls.Add(btnModificationAgent, 1, 1);

            // Zone du bouton échange - Panneau avec fond
            var exchangePanel = new Panel 
            { 
                Dock = DockStyle.Fill,
                BackColor = Theme.Colors.Background,
                Padding = new Padding(0, Theme.Spacing.Large, 0, 0)
            };
            
            btnExange.Dock = DockStyle.None;
            btnExange.Anchor = AnchorStyles.None;
            btnExange.Width = Theme.Sizes.ButtonWidthLarge;
            btnExange.Height = Theme.Sizes.ButtonHeightXLarge;
            exchangePanel.Controls.Add(btnExange);
            
            // Centrer le bouton échange
            exchangePanel.Resize += (s, e) => {
                btnExange.Location = new Point(
                    (exchangePanel.ClientSize.Width - btnExange.Width) / 2,
                    (exchangePanel.ClientSize.Height - btnExange.Height) / 2
                );
            };

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
        /// Ouvre la fenêtre pour échanger des équipements entre 2 agents
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
                Margin = new Padding(Theme.Spacing.Medium),
                AutoSize = false,
                Cursor = Cursors.Hand
            };
        }
    }
