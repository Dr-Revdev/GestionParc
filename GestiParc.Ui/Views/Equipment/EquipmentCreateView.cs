using System.Drawing;
using System.Windows.Forms;
using GestiParc.Ui.Data;
using GestiParc.Ui.Services;
using GestiParc.Core.DTOs;
using GestiParc.Infrastructure.Data.Repositories;

namespace GestiParc.Ui.Views.Equipment;

/// <summary>
/// Formulaire de création d'équipement - remplit type, nom, code parc, n° série, marque, commentaire
/// </summary>
public class EquipmentCreateView : UserControl
{
    private readonly Action _onBack;

    private ComboBox cbType = null!;
    private TextBox tbName = null!;
    private TextBox tbCodeParc = null!;
    private TextBox tbSerial = null!;
    private TextBox tbBrand = null!;
    private TextBox tbComment = null!;
    private Button btnCreate = null!;

    /// <summary>
    /// Constructeur - monte l'UI et charge les types d'équipement
    /// </summary>
    /// <param name="onBack">Callback retour</param>
    public EquipmentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadEquipmentTypes();
        btnCreate.Click += btnCreate_Click;
    }

    /// <summary>
    /// Monte toute l'interface - formulaire avec 6 champs sur 3 colonnes
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

        // Panneau du formulaire
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
    /// Item pour la combobox des types d'équipement
    /// </summary>
    private sealed class EquipmentTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    /// <summary>
    /// Remplit la liste déroulante des types (PC, Ecran, etc.)
    /// </summary>
    private void LoadEquipmentTypes()
    {
        try
        {
            var types = new EquipmentTypeMySqlRepository().GetAll();

            var equipmentTypeItems = types
                .Select(t => new EquipmentTypeItem { Id = t.Id, Name = t.Name })
                .OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            cbType.DataSource = equipmentTypeItems;
            cbType.DisplayMember = nameof(EquipmentTypeItem.Name);
            cbType.ValueMember = nameof(EquipmentTypeItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du chargement des types d'équipement : {ex.Message}",
                "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error
            );
        }
    }

    /// <summary>
    /// Vérifie que le formulaire est correct - le type est obligatoire + au moins 1 champ parmi nom/code parc/n°série
    /// </summary>
    /// <param name="errorMessage">Message d'erreur si y'a un souci</param>
    /// <returns>true si tout est bon</returns>
    private bool ValidateEquipmentForm(out string errorMessage)
    {
        // Type obligatoire (toujours nécessaire pour identifier l'équipement)
        if (cbType.SelectedItem is not EquipmentTypeItem)
        {
            errorMessage = "Sélectionner un type d'équipement.";
            return false;
        }

        // Au moins UN champ identifiant doit être rempli
        bool hasName = !string.IsNullOrWhiteSpace(tbName.Text);
        bool hasCodeParc = !string.IsNullOrWhiteSpace(tbCodeParc.Text);
        bool hasSerial = !string.IsNullOrWhiteSpace(tbSerial.Text);

        if (!hasName && !hasCodeParc && !hasSerial)
        {
            errorMessage = "Au moins un champ parmi Nom, Code Parc ou N° Série doit être renseigné.";
            return false;
        }

        errorMessage = "";
        return true;
    }

    /// <summary>
    /// Génère un ID unique pour l'équipement (GUID sans tirets)
    /// </summary>
    private static string GenerateEquipmentId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Sauvegarde le nouvel équipement en base (INSERT dans Equipements)
    /// </summary>
    private void InsertEquipment()
    {
        if (!ValidateEquipmentForm(out var errorMessage))
        {
            MessageBox.Show(errorMessage);
            return;
        }

        var selectedType = (EquipmentTypeItem?)cbType.SelectedItem;
        if (selectedType == null) return;

        // Créer le DTO avec les données du formulaire
        var equipment = new EquipmentDto(
            IdEquipement: GenerateEquipmentId(),
            TypeId: selectedType.Id,
            Nom: tbName.Text.Trim(),
            CodeParc: tbCodeParc.Text.Trim(),
            NumeroSerie: tbSerial.Text.Trim(),
            Marque: tbBrand.Text.Trim(),
            Commentaire: string.IsNullOrWhiteSpace(tbComment.Text) ? null : tbComment.Text.Trim(),
            EtatPret: 0,  // Disponible par défaut
            Idrh: string.Empty,   // Pas encore assigné
            DateRenduDsem: string.Empty
        );

        try
        {
            // Utiliser le Repository pour l'insertion
            var repo = new EquipmentMySqlRepository();
            repo.Insert(equipment);

            MessageBox.Show("Équipement créé.");

            // Réinitialiser le formulaire
            tbSerial.Clear();
            tbName.Clear();
            tbBrand.Clear();
            tbCodeParc.Clear();
            tbComment.Clear();
            if (cbType.Items.Count > 0) cbType.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la création : {ex.Message}");
        }
    }

    /// <summary>
    /// Handler du bouton Créer
    /// </summary>
    private void btnCreate_Click(object? sender, EventArgs e)
    {
        InsertEquipment();
    }

    /// <summary>
    /// Helper pour ajouter un champ au formulaire
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
