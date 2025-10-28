using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ProjetParc.Data;

namespace ProjetParc.Views.FirstRun;

/// <summary>
/// Fenêtre de configuration du premier lancement
/// </summary>
public class FirstRunView : Form
{
    private Label _titleLabel = null!;
    private Label _descriptionLabel = null!;
    private Button _useExistingButton = null!;
    private Button _createNewButton = null!;
    private Panel _mainPanel = null!;

    public string SelectedDatabasePath { get; private set; }

    public FirstRunView()
    {
        InitializeComponents();
        SetupLayout();
    }

    private void InitializeComponents()
    {
        // Configuration de la fenêtre
        Text = "Premier lancement - Configuration";
        Size = new Size(600, 400);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        // Panel principal
        _mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40)
        };

        // Titre
        _titleLabel = new Label
        {
            Text = "Bienvenue dans GestiParc !",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(40, 40)
        };

        // Description
        _descriptionLabel = new Label
        {
            Text = "Pour commencer, veuillez configurer l'emplacement de votre base de données.\n\n" +
                   "Vous pouvez utiliser une base de données existante ou en créer une nouvelle.\n" +
                   "La base de données peut être située sur votre disque local, un réseau partagé,\n" +
                   "SharePoint ou tout autre emplacement accessible.",
            Font = new Font("Segoe UI", 10),
            AutoSize = false,
            Size = new Size(520, 120),
            Location = new Point(40, 90)
        };

        // Bouton "Utiliser une BDD existante"
        _useExistingButton = new Button
        {
            Text = "📂 Utiliser une base de données existante",
            Font = new Font("Segoe UI", 11),
            Size = new Size(400, 50),
            Location = new Point(100, 230),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _useExistingButton.FlatAppearance.BorderSize = 0;
        _useExistingButton.Click += UseExistingButton_Click;

        // Bouton "Créer une nouvelle BDD"
        _createNewButton = new Button
        {
            Text = "✨ Créer une nouvelle base de données",
            Font = new Font("Segoe UI", 11),
            Size = new Size(400, 50),
            Location = new Point(100, 290),
            BackColor = Color.FromArgb(16, 137, 62),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _createNewButton.FlatAppearance.BorderSize = 0;
        _createNewButton.Click += CreateNewButton_Click;

        // Ajouter les contrôles
        _mainPanel.Controls.Add(_titleLabel);
        _mainPanel.Controls.Add(_descriptionLabel);
        _mainPanel.Controls.Add(_useExistingButton);
        _mainPanel.Controls.Add(_createNewButton);

        Controls.Add(_mainPanel);
    }

    private void SetupLayout()
    {
        // Effet hover sur les boutons
        _useExistingButton.MouseEnter += (s, e) => _useExistingButton.BackColor = Color.FromArgb(0, 100, 180);
        _useExistingButton.MouseLeave += (s, e) => _useExistingButton.BackColor = Color.FromArgb(0, 120, 212);

        _createNewButton.MouseEnter += (s, e) => _createNewButton.BackColor = Color.FromArgb(14, 120, 55);
        _createNewButton.MouseLeave += (s, e) => _createNewButton.BackColor = Color.FromArgb(16, 137, 62);
    }

    private void UseExistingButton_Click(object sender, EventArgs e)
    {
        using var openDialog = new OpenFileDialog
        {
            Title = "Sélectionner une base de données existante",
            Filter = "Base de données SQLite (*.db)|*.db|Tous les fichiers (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (openDialog.ShowDialog() == DialogResult.OK)
        {
            var selectedPath = openDialog.FileName;

            // Vérifier que le fichier est accessible
            if (!File.Exists(selectedPath))
            {
                MessageBox.Show(
                    "Le fichier sélectionné n'existe pas ou n'est pas accessible.",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Confirmer le choix
            var result = MessageBox.Show(
                $"Utiliser cette base de données ?\n\n{selectedPath}",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                SelectedDatabasePath = selectedPath;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }

    private void CreateNewButton_Click(object sender, EventArgs e)
    {
        using var saveDialog = new SaveFileDialog
        {
            Title = "Créer une nouvelle base de données",
            Filter = "Base de données SQLite (*.db)|*.db",
            DefaultExt = "db",
            FileName = "bddProjetParc.db",
            OverwritePrompt = true
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            var newPath = saveDialog.FileName;

            try
            {
                // Créer le répertoire si nécessaire
                var directory = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Créer un fichier vide (sera initialisé par Database.Open())
                File.WriteAllBytes(newPath, Array.Empty<byte>());

                // Confirmer le choix
                var result = MessageBox.Show(
                    $"Base de données créée avec succès !\n\n{newPath}\n\nUtiliser cette base de données ?",
                    "Succès",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (result == DialogResult.Yes)
                {
                    SelectedDatabasePath = newPath;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    // Supprimer le fichier si l'utilisateur refuse
                    File.Delete(newPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de la création de la base de données :\n\n{ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
