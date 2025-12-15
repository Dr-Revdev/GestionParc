using GestiParc.Infrastructure.Data.Repositories;
using GestiParc.Ui.Services.Api;
using System.Net.Http;

namespace GestiParc.Ui.Views.Settings;

/// <summary>
/// Écran de gestion des paramètres - 3 onglets (Équipes, Sites, Types d'équipement)
/// Chaque onglet permet d'ajouter/modifier/supprimer les entrées dans ces tables référentielles
/// </summary>
public class SettingsView : UserControl
{
    private readonly Action _onBack;
    private TabControl _tabControl = null!;

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
/// Contrôle réutilisable pour gérer une table référentielle (liste + form modification)
/// Utilisé pour Équipes, Sites et Types d'équipement
/// </summary>
internal class ParameterManagerControl : UserControl
{
    private readonly AgentApiClient _agentApiClient = new AgentApiClient();
    private readonly EquipeApiClient _equipeApiClient = new EquipeApiClient();
    private readonly SiteApiClient _siteApiClient = new SiteApiClient();
    private readonly EquipmentTypeApiClient _equipmentTypeApiClient = new EquipmentTypeApiClient();
    private readonly EquipmentApiClient _equipmentApiClient = new EquipmentApiClient();
    private readonly string _tableName;
    private ListView _listView = null!;
    private ListViewColumnSorter _listViewSorter = null!;
    private TextBox _txtName = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
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

        // Chargement initial via Load event
        Load += async (s, e) =>
        {
            try
            {
                await LoadDataAsync();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erreur de connexion au serveur : {ex.Message}",
                    "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
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

        // Colonne ID cachée (width = 0) mais conservée pour le tri
        _listView.Columns.Add("ID", 0);
        _listView.Columns.Add("Nom", 360);

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
            Text = "Nom :",
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
        layout.Controls.Add(label, 0, 0);

        // TextBox (éditable pour modification)
        _txtName = new TextBox
        {
            Height = Theme.Sizes.InputHeight,
            Dock = DockStyle.Fill,
            ReadOnly = false,
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
        btnNew.Click += btnNew_Click;
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

        btnEdit = new Button
        {
            Text = "Modifier",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Enabled = false,
            Font = Theme.Fonts.Button
        };
        Theme.StyleSecondaryButton(btnEdit, setHeight: false);
        btnEdit.Click += btnEdit_Click;
        buttonPanel.Controls.Add(btnEdit, 0, 0);

        btnDelete = new Button
        {
            Text = "Supprimer",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Fill,
            Enabled = false,
            Font = Theme.Fonts.Button
        };
        Theme.StyleDangerButton(btnDelete, setHeight: false);
        btnDelete.Click += btnDelete_Click;
        buttonPanel.Controls.Add(btnDelete, 0, 2);

        layout.Controls.Add(buttonPanel, 0, 4);

        panel.Controls.Add(layout);
        return panel;
    }

    private async Task LoadDataAsync()
    {
        _listView.Items.Clear();
        
        // Charger depuis le repository approprié selon la table
        var items = _tableName switch
        {
            "Equipes" => (await _equipeApiClient.GetAllAsync())
                .OrderBy(e => e.Id)
                .Select(e => (e.Id, e.Name))
                .ToList(),
            "Sites" => (await _siteApiClient.GetAllAsync())
                .OrderBy(s => s.Id)
                .Select(s => (s.Id, s.Name))
                .ToList(),
            "equipment_type" => (await _equipmentTypeApiClient.GetAllAsync())
                .OrderBy(t => t.Id)
                .Select(t => (t.Id, t.Name))
                .ToList(),
            _ => new List<(int Id, string Name)>()
        };

        foreach (var item in items)
        {
            var listItem = new ListViewItem(item.Id.ToString());
            listItem.SubItems.Add(item.Name);
            listItem.Tag = item.Id;
            _listView.Items.Add(listItem);
        }
    }

    private async void LoadData()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Erreur de connexion au serveur : {ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnNew_Click(object? sender, EventArgs e)
    {
        // Ouvrir une fenêtre modale pour ajouter un nouvel élément
        var dialog = new AddParameterDialog(_tableName);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadData();
        }
    }

    private void OnListSelectionChanged(object? sender, EventArgs e)
    {
        if (_listView.SelectedItems.Count > 0)
        {
            _selectedId = _listView.SelectedItems[0].Tag as int?;
            _txtName.Text = _listView.SelectedItems[0].SubItems[1].Text;
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
        }
        else
        {
            _selectedId = null;
            _txtName.Clear();
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }
    }

    private async Task UpdateItemAsync()
    {
        if (!_selectedId.HasValue) return;

        var name = _txtName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Veuillez entrer un nom.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool updated = false;
        
        switch (_tableName)
        {
            case "Equipes":
                var equipes = await _equipeApiClient.GetAllAsync();
                if (equipes.Any(e => e.Name == name && e.Id != _selectedId.Value))
                {
                    MessageBox.Show($"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.", 
                        "Nom existant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var equipe = equipes.FirstOrDefault(e => e.Id == _selectedId.Value);
                if (equipe != null)
                {
                    await _equipeApiClient.UpdateAsync(_selectedId.Value, equipe with { Name = name });
                    updated = true;
                }
                break;
                
            case "Sites":
                var sites = await _siteApiClient.GetAllAsync();
                if (sites.Any(s => s.Name == name && s.Id != _selectedId.Value))
                {
                    MessageBox.Show($"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.", 
                        "Nom existant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var site = sites.FirstOrDefault(s => s.Id == _selectedId.Value);
                if (site != null)
                {
                    await _siteApiClient.UpdateAsync(_selectedId.Value, site with { Name = name });
                    updated = true;
                }
                break;
                
            case "equipment_type":
                var types = await _equipmentTypeApiClient.GetAllAsync();
                if (types.Any(t => t.Name == name && t.Id != _selectedId.Value))
                {
                    MessageBox.Show($"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.", 
                        "Nom existant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var type = types.FirstOrDefault(t => t.Id == _selectedId.Value);
                if (type != null)
                {
                    await _equipmentTypeApiClient.UpdateAsync(_selectedId.Value, type with { Name = name });
                    updated = true;
                }
                break;
        }

        if (updated)
        {
            await LoadDataAsync();
        }
    }

    private async void btnEdit_Click(object? sender, EventArgs e)
    {
        try
        {
            await UpdateItemAsync();
            MessageBox.Show("Élément modifié avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Erreur de connexion au serveur : {ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification : {ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteItemAsync()
    {
        if (!_selectedId.HasValue) return;

        // Vérifier si l'élément est utilisé
        if (await IsInUseAsync(_selectedId.Value))
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

        switch (_tableName)
        {
            case "Equipes":
                await _equipeApiClient.DeleteAsync(_selectedId.Value);
                break;
            case "Sites":
                await _siteApiClient.DeleteAsync(_selectedId.Value);
                break;
            case "equipment_type":
                await _equipmentTypeApiClient.DeleteAsync(_selectedId.Value);
                break;
        }

        _txtName.Clear();
        _selectedId = null;
        await LoadDataAsync();
    }

    private async void btnDelete_Click(object? sender, EventArgs e)
    {
        try
        {
            await DeleteItemAsync();
            MessageBox.Show("Élément supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Erreur de connexion au serveur : {ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression : {ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task<bool> IsInUseAsync(int id)
    {
        // Vérifier selon le type de table
        return _tableName switch
        {
            "Equipes" => await CheckEquipeInUseAsync(id),
            "Sites" => await CheckSiteInUseAsync(id),
            "equipment_type" => await CheckEquipmentTypeInUseAsync(id),
            _ => false
        };
    }

    private async Task<bool> CheckEquipeInUseAsync(int id)
    {
        var agents = await _agentApiClient.GetAllAsync();
        return agents.Any(a => a.EquipeId == id);
    }

    private async Task<bool> CheckSiteInUseAsync(int id)
    {
        var agents = await _agentApiClient.GetAllAsync();
        return agents.Any(a => a.SiteId == id);
    }

    private async Task<bool> CheckEquipmentTypeInUseAsync(int id)
    {
        var equipments = await _equipmentApiClient.GetAllAsync();
        return equipments.Any(e => e.TypeId == id);
    }
}

/// <summary>
/// Popup pour ajouter un nouveau paramètre (équipe, site ou type équipement)
/// </summary>
internal class AddParameterDialog : Form
{
    private readonly EquipeApiClient _equipeApiClient = new EquipeApiClient();
    private readonly SiteApiClient _siteApiClient = new SiteApiClient();
    private readonly EquipmentTypeApiClient _equipmentTypeApiClient = new EquipmentTypeApiClient();
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
        btnOk.Click += btnOk_Click;
        buttonPanel.Controls.Add(btnOk, 1, 0);

        layout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(layout);
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        // Focus sur la TextBox à l'ouverture
        _txtName.Focus();
    }

    private async Task InsertItemAsync(string name)
    {
        switch (_tableName)
        {
            case "Equipes":
                var equipes = await _equipeApiClient.GetAllAsync();
                if (equipes.Any(e => e.Name == name))
                {
                    MessageBox.Show($"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.",
                        "Nom existant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await _equipeApiClient.CreateAsync(new Core.DTOs.EquipeDto(0, name));
                break;
                
            case "Sites":
                var sites = await _siteApiClient.GetAllAsync();
                if (sites.Any(s => s.Name == name))
                {
                    MessageBox.Show($"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.",
                        "Nom existant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await _siteApiClient.CreateAsync(new Core.DTOs.SiteDto(0, name));
                break;
                
            case "equipment_type":
                var types = await _equipmentTypeApiClient.GetAllAsync();
                if (types.Any(t => t.Name == name))
                {
                    MessageBox.Show($"Le nom \"{name}\" existe déjà.\nVeuillez choisir un nom différent.",
                        "Nom existant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await _equipmentTypeApiClient.CreateAsync(new Core.DTOs.EquipmentTypeDto(0, name));
                break;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private async void btnOk_Click(object? sender, EventArgs e)
    {
        var name = _txtName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Veuillez entrer un nom.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            await InsertItemAsync(name);
            MessageBox.Show("Élément ajouté avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Erreur de connexion au serveur : {ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
