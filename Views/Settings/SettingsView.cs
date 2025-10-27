using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ProjetParc.Views.Settings;

/// <summary>
/// Vue de gestion des paramètres de l'application : Équipes, Sites et Types d'équipement
/// </summary>
public class SettingsView : UserControl
{
    private readonly Action _onBack;
    private TabControl _tabControl;

    public SettingsView(Action onBack)
    {
        _onBack = onBack;

        // Configuration de base
        Dock = DockStyle.Fill;
        BackColor = Theme.Colors.Background;
        Padding = new Padding(Theme.Spacing.Large);

        // Layout principal
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = Theme.Colors.Background
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // En-tête
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu

        // En-tête
        var headerPanel = CreateHeader();
        mainLayout.Controls.Add(headerPanel, 0, 0);

        // TabControl pour les trois catégories
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Body
        };

        // Onglets
        _tabControl.TabPages.Add(CreateTabPage("Équipes", "Equipes"));
        _tabControl.TabPages.Add(CreateTabPage("Sites", "Sites"));
        _tabControl.TabPages.Add(CreateTabPage("Types d'équipement", "equipment_type"));

        mainLayout.Controls.Add(_tabControl, 0, 1);
        Controls.Add(mainLayout);
    }

    private Panel CreateHeader()
    {
        var headerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, Theme.Spacing.Large),
            BackColor = Theme.Colors.Background
        };
        headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

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
            Text = "Paramètres",
            Font = Theme.Fonts.H3,
            ForeColor = Theme.Colors.Primary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(Theme.Spacing.Medium, 0, 0, 0)
        };

        headerPanel.Controls.Add(btnBack, 0, 0);
        headerPanel.Controls.Add(title, 1, 0);

        return headerPanel;
    }

    private TabPage CreateTabPage(string title, string tableName)
    {
        var tabPage = new TabPage(title)
        {
            BackColor = Theme.Colors.Background,
            Padding = new Padding(Theme.Spacing.Medium)
        };

        var managerControl = new ParameterManagerControl(tableName);
        managerControl.Dock = DockStyle.Fill;
        tabPage.Controls.Add(managerControl);

        return tabPage;
    }
}

/// <summary>
/// Contrôle de gestion d'un paramètre (CRUD avec protection)
/// </summary>
internal class ParameterManagerControl : UserControl
{
    private readonly string _tableName;
    private ListView _listView;
    private ListViewColumnSorter _listViewSorter;
    private TextBox _txtName;
    private Button _btnEdit;
    private Button _btnDelete;
    private int? _selectedId;

    public ParameterManagerControl(string tableName)
    {
        _tableName = tableName;
        Dock = DockStyle.Fill;
        BackColor = Theme.Colors.Background;

        // Layout principal : liste à gauche, formulaire à droite
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Liste
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Formulaire

        // Panneau de liste
        var listPanel = CreateListPanel();
        mainLayout.Controls.Add(listPanel, 0, 0);

        // Panneau de formulaire
        var formPanel = CreateFormPanel();
        mainLayout.Controls.Add(formPanel, 1, 0);

        Controls.Add(mainLayout);

        // Chargement initial
        LoadData();
    }

    private Panel CreateListPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, Theme.Spacing.Medium, 0),
            BackColor = Theme.Colors.Background
        };

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Surface,
            BorderStyle = BorderStyle.FixedSingle
        };

        _listView.Columns.Add("ID", 60);
        _listView.Columns.Add("Nom", 300);

        // Configuration du tri par colonnes
        _listViewSorter = new ListViewColumnSorter();
        _listView.ListViewItemSorter = _listViewSorter;
        _listView.ColumnClick += (s, e) => {
            _listViewSorter.SetSortColumn(e.Column);
            _listView.Sort();
        };

        _listView.SelectedIndexChanged += OnListSelectionChanged;

        panel.Controls.Add(_listView);
        return panel;
    }

    private Panel CreateFormPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(Theme.Spacing.Medium),
            BackColor = Theme.Colors.Surface
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 1
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Label
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // TextBox
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Spacing.Medium)); // Espacement
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Bouton Nouveau
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Boutons Edit/Delete

        // Label
        var label = new Label
        {
            Text = "Sélectionner un élément :",
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
        layout.Controls.Add(label, 0, 0);

        // TextBox (lecture seule, juste pour afficher la sélection)
        _txtName = new TextBox
        {
            Height = Theme.Sizes.InputHeight,
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = Theme.Colors.Surface
        };
        Theme.StyleTextBox(_txtName);
        layout.Controls.Add(_txtName, 0, 1);

        // Bouton Nouveau
        var btnNew = new Button
        {
            Text = "Nouveau",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Button
        };
        Theme.StyleSuccessButton(btnNew, setHeight: false);
        btnNew.Click += OnNew;
        layout.Controls.Add(btnNew, 0, 3);

        // Panneau des boutons Edit/Delete
        var buttonPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 120,
            RowCount = 3,
            ColumnCount = 1
        };

        buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Edit
        buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Spacing.Small)); // Espacement
        buttonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Delete

        _btnEdit = new Button
        {
            Text = "Modifier",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Enabled = false,
            Font = Theme.Fonts.Button
        };
        Theme.StyleSecondaryButton(_btnEdit, setHeight: false);
        _btnEdit.Click += OnEdit;
        buttonPanel.Controls.Add(_btnEdit, 0, 0);

        _btnDelete = new Button
        {
            Text = "Supprimer",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Enabled = false,
            Font = Theme.Fonts.Button
        };
        Theme.StyleDangerButton(_btnDelete, setHeight: false);
        _btnDelete.Click += OnDelete;
        buttonPanel.Controls.Add(_btnDelete, 0, 2);

        layout.Controls.Add(buttonPanel, 0, 4);

        panel.Controls.Add(layout);
        return panel;
    }

    private void LoadData()
    {
        _listView.Items.Clear();
        using var connection = Data.Database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT id, name FROM {_tableName} ORDER BY id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var item = new ListViewItem(reader.GetInt32(0).ToString());
            item.SubItems.Add(reader.GetString(1));
            item.Tag = reader.GetInt32(0);
            _listView.Items.Add(item);
        }
    }

    private void OnNew(object sender, EventArgs e)
    {
        // Ouvrir une fenêtre modale pour ajouter un nouvel élément
        var dialog = new AddParameterDialog(_tableName);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void OnListSelectionChanged(object sender, EventArgs e)
    {
        if (_listView.SelectedItems.Count > 0)
        {
            _selectedId = (int)_listView.SelectedItems[0].Tag;
            _txtName.Text = _listView.SelectedItems[0].SubItems[1].Text;
            _btnEdit.Enabled = true;
            _btnDelete.Enabled = true;
        }
        else
        {
            _selectedId = null;
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
        }
    }

    private void OnEdit(object sender, EventArgs e)
    {
        if (!_selectedId.HasValue) return;

        var name = _txtName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Veuillez entrer un nom.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var connection = Data.Database.Open();
            
            // Vérifier si le nom existe déjà (sauf pour l'élément actuel)
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = $"SELECT COUNT(*) FROM {_tableName} WHERE name = $name AND id != $id";
                checkCommand.Parameters.AddWithValue("$name", name);
                checkCommand.Parameters.AddWithValue("$id", _selectedId.Value);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                
                if (count > 0)
                {
                    MessageBox.Show(
                        $"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.", 
                        "Nom existant", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Warning
                    );
                    return;
                }
            }
            
            // Modifier l'élément
            using var command = connection.CreateCommand();
            command.CommandText = $"UPDATE {_tableName} SET name = $name WHERE id = $id";
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$id", _selectedId.Value);
            command.ExecuteNonQuery();

            MessageBox.Show("Élément modifié avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
        }
        catch (SqliteException ex)
        {
            MessageBox.Show($"Erreur lors de la modification : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnDelete(object sender, EventArgs e)
    {
        if (!_selectedId.HasValue) return;

        // Vérifier si l'élément est utilisé
        if (IsInUse(_selectedId.Value))
        {
            MessageBox.Show(
                "Cet élément est utilisé et ne peut pas être supprimé.\nVous pouvez le modifier si nécessaire.",
                "Suppression impossible",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        var result = MessageBox.Show(
            "Êtes-vous sûr de vouloir supprimer cet élément ?",
            "Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes) return;

        try
        {
            using var connection = Data.Database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM {_tableName} WHERE id = $id";
            command.Parameters.AddWithValue("$id", _selectedId.Value);
            command.ExecuteNonQuery();

            MessageBox.Show("Élément supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _txtName.Clear();
            _selectedId = null;
            LoadData();
        }
        catch (SqliteException ex)
        {
            MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool IsInUse(int id)
    {
        using var connection = Data.Database.Open();
        using var command = connection.CreateCommand();

        // Vérifier selon le type de table
        switch (_tableName)
        {
            case "Equipes":
                command.CommandText = "SELECT COUNT(*) FROM Agents WHERE equipe_id = $id";
                break;
            case "Sites":
                command.CommandText = "SELECT COUNT(*) FROM Agents WHERE site_id = $id";
                break;
            case "equipment_type":
                command.CommandText = "SELECT COUNT(*) FROM Equipements WHERE type_id = $id";
                break;
            default:
                return false;
        }

        command.Parameters.AddWithValue("$id", id);
        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }
}

/// <summary>
/// Fenêtre modale pour ajouter un nouveau paramètre
/// </summary>
internal class AddParameterDialog : Form
{
    private readonly string _tableName;
    private TextBox _txtName;

    public AddParameterDialog(string tableName)
    {
        _tableName = tableName;

        // Configuration de la fenêtre
        Text = "Nouvel élément";
        Size = new Size(500, 250);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Theme.Colors.Background;

        // Layout principal
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(Theme.Spacing.Large),
            RowCount = 4,
            ColumnCount = 1
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Label
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // TextBox
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Spacing.Medium)); // Espacement
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); // Boutons

        // Label
        var label = new Label
        {
            Text = "Nom :",
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
        layout.Controls.Add(label, 0, 0);

        // TextBox
        _txtName = new TextBox
        {
            Height = Theme.Sizes.InputHeight,
            Dock = DockStyle.Fill
        };
        Theme.StyleTextBox(_txtName);
        layout.Controls.Add(_txtName, 0, 1);

        // Panneau des boutons
        var buttonPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var btnCancel = new Button
        {
            Text = "Annuler",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Button,
            Margin = new Padding(0, 0, Theme.Spacing.Small / 2, 0),
            DialogResult = DialogResult.Cancel
        };
        Theme.StyleOutlineButton(btnCancel, setHeight: false);
        buttonPanel.Controls.Add(btnCancel, 0, 0);

        var btnOk = new Button
        {
            Text = "OK",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Button,
            Margin = new Padding(Theme.Spacing.Small / 2, 0, 0, 0)
        };
        Theme.StylePrimaryButton(btnOk, setHeight: false);
        btnOk.Click += OnOk;
        buttonPanel.Controls.Add(btnOk, 1, 0);

        layout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(layout);
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        // Focus sur la TextBox à l'ouverture
        _txtName.Focus();
    }

    private void OnOk(object sender, EventArgs e)
    {
        var name = _txtName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Veuillez entrer un nom.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var connection = Data.Database.Open();

            // Vérifier si le nom existe déjà
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = $"SELECT COUNT(*) FROM {_tableName} WHERE name = $name";
                checkCommand.Parameters.AddWithValue("$name", name);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show(
                        $"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.",
                        "Nom existant",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
            }

            // Ajouter l'élément
            using var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {_tableName} (name) VALUES ($name)";
            command.Parameters.AddWithValue("$name", name);
            command.ExecuteNonQuery();

            MessageBox.Show("Élément ajouté avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (SqliteException ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
