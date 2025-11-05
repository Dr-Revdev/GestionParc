using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProjetParc.Data;

namespace ProjetParc.Views.Equipment;

/// <summary>
/// Écran avec 3 colonnes : équipements disponibles (gauche), rendus DSEM (milieu), détails (droite)
/// Permet de marquer un équipement comme "rendu DSEM" avec une date
/// </summary>
public class FreeEquipmentView : UserControl
{
    // Affichage gauche
    private TextBox tbSearchAvailable;
    private Button btnSearchAvailable;
    private ListView lvAvailable;
    private ListViewColumnSorter lvAvailableSorter;

    // Affichage milieu
    private TextBox tbSearchReturned;
    private Button btnSearchReturned;
    private ListView lvReturned;
    private ListViewColumnSorter lvReturnedSorter;

    // Affichage détail sélection droite
    private TextBox tbType, tbName, tbCodeParc, tbSerial, tbBrand, tbComment;
    private CheckBox cbxRenduDsem;
    private Label lblDateRenduDsem;

    // Action retour parge précedante
    private readonly Action _onBack;


    /// <summary>
    /// Constructeur - monte l'UI, charge les 2 listes, branche tous les événements
    /// </summary>
    /// <param name="onBack">Callback retour</param>
    public FreeEquipmentView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();

        // Chargement des Listes
        LoadAvailable();
        LoadReturned();

        // Recherche
        btnSearchAvailable.Click += (_, __) => LoadAvailable(tbSearchAvailable.Text);
        btnSearchReturned.Click += (_, __) => LoadReturned(tbSearchReturned.Text);

        // Chargement du panneau droit quand sélection d'un item
        lvAvailable.SelectedIndexChanged += lvAvailable_Selected;
        lvReturned.SelectedIndexChanged += lvReturned_Selected;

        // Maj et rafraichisement des 2 listes

        cbxRenduDsem.CheckedChanged += cbxRenduDsem_CheckedChanged;

        // Mise à zéro de la sélection
        lvAvailable.Enter += (_, __) => lvReturned.SelectedItems.Clear();
        lvReturned.Enter += (_, __) => lvAvailable.SelectedItems.Clear();
    }

    /// <summary>
    /// Monte toute l'interface - 3 colonnes (disponible 33%, DSEM 33%, détails 34%)
    /// </summary>
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;
        
        // Layout principal avec en-tête
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(Theme.Spacing.Large),
            BackColor = Theme.Colors.Background
        };

        // Configuration des lignes
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // En-tête
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu

        Controls.Add(mainLayout);

        // En-tête avec bouton retour
        var headerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, Theme.Spacing.Medium),
            BackColor = Theme.Colors.Background
        };
        headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
        headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre

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

        var lblTitle = new Label
        {
            Text = "Gestion des équipements",
            Font = Theme.Fonts.H3,
            ForeColor = Theme.Colors.Primary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(Theme.Spacing.Medium, 0, 0, 0)
        };

        headerPanel.Controls.Add(btnBack, 0, 0);
        headerPanel.Controls.Add(lblTitle, 1, 0);
        mainLayout.Controls.Add(headerPanel, 0, 0);

        // Layout du contenu principal
        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Theme.Colors.Background,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 33),  // Liste disponible
                new ColumnStyle(SizeType.Percent, 33),  // Liste DSEM
                new ColumnStyle(SizeType.Percent, 34)   // Détails
            }
        };

        mainLayout.Controls.Add(contentLayout, 0, 1);

        // Panneau gauche (Disponible)
        TableLayoutPanel leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(Theme.Spacing.Small),
            BackColor = Theme.Colors.Surface,
            Margin = new Padding(0, 0, Theme.Spacing.Small, 0),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 45),  // Titre
                new RowStyle(SizeType.Absolute, 55),  // Recherche
                new RowStyle(SizeType.Percent, 100)   // Liste
            }
        };
        contentLayout.Controls.Add(leftPanel, 0, 0);

        // Titre gauche
        var lblAvailable = new Label 
        { 
            Text = "Disponible", 
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.H5,
            ForeColor = Theme.Colors.Primary,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, Theme.Spacing.Small, 0, 0)
        };
        leftPanel.Controls.Add(lblAvailable, 0, 0);

        // Recherche gauche
        var searchPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(Theme.Spacing.Small),
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 100),
                new ColumnStyle(SizeType.Absolute, 40)
            }
        };
        tbSearchAvailable = new TextBox { Dock = DockStyle.Fill, Font = Theme.Fonts.Body };
        Theme.StyleTextBox(tbSearchAvailable);
        btnSearchAvailable = new Button { Text = "🔍", Width = Theme.Sizes.SearchButtonSize, Height = Theme.Sizes.SearchButtonSize, Dock = DockStyle.Right };
        Theme.StylePrimaryButton(btnSearchAvailable, setHeight: false);
        btnSearchAvailable.Font = new Font("Segoe UI", 12f);
        searchPanel.Controls.Add(tbSearchAvailable, 0, 0);
        searchPanel.Controls.Add(btnSearchAvailable, 1, 0);
        leftPanel.Controls.Add(searchPanel, 0, 1);

        // Liste gauche (ListView avec colonnes)
        lvAvailable = new ListView 
        { 
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Theme.Colors.Surface,
            ForeColor = Theme.Colors.TextPrimary,
            Font = Theme.Fonts.Body,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(Theme.Spacing.Small)
        };
        
        // Colonnes pour équipements : Type | Nom | Code Parc | N° Série
        lvAvailable.Columns.Add("Type", 120);
        lvAvailable.Columns.Add("Code Parc", 100);
        lvAvailable.Columns.Add("N° Série", 100);
        lvAvailable.Columns.Add("Nom", 150);
        
        // Configuration du tri par colonnes
        lvAvailableSorter = new ListViewColumnSorter();
        lvAvailable.ListViewItemSorter = lvAvailableSorter;
        lvAvailable.ColumnClick += (s, e) => {
            lvAvailableSorter.SetSortColumn(e.Column);
            lvAvailable.Sort();
        };
        
        leftPanel.Controls.Add(lvAvailable, 0, 2);

        // Panneau milieu (DSEM)
        TableLayoutPanel middlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(Theme.Spacing.Small),
            BackColor = Theme.Colors.Surface,
            Margin = new Padding(Theme.Spacing.Small, 0, Theme.Spacing.Small, 0),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 45),  // Titre
                new RowStyle(SizeType.Absolute, 55),  // Recherche
                new RowStyle(SizeType.Percent, 100)   // Liste
            }
        };
        contentLayout.Controls.Add(middlePanel, 1, 0);

        // Titre milieu
        var lblReturned = new Label 
        { 
            Text = "Rendu DSEM", 
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.H5,
            ForeColor = Theme.Colors.Secondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, Theme.Spacing.Small, 0, 0)
        };
        middlePanel.Controls.Add(lblReturned, 0, 0);

        // Recherche milieu
        var searchPanel2 = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(Theme.Spacing.Small),
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 100),
                new ColumnStyle(SizeType.Absolute, 40)
            }
        };
        tbSearchReturned = new TextBox { Dock = DockStyle.Fill, Font = Theme.Fonts.Body };
        Theme.StyleTextBox(tbSearchReturned);
        btnSearchReturned = new Button { Text = "🔍", Width = Theme.Sizes.SearchButtonSize, Height = Theme.Sizes.SearchButtonSize, Dock = DockStyle.Right };
        Theme.StylePrimaryButton(btnSearchReturned, setHeight: false);
        btnSearchReturned.Font = new Font("Segoe UI", 12f);
        searchPanel2.Controls.Add(tbSearchReturned, 0, 0);
        searchPanel2.Controls.Add(btnSearchReturned, 1, 0);
        middlePanel.Controls.Add(searchPanel2, 0, 1);

        // Liste milieu (ListView avec colonnes)
        lvReturned = new ListView 
        { 
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Theme.Colors.Surface,
            ForeColor = Theme.Colors.TextPrimary,
            Font = Theme.Fonts.Body,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(Theme.Spacing.Small)
        };
        
        // Colonnes pour équipements : Type | Nom | Code Parc | N° Série
        lvReturned.Columns.Add("Type", 120);
        lvReturned.Columns.Add("Code Parc", 100);
        lvReturned.Columns.Add("N° Série", 100);
        lvReturned.Columns.Add("Nom", 150);
        
        // Configuration du tri par colonnes
        lvReturnedSorter = new ListViewColumnSorter();
        lvReturned.ListViewItemSorter = lvReturnedSorter;
        lvReturned.ColumnClick += (s, e) => {
            lvReturnedSorter.SetSortColumn(e.Column);
            lvReturned.Sort();
        };
        
        middlePanel.Controls.Add(lvReturned, 0, 2);

        // Panneau droit (Détails)
        TableLayoutPanel rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 13,
            ColumnCount = 1,
            Padding = new Padding(Theme.Spacing.Medium),
            BackColor = Theme.Colors.Surface,
            Margin = new Padding(0)
        };
        
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Label Type
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Input Type
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Label Nom
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Input Nom
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Label Code parc
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Input Code parc
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Label Numéro de série
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Input Numéro de série
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Label Marque
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Input Marque
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Checkbox DSEM + Date
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Label Commentaire
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Commentaire multiline
        
        contentLayout.Controls.Add(rightPanel, 2, 0);

        // Labels et TextBox pour les détails
        tbType = new TextBox { ReadOnly = true };
        Theme.StyleTextBox(tbType);
        AddDetailRow(rightPanel, 0, "Type", tbType);
        
        tbName = new TextBox { ReadOnly = true };
        Theme.StyleTextBox(tbName);
        AddDetailRow(rightPanel, 2, "Nom", tbName);
        
        tbCodeParc = new TextBox { ReadOnly = true };
        Theme.StyleTextBox(tbCodeParc);
        AddDetailRow(rightPanel, 4, "Code parc", tbCodeParc);
        
        tbSerial = new TextBox { ReadOnly = true };
        Theme.StyleTextBox(tbSerial);
        AddDetailRow(rightPanel, 6, "Numéro de série", tbSerial);
        
        tbBrand = new TextBox { ReadOnly = true };
        Theme.StyleTextBox(tbBrand);
        AddDetailRow(rightPanel, 8, "Marque", tbBrand);
        
        // Case à cocher DSEM
        var dsemPanel = new FlowLayoutPanel 
        { 
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(Theme.Spacing.Small),
            BackColor = Theme.Colors.SurfaceHover,
            Padding = new Padding(Theme.Spacing.Small),
            WrapContents = false
        };
        var dsemLabel = new Label 
        { 
            Text = "Rendre DSEM", 
            AutoSize = true, 
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextPrimary,
            Margin = new Padding(0, 3, 10, 0)
        };
        dsemPanel.Controls.Add(dsemLabel);
        cbxRenduDsem = new CheckBox { AutoSize = true, Margin = new Padding(0, 3, 5, 0) };
        dsemPanel.Controls.Add(cbxRenduDsem);
        
        // Label pour la date de rendu DSEM (sur la même ligne)
        lblDateRenduDsem = new Label 
        { 
            AutoSize = true, 
            Margin = new Padding(0, 5, 0, 0),
            Font = Theme.Fonts.BodySmall,
            ForeColor = Theme.Colors.TextSecondary,
            Visible = false // Caché par défaut
        };
        dsemPanel.Controls.Add(lblDateRenduDsem);
        
        rightPanel.Controls.Add(dsemPanel, 0, 10);

        // Commentaire
        tbComment = new TextBox { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical };
        Theme.StyleTextBox(tbComment);
        AddDetailRow(rightPanel, 11, "Commentaire", tbComment);

        ResumeLayout(false);
    }
    /// <summary>Handler de la checkbox "Rendu DSEM" - appelle UpdateRenduDsem()</summary>
    private void cbxRenduDsem_CheckedChanged(object sender, EventArgs e) => UpdateRenduDsem();

    /// <summary>
    /// Charge les équipements disponibles (etat_pret = 0) dans la liste de gauche. Filtre optionnel pour la recherche
    /// </summary>
    private void LoadAvailable(string filter = null)
    {
        var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
        var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();

        var equipments = equipmentRepo.GetAll().Where(e => e.EtatPret == 0).ToList();
        var types = typeRepo.GetAll();
        var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

        // Appliquer le filtre si fourni
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLower();
            equipments = equipments.Where(e =>
                (e.Nom?.ToLower().Contains(f) ?? false) ||
                (e.CodeParc?.ToLower().Contains(f) ?? false) ||
                (e.NumeroSerie?.ToLower().Contains(f) ?? false) ||
                (typeDict.ContainsKey(e.TypeId) && typeDict[e.TypeId].ToLower().Contains(f))
            ).ToList();
        }

        // Trier par type puis nom
        var sortedEquipments = equipments
            .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
            .ThenBy(e => e.Nom ?? "");

        lvAvailable.SelectedIndexChanged -= lvAvailable_Selected;
        lvAvailable.Items.Clear();

        foreach (var eq in sortedEquipments)
        {
            var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "";
            var codeParc = string.IsNullOrWhiteSpace(eq.CodeParc) ? "" : eq.CodeParc.Trim();
            var numeroSerie = string.IsNullOrWhiteSpace(eq.NumeroSerie) ? "" : eq.NumeroSerie.Trim();
            var nom = string.IsNullOrWhiteSpace(eq.Nom) ? "(sans nom)" : eq.Nom.Trim();

            var item = new ListViewItem(typeName);
            item.SubItems.Add(codeParc);
            item.SubItems.Add(numeroSerie);
            item.SubItems.Add(nom);
            item.Tag = eq.IdEquipement;

            lvAvailable.Items.Add(item);
        }

        lvAvailable.SelectedIndexChanged += lvAvailable_Selected;
    }

    /// <summary>
    /// Charge les équipements rendus DSEM (etat_pret = 2) dans la liste du milieu. Filtre optionnel
    /// </summary>
    private void LoadReturned(string filter = null)
    {
        try
        {
            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();

            var equipments = equipmentRepo.GetAll().Where(e => e.EtatPret == 2).ToList();
            var types = typeRepo.GetAll();
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

            // Appliquer le filtre si fourni
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var f = filter.Trim().ToLower();
                equipments = equipments.Where(e =>
                    (e.Nom?.ToLower().Contains(f) ?? false) ||
                    (e.CodeParc?.ToLower().Contains(f) ?? false) ||
                    (e.NumeroSerie?.ToLower().Contains(f) ?? false) ||
                    (typeDict.ContainsKey(e.TypeId) && typeDict[e.TypeId].ToLower().Contains(f))
                ).ToList();
            }

            // Trier par type puis nom
            var sortedEquipments = equipments
                .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
                .ThenBy(e => e.Nom ?? "");

            lvReturned.SelectedIndexChanged -= lvReturned_Selected;
            lvReturned.Items.Clear();

            foreach (var eq in sortedEquipments)
            {
                var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "";
                var codeParc = string.IsNullOrWhiteSpace(eq.CodeParc) ? "" : eq.CodeParc.Trim();
                var numeroSerie = string.IsNullOrWhiteSpace(eq.NumeroSerie) ? "" : eq.NumeroSerie.Trim();
                var nom = string.IsNullOrWhiteSpace(eq.Nom) ? "(sans nom)" : eq.Nom.Trim();

                var item = new ListViewItem(typeName);
                item.SubItems.Add(codeParc);
                item.SubItems.Add(numeroSerie);
                item.SubItems.Add(nom);
                item.Tag = eq.IdEquipement;

                lvReturned.Items.Add(item);
            }

            lvReturned.SelectedIndexChanged += lvReturned_Selected;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements rendus : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Récupère un équipement et affiche tous ses détails dans le panneau de droite
    /// </summary>
    private void LoadDetails(string equipmentId)
    {
        try
        {
            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();

            var equipment = equipmentRepo.GetById(equipmentId);
            if (equipment == null)
            {
                MessageBox.Show("Équipement introuvable.");
                return;
            }

            var types = typeRepo.GetAll();
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
            var typeName = typeDict.ContainsKey(equipment.TypeId) ? typeDict[equipment.TypeId] : "";

            tbType.Text = typeName;
            tbName.Text = equipment.Nom ?? "";
            tbCodeParc.Text = equipment.CodeParc ?? "";
            tbSerial.Text = equipment.NumeroSerie ?? "";
            tbBrand.Text = equipment.Marque ?? "";
            tbComment.Text = equipment.Commentaire ?? "";
            cbxRenduDsem.Tag = equipmentId;

            // Gérer la checkbox et afficher la date si DSEM
            cbxRenduDsem.CheckedChanged -= cbxRenduDsem_CheckedChanged;
            cbxRenduDsem.Checked = equipment.EtatPret == 2;
            
            // Afficher la date dans un label séparé si DSEM
            if (equipment.EtatPret == 2 && !string.IsNullOrEmpty(equipment.DateRenduDsem))
            {
                lblDateRenduDsem.Text = $"(Date: {equipment.DateRenduDsem})";
                lblDateRenduDsem.Visible = true;
            }
            else
            {
                lblDateRenduDsem.Visible = false;
            }
            
            cbxRenduDsem.CheckedChanged += cbxRenduDsem_CheckedChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des détails de l'équipement : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Bascule l'état DSEM (coché = rendu DSEM avec date, décoché = disponible sans date)
    /// Ouvre une popup pour demander la date si on coche. Rafraîchit les 2 listes après
    /// </summary>
    private void UpdateRenduDsem()
    {
        if (cbxRenduDsem.Tag is not string id) return;

        var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
        var equipment = equipmentRepo.GetById(id);
        if (equipment == null) return;

        if (cbxRenduDsem.Checked)
        {
            // Demander la date de rendu DSEM
            using var dateDialog = new Form
            {
                Text = "Date de rendu DSEM",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 3,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            var label = new Label
            {
                Text = "Date de rendu :",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var datePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            var btnOk = new Button
            {
                Text = "OK",
                Width = 80,
                Height = 30,
                DialogResult = DialogResult.OK
            };
            Theme.StylePrimaryButton(btnOk);

            var btnCancel = new Button
            {
                Text = "Annuler",
                Width = 80,
                Height = 30,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(0, 0, 10, 0)
            };
            Theme.StyleOutlineButton(btnCancel);

            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);

            layout.Controls.Add(label, 0, 0);
            layout.Controls.Add(datePicker, 0, 1);
            layout.Controls.Add(btnPanel, 0, 2);

            dateDialog.Controls.Add(layout);
            dateDialog.AcceptButton = btnOk;
            dateDialog.CancelButton = btnCancel;

            if (dateDialog.ShowDialog() == DialogResult.OK)
            {
                // Mettre à jour avec la date
                var dateRendu = datePicker.Value.ToString("yyyy-MM-dd");
                var updatedEquipment = equipment with 
                { 
                    EtatPret = 2, 
                    DateRenduDsem = dateRendu 
                };
                equipmentRepo.Update(updatedEquipment);
                
                // Afficher la date dans le label
                lblDateRenduDsem.Text = $"(Date: {dateRendu})";
                lblDateRenduDsem.Visible = true;
            }
            else
            {
                // Annulé, décocher la case
                cbxRenduDsem.CheckedChanged -= cbxRenduDsem_CheckedChanged;
                cbxRenduDsem.Checked = false;
                cbxRenduDsem.CheckedChanged += cbxRenduDsem_CheckedChanged;
                return;
            }
        }
        else
        {
            // Décocher = remettre disponible et enlever la date
            var updatedEquipment = equipment with 
            { 
                EtatPret = 0, 
                DateRenduDsem = null 
            };
            equipmentRepo.Update(updatedEquipment);
            
            // Cacher le label de date
            lblDateRenduDsem.Visible = false;
        }

        //Rafraichir les listes
        LoadAvailable(tbSearchAvailable.Text);
        LoadReturned(tbSearchReturned.Text);
    }

    private void lvAvailable_Selected(object s, EventArgs e)
    {
        if (lvAvailable.SelectedItems.Count > 0)
        {
            // enlève la surbrillance à droite
            lvReturned.SelectedItems.Clear();

            var selectedItem = lvAvailable.SelectedItems[0];
            var id = (string)selectedItem.Tag;
            LoadDetails(id);
        }
    }

    private void lvReturned_Selected(object s, EventArgs e)
    {
        if (lvReturned.SelectedItems.Count > 0)
        {
            // enlève la surbrillance à gauche
            lvAvailable.SelectedItems.Clear();

            var selectedItem = lvReturned.SelectedItems[0];
            var id = (string)selectedItem.Tag;
            LoadDetails(id);
        }
    }

    /// <summary>
    /// Helper pour ajouter un champ dans le panneau de détails
    /// </summary>
    private void AddDetailRow(TableLayoutPanel panel, int row, string labelText, Control control)
    {
        var label = new Label 
        { 
            Text = labelText, 
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary,
            Padding = new Padding(Theme.Spacing.Small, 0, 0, Theme.Spacing.Small)
        };
        panel.Controls.Add(label, 0, row);
        
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(Theme.Spacing.Small, 0, Theme.Spacing.Small, Theme.Spacing.Medium);
        panel.Controls.Add(control, 0, row + 1);
    }
}