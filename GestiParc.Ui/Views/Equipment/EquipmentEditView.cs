using System;
using System.Drawing;
using System.Windows.Forms;
using GestiParc.Ui.Data;
using GestiParc.Ui.Services;
using GestiParc.Infrastructure.Data.Repositories;
using GestiParc.Core.DTOs;

namespace GestiParc.Ui.Views.Equipment;

/// <summary>
/// Écran de modification/suppression des équipements. Liste à gauche avec recherche, formulaire à droite
/// </summary>
public class EquipementEditView : UserControl
{
    private TextBox tbSearch = null!;
    private Button btnSearch = null!;
    private ListView lvEquipment = null!;
    private ListViewColumnSorter lvEquipmentSorter = null!;
    private TextBox tbSerialNumber = null!, tbName = null!, tbBrand = null!, tbCodeParc = null!, tbComment = null!;
    private ComboBox cbType = null!;

    private Button btnUpdate = null!, btnDelete = null!;

    private readonly Action _onBack;

    /// <summary>
    /// Constructeur - monte l'UI, charge les types et la liste des équipements
    /// </summary>
    /// <param name="onBack">Callback retour</param>
    public EquipementEditView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadEquipmentTypes();
        LoadEquipmentList();

        btnSearch.Click += btnSearch_Click;
        lvEquipment.SelectedIndexChanged += lbEquipment_SelectedIndexChanged;
        btnUpdate.Click += (_, __) => SaveEquipmentChanges();
        btnDelete.Click += (_, __) => DeleteSelectedEquipment();
    }

    /// <summary>
    /// Monte toute l'interface - split 30/70 (liste à gauche, formulaire à droite)
    /// </summary>
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;
        Padding = new Padding(20);

        // TableLayoutPanel principal
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Liste à gauche
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2)); // Séparateur
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Formulaire à droite

        // Panneau gauche (recherche et liste)
        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(10),
            BackColor = Theme.Colors.Background
        };

        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Bouton retour
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Recherche
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Liste

        var btnBack = new Button { Text = "← Retour", Dock = DockStyle.Left, Width = 120 };
        Theme.StyleSecondaryButton(btnBack);
        btnBack.Click += (_, __) => _onBack?.Invoke();

        var searchPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 5),
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 100),
                new ColumnStyle(SizeType.Absolute, 40)
            }
        };

        tbSearch = new TextBox { Dock = DockStyle.Fill, Font = Theme.Fonts.Body };
        Theme.StyleTextBox(tbSearch);
        btnSearch = new Button { Text = "🔍", Width = Theme.Sizes.SearchButtonSize, Height = Theme.Sizes.SearchButtonSize, Dock = DockStyle.Right };
        Theme.StyleSecondaryButton(btnSearch, setHeight: false);
        btnSearch.Font = new Font("Segoe UI", 12f);
        searchPanel.Controls.Add(tbSearch, 0, 0);
        searchPanel.Controls.Add(btnSearch, 1, 0);

        lvEquipment = new ListView
        {
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Surface,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        lvEquipment.Columns.Add("Type", 150);
        lvEquipment.Columns.Add("Code Parc", 120);
        lvEquipment.Columns.Add("N° Série", 150);
        lvEquipment.Columns.Add("Nom", 200);
        
        // Configuration du tri par colonnes
        lvEquipmentSorter = new ListViewColumnSorter();
        lvEquipment.ListViewItemSorter = lvEquipmentSorter;
        lvEquipment.ColumnClick += (s, e) => {
            lvEquipmentSorter.SetSortColumn(e.Column);
            lvEquipment.Sort();
        };

        leftPanel.Controls.Add(btnBack, 0, 0);
        leftPanel.Controls.Add(searchPanel, 0, 1);
        leftPanel.Controls.Add(lvEquipment, 0, 2);

        // Séparateur
        var separator = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Colors.Border };

        // Panneau droit (formulaire d'édition)
        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 8,
            ColumnCount = 3,
            Padding = new Padding(20),
            BackColor = Theme.Colors.Surface
        };

        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Label ligne 1
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Input ligne 1
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Label ligne 2
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Input ligne 2
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Label commentaire
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Commentaire multiline - hauteur fixe
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Espacement flexible
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Boutons

        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        // Initialisation des contrôles du formulaire
        cbType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        Theme.StyleComboBox(cbType);
        
        tbName = new TextBox { Dock = DockStyle.Fill };
        Theme.StyleTextBox(tbName);
        
        tbCodeParc = new TextBox { Dock = DockStyle.Fill };
        Theme.StyleTextBox(tbCodeParc);
        
        tbSerialNumber = new TextBox { Dock = DockStyle.Fill };
        Theme.StyleTextBox(tbSerialNumber);
        
        tbBrand = new TextBox { Dock = DockStyle.Fill };
        Theme.StyleTextBox(tbBrand);
        
        tbComment = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
        Theme.StyleTextBox(tbComment);

        // Ajout des labels et contrôles au panneau droit
        AddFormRow(rightPanel, 0, "Type", cbType, 0);
        AddFormRow(rightPanel, 0, "Nom", tbName, 1);
        AddFormRow(rightPanel, 0, "Code parc", tbCodeParc, 2);
        AddFormRow(rightPanel, 2, "Numéro de série", tbSerialNumber, 0);
        AddFormRow(rightPanel, 2, "Marque", tbBrand, 1);

        var commentLabel = new Label 
        { 
            Text = "Commentaire", 
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary,
            Padding = new Padding(5, 0, 0, 5),
            Margin = new Padding(5, 0, 0, 0)
        };
        tbComment.Margin = new Padding(5, 0, 15, 15);
        rightPanel.Controls.Add(commentLabel, 0, 4);
        rightPanel.SetColumnSpan(commentLabel, 3);
        rightPanel.Controls.Add(tbComment, 0, 5);
        rightPanel.SetColumnSpan(tbComment, 3);

        // Boutons d'action
        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Colors.Surface,
            Padding = new Padding(0)
        };

        btnUpdate = new Button { Text = "Modifier", Width = Theme.Sizes.ButtonWidth, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0) };
        Theme.StylePrimaryButton(btnUpdate);
        
        btnDelete = new Button { Text = "Supprimer", Width = Theme.Sizes.ButtonWidth, Height = Theme.Sizes.ButtonHeight, Margin = new Padding(0, 0, 10, 0) };
        Theme.StyleDangerButton(btnDelete);
        
        buttonsPanel.Controls.AddRange([btnUpdate, btnDelete]);

        rightPanel.Controls.Add(buttonsPanel, 0, 7);
        rightPanel.SetColumnSpan(buttonsPanel, 3);

        // Assemblage final
        mainLayout.Controls.Add(leftPanel, 0, 0);
        mainLayout.Controls.Add(separator, 1, 0);
        mainLayout.Controls.Add(rightPanel, 2, 0);

        Controls.Add(mainLayout);
        ResumeLayout(false);

    }

    /// <summary>
    /// Item pour la combobox des types
    /// </summary>
    private sealed class EquipmentTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    /// <summary>
    /// Remplit la combobox des types (PC, Ecran, etc.)
    /// </summary>
    private void LoadEquipmentTypes()
    {
        try
        {
            var types = new EquipmentTypeMySqlRepository().GetAll();

            var items = types
                .Select(t => new EquipmentTypeItem { Id = t.Id, Name = t.Name })
                .OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            cbType.DataSource = items;
            cbType.DisplayMember = nameof(EquipmentTypeItem.Name);
            cbType.ValueMember = nameof(EquipmentTypeItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du chargement des types d'équipement : {ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error
            );
        }
    }
    /// <summary>
    /// Charge tous les équipements et les affiche dans la liste
    /// </summary>
    private void LoadEquipmentList()
    {
        try
        {
            // Récupérer tous les équipements via le Repository
            var repo = new EquipmentMySqlRepository();
            var equipments = repo.GetAll();

            // Récupérer les types pour afficher le nom du type
            var typeRepo = new EquipmentTypeMySqlRepository();
            var types = typeRepo.GetAll().ToDictionary(t => t.Id, t => t.Name);

            // Vider et remplir le ListView
            lvEquipment.Items.Clear();
            foreach (var equipment in equipments)
            {
                var typeName = types.ContainsKey(equipment.TypeId) ? types[equipment.TypeId] : "Inconnu";
                var codeParc = string.IsNullOrWhiteSpace(equipment.CodeParc) ? "-" : equipment.CodeParc.Trim();
                var serial = string.IsNullOrWhiteSpace(equipment.NumeroSerie) ? "-" : equipment.NumeroSerie.Trim();
                var nom = string.IsNullOrWhiteSpace(equipment.Nom) ? "(sans nom)" : equipment.Nom.Trim();

                var item = new ListViewItem(typeName);
                item.SubItems.AddRange(new[] { codeParc, serial, nom });
                item.Tag = equipment.IdEquipement;
                lvEquipment.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    /// <summary>
    /// Récupère un équipement depuis la base et remplit tous les champs du formulaire
    /// </summary>
    private void LoadEquipmentById(string equipmentId)
    {
        try
        {
            var repo = new EquipmentMySqlRepository();
            var equipment = repo.GetById(equipmentId);

            // Remplir les champs (gérer les valeurs null)
            tbName.Text = equipment.Nom ?? "";
            tbCodeParc.Text = equipment.CodeParc ?? "";
            tbSerialNumber.Text = equipment.NumeroSerie ?? "";
            tbBrand.Text = equipment.Marque ?? "";
            tbComment.Text = equipment.Commentaire ?? "";

            // Sélectionner le type correspondant dans la ComboBox
            for (int i = 0; i < cbType.Items.Count; i++)
            {
                if (cbType.Items[i] is EquipmentTypeItem t && t.Id == equipment.TypeId)
                {
                    cbType.SelectedIndex = i;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de l'équipement : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Quand on clique sur un équipement dans la liste, on charge ses infos
    /// </summary>
    private void lbEquipment_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lvEquipment.SelectedItems.Count > 0)
        {
            var selectedItem = lvEquipment.SelectedItems[0];
            var equipmentId = selectedItem.Tag as string;
            if (equipmentId != null) LoadEquipmentById(equipmentId);
        }
    }

    /// <summary>
    /// Vérifie que le nom, le type et le code parc sont bien remplis
    /// </summary>
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
    /// Sauvegarde les modifs en base (UPDATE). Garde l'état de prêt et l'agent actuel tel quel
    /// </summary>
    private void SaveEquipmentChanges()
    {
        if (lvEquipment.SelectedItems.Count == 0)
        {
            MessageBox.Show("Choisir d'abord un équipement.");
            return;
        }
        if (!ValidateEquipmentForm(out var msg))
        {
            MessageBox.Show(msg);
            return;
        }

        var selectedItem = lvEquipment.SelectedItems[0];
        var equipmentId = selectedItem.Tag as string;
        var selectedType = cbType.SelectedItem as EquipmentTypeItem;
        if (equipmentId == null || selectedType == null) return;

        try
        {
            var repo = new EquipmentMySqlRepository();
            
            // Charger l'équipement existant pour récupérer les champs non modifiables
            var existingEquipment = repo.GetById(equipmentId);

            // Créer un DTO avec les valeurs modifiées + les champs préservés
            var equipment = new EquipmentDto(
                IdEquipement: equipmentId,
                TypeId: selectedType.Id,
                Nom: tbName.Text.Trim(),
                CodeParc: tbCodeParc.Text.Trim(),
                NumeroSerie: tbSerialNumber.Text.Trim(),
                Marque: tbBrand.Text.Trim(),
                Commentaire: string.IsNullOrWhiteSpace(tbComment.Text) ? null : tbComment.Text.Trim(),
                EtatPret: existingEquipment.EtatPret,       // Préserver l'état de prêt
                Idrh: existingEquipment.Idrh,               // Préserver l'agent
                DateRenduDsem: existingEquipment.DateRenduDsem // Préserver la date de rendu
            );

            // Appeler le repository pour la mise à jour
            repo.Update(equipment);

            MessageBox.Show("Modifications enregistrées.");

            // Recharger la liste pour refléter les modifications
            LoadEquipmentList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    /// <summary>
    /// Handler du bouton recherche - applique le filtre
    /// </summary>
    private void btnSearch_Click(object? sender, EventArgs e)
    {
        var q = (tbSearch?.Text ?? "").Trim();
        LoadEquipmentListFiltered(q);
    }

    /// <summary>
    /// Charge les équipements avec un filtre de recherche (cherche dans nom, code parc, n° série, type)
    /// </summary>
    private void LoadEquipmentListFiltered(string query)
    {
        try
        {
            var repo = new EquipmentMySqlRepository();
            var typeRepo = new EquipmentTypeMySqlRepository();

            // Charger les équipements et les types
            var equipments = repo.GetAll();
            var types = typeRepo.GetAll();
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

            lvEquipment.Items.Clear();

            // Filtrer les équipements si une requête est fournie
            IEnumerable<EquipmentDto> filteredEquipments = equipments;
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.ToLower();
                filteredEquipments = equipments.Where(e =>
                    (e.Nom?.ToLower().Contains(q) ?? false) ||
                    (e.CodeParc?.ToLower().Contains(q) ?? false) ||
                    (e.NumeroSerie?.ToLower().Contains(q) ?? false) ||
                    (typeDict.ContainsKey(e.TypeId) && typeDict[e.TypeId].ToLower().Contains(q))
                );
            }

            // Trier par type, code_parc, numéro de série
            var sortedEquipments = filteredEquipments
                .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
                .ThenBy(e => e.CodeParc ?? "")
                .ThenBy(e => e.NumeroSerie ?? "");

            // Remplir la ListView
            foreach (var eq in sortedEquipments)
            {
                var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";
                var code = string.IsNullOrWhiteSpace(eq.CodeParc) ? "-" : eq.CodeParc.Trim();
                var serial = string.IsNullOrWhiteSpace(eq.NumeroSerie) ? "-" : eq.NumeroSerie.Trim();
                var nom = string.IsNullOrWhiteSpace(eq.Nom) ? "(sans nom)" : eq.Nom.Trim();

                var item = new ListViewItem(typeName);
                item.SubItems.AddRange(new[] { code, serial, nom });
                item.Tag = eq.IdEquipement;
                lvEquipment.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche d'équipements : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Supprime l'équipement (demande confirmation avant)
    /// </summary>
    private void DeleteSelectedEquipment()
    {
        if (lvEquipment.SelectedItems.Count == 0)
        { MessageBox.Show("Sélectionne un équipement à supprimer."); return; }

        var selectedItem = lvEquipment.SelectedItems[0];
        var equipmentId = selectedItem.Tag as string;
        if (equipmentId == null) return;
        var equipmentLabel = $"{selectedItem.SubItems[3].Text} [{selectedItem.Text}]";
        
        var confirm = MessageBox.Show(
            $"Supprimer « {equipmentLabel} » ?",
            "Confirmer la suppression",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            var repo = new EquipmentMySqlRepository();
            repo.Delete(equipmentId);

            LoadEquipmentListFiltered(tbSearch?.Text?.Trim() ?? "");
            tbName.Clear(); tbCodeParc.Clear(); tbSerialNumber.Clear(); tbBrand.Clear(); tbComment.Clear();
            MessageBox.Show("Équipement supprimé.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Helper pour ajouter un champ dans le formulaire (label + contrôle)
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
        }
    }

}
