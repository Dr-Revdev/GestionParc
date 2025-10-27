using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;

namespace ProjetParc.Views.Equipment;

/// <summary>
/// Vue permettant la création d'un nouvel équipement dans le système
/// </summary>
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

    /// <summary>
    /// Initialise une nouvelle instance de la vue de création d'équipement
    /// </summary>
    /// <param name="onBack">Action à exécuter pour revenir à la vue précédente</param>
    public EquipmentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadEquipmentTypes();
        btnCreate.Click += btnCreate_Click;
    }

    /// <summary>
    /// Construit l'interface utilisateur de la vue de création d'équipement
    /// </summary>
    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;

        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(Theme.Spacing.Large),
            BackColor = Theme.Colors.Background,
            RowStyles = {
                new RowStyle(SizeType.Absolute, 60),  // Bouton retour
                new RowStyle(SizeType.Percent, 100),  // Formulaire
                new RowStyle(SizeType.Absolute, 80)   // Bouton créer
            }
        };
        Controls.Add(mainLayout);

        // Bouton retour
        var btnBack = new Button 
        { 
            Text = "← Retour", 
            Width = Theme.Sizes.ButtonWidth, 
            Height = Theme.Sizes.ButtonHeightLarge, 
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0),
            Font = Theme.Fonts.Button
        };
        Theme.StyleOutlineButton(btnBack);
        btnBack.Click += (_, __) => _onBack?.Invoke();
        mainLayout.Controls.Add(btnBack, 0, 0);

        // Panel du formulaire
        TableLayoutPanel formLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 7,
            Padding = new Padding(Theme.Spacing.Large),
            BackColor = Theme.Colors.Surface,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 33.33f),
                new ColumnStyle(SizeType.Percent, 33.33f),
                new ColumnStyle(SizeType.Percent, 33.33f)
            },
            RowStyles = {
                new RowStyle(SizeType.Absolute, 30),   // Labels ligne 1
                new RowStyle(SizeType.Absolute, 45),   // Inputs ligne 1
                new RowStyle(SizeType.Absolute, 30),   // Labels ligne 2
                new RowStyle(SizeType.Absolute, 45),   // Inputs ligne 2
                new RowStyle(SizeType.Absolute, 30),   // Label commentaire
                new RowStyle(SizeType.Absolute, 150),  // Commentaire multiline - hauteur fixe
                new RowStyle(SizeType.Percent, 100)    // Espacement flexible
            }
        };
        mainLayout.Controls.Add(formLayout, 0, 1);

        // Première ligne : Type, Nom, Code parc
        cbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        Theme.StyleComboBox(cbType);
        AddFormRow(formLayout, 0, "Type", cbType);
        
        tbName = new TextBox();
        Theme.StyleTextBox(tbName);
        AddFormRow(formLayout, 0, "Nom", tbName, 1);
        
        tbCodeParc = new TextBox();
        Theme.StyleTextBox(tbCodeParc);
        AddFormRow(formLayout, 0, "Code parc", tbCodeParc, 2);

        // Deuxième ligne : Numéro de série, Marque
        tbSerial = new TextBox();
        Theme.StyleTextBox(tbSerial);
        AddFormRow(formLayout, 2, "Numéro de série", tbSerial);
        
        tbBrand = new TextBox();
        Theme.StyleTextBox(tbBrand);
        AddFormRow(formLayout, 2, "Marque", tbBrand, 1);

        // Troisième ligne : Commentaire (sur toute la largeur)
        tbComment = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };
        Theme.StyleTextBox(tbComment);
        AddFormRow(formLayout, 4, "Commentaire", tbComment, 0, 3);

        // Bouton créer (en bas)
        btnCreate = new Button
        {
            Text = "Créer",
            Height = Theme.Sizes.ButtonHeight,
            Dock = DockStyle.Right,
            Width = Theme.Sizes.ButtonWidth
        };
        Theme.StylePrimaryButton(btnCreate);
        mainLayout.Controls.Add(btnCreate, 0, 2);

        // Définir l'ordre de tabulation
        cbType.TabIndex = 0;
        tbName.TabIndex = 1;
        tbCodeParc.TabIndex = 2;
        tbSerial.TabIndex = 3;
        tbBrand.TabIndex = 4;
        tbComment.TabIndex = 5;
        btnCreate.TabIndex = 6;

        
    }

    /// <summary>
    /// Classe représentant un type d'équipement dans la liste déroulante
    /// </summary>
    private sealed class EquipmentTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    /// <summary>
    /// Charge la liste des types d'équipement depuis la base de données
    /// </summary>
    private void LoadEquipmentTypes()
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = "SELECT id, name FROM equipment_type ORDER BY name;";

            using var reader = command.ExecuteReader();
            var equipmentTypeItems = new List<EquipmentTypeItem>();
            while (reader.Read())
            {
                var typeItem = new EquipmentTypeItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                equipmentTypeItems.Add(typeItem);
            }

            cbType.DataSource = equipmentTypeItems;
            cbType.DisplayMember = nameof(EquipmentTypeItem.Name);
            cbType.ValueMember = nameof(EquipmentTypeItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des types d'équipement : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Valide les données du formulaire de création d'équipement
    /// </summary>
    /// <param name="errorMessage">Message d'erreur en cas de validation échouée</param>
    /// <returns>true si la validation est réussie, false sinon</returns>
    private bool ValidateEquipmentForm(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(tbName.Text))
        {
            errorMessage = "Le nom est obligatoire.";
            return false;
        }
        if (cbType.SelectedItem is not EquipmentTypeItem)
        {
            errorMessage = "Sélectionner un type d'équipement.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(tbCodeParc.Text))
        {
            errorMessage = "Le code parc est obligatoire.";
            return false;
        }

        errorMessage = "";
        return true;
    }
    /// <summary>
    /// Convertit une chaîne en objet DBNull si elle est vide ou null
    /// </summary>
    /// <param name="s">La chaîne à convertir</param>
    /// <returns>La chaîne nettoyée ou DBNull.Value si vide</returns>
    private static object ToDbNullable(string s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();

    /// <summary>
    /// Génère un identifiant unique pour un nouvel équipement
    /// </summary>
    /// <returns>Un identifiant unique au format string</returns>
    private static string GenerateEquipmentId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Insère un nouvel équipement dans la base de données avec les valeurs du formulaire
    /// </summary>
    private void InsertEquipment()
    {
        if (!ValidateEquipmentForm(out var errorMessage))
        {
            MessageBox.Show(errorMessage);
            return;
        }

        var SelectedType = (EquipmentTypeItem)cbType.SelectedItem;
        var newId = GenerateEquipmentId();

        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = @"INSERT INTO ""Equipements"" (id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire) VALUES ($id, $typeId, $name, $codeParc, $serial, $brand, $comment);";

        command.Parameters.AddWithValue("$id", newId);
        command.Parameters.AddWithValue("$typeId", SelectedType.Id);
        command.Parameters.AddWithValue("$name", tbName.Text.Trim());
        command.Parameters.AddWithValue("$codeParc", tbCodeParc.Text.Trim());

        command.Parameters.AddWithValue("$serial",   ToDbNullable(tbSerial.Text));
        command.Parameters.AddWithValue("$brand",    ToDbNullable(tbBrand.Text));
        command.Parameters.AddWithValue("$comment",  ToDbNullable(tbComment.Text));

        try
        {
            command.ExecuteNonQuery();
            MessageBox.Show("Équipement créé.");

            tbSerial.Clear();
            tbName.Clear();
            tbBrand.Clear();
            tbCodeParc.Clear();
            tbComment.Clear();
            if (cbType.Items.Count > 0) cbType.SelectedIndex = 0;
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Erreur SQL : " + ex.Message);
        }
    }

    /// <summary>
    /// Gestionnaire d'événement pour le clic sur le bouton de création
    /// </summary>
    private void btnCreate_Click(object sender, EventArgs e)
    {
        InsertEquipment();
    }

    /// <summary>
    /// Ajoute une ligne de formulaire avec un label et un contrôle dans le TableLayoutPanel
    /// </summary>
    private void AddFormRow(TableLayoutPanel panel, int row, string labelText, Control control, int col = 0, int colSpan = 1)
    {
        var label = new Label 
        { 
            Text = labelText, 
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary,
            Padding = new Padding(5, 0, 0, 5),
            Margin = new Padding(5, 0, 15, 0)
        };
        panel.Controls.Add(label, col, row);

        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(5, 0, 15, 15);
        panel.Controls.Add(control, col, row + 1);
        if (colSpan > 1)
        {
            panel.SetColumnSpan(control, colSpan);
            if (colSpan > 1 && row == 4) // Label commentaire
            {
                panel.SetColumnSpan(label, colSpan);
            }
        }
    }
}