using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;

namespace ProjetParc.Views.Equipment;

/// <summary>
/// Vue affichant les équipements "libres" (disponibles) et ceux "rendus" (prêts).
/// Fournit des filtres de recherche, l'affichage des détails et la bascule "Rendre DSEM".
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

    // Action retour parge précedante
    private readonly Action _onBack;


    /// <summary>
    /// Initialise la vue des équipements disponibles et rendus.
    /// Charge l'UI, initialise les listes et attache les gestionnaires d'événements.
    /// </summary>
    /// <param name="onBack">Callback pour revenir à la vue précédente.</param>
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
        lvAvailable.SelectedIndexChanged += LbAvailable_Selected;
        lvReturned.SelectedIndexChanged += LbReturned_Selected;

        // Maj et rafraichisement des 2 listes

        cbxRenduDsem.CheckedChanged += CbxRenduDsem_CheckedChanged;

        // Mise à zéro de la sélection
        lvAvailable.Enter += (_, __) => lvReturned.SelectedItems.Clear();
        lvReturned.Enter += (_, __) => lvAvailable.SelectedItems.Clear();
    }

    /// <summary>
    /// Construit et positionne les contrôles de l'interface utilisateur (colonnes gauche/milieu/droite).
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
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Checkbox DSEM
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
            Dock = DockStyle.Fill, 
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(Theme.Spacing.Small),
            BackColor = Theme.Colors.SurfaceHover,
            Padding = new Padding(Theme.Spacing.Small)
        };
        var dsemLabel = new Label 
        { 
            Text = "Rendre DSEM", 
            AutoSize = true, 
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextPrimary
        };
        dsemPanel.Controls.Add(dsemLabel);
        cbxRenduDsem = new CheckBox { AutoSize = true, Margin = new Padding(10, 3, 0, 0) };
        dsemPanel.Controls.Add(cbxRenduDsem);
        rightPanel.Controls.Add(dsemPanel, 0, 10);

        // Commentaire
        tbComment = new TextBox { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical };
        Theme.StyleTextBox(tbComment);
        AddDetailRow(rightPanel, 11, "Commentaire", tbComment);

        ResumeLayout(false);
    }
    /// <summary>
    /// Gestionnaire d'événement pour le CheckBox "Rendu DSEM" qui délègue vers <see cref="UpdateRenduDsem"/>.
    /// </summary>
    private void CbxRenduDsem_CheckedChanged(object sender, EventArgs e) => UpdateRenduDsem();

    /// <summary>
    /// Charge et remplit la colonne des équipements disponibles (non prêtés).
    /// Accepte un filtre optionnel pour la recherche texte.
    /// </summary>
    private void LoadAvailable(string filter = null)
    {
        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();

        if (string.IsNullOrWhiteSpace(filter))
        {
            command.CommandText = @"
                SELECT e.id_equipement,
                        t.name AS type,
                        COALESCE(TRIM(e.code_parc),'') AS code_parc,
                        COALESCE(TRIM(e.numero_serie),'') AS numero_serie,
                        COALESCE(TRIM(e.nom),'(sans nom)') AS nom
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE COALESCE(e.etat_pret,0) = 0
                ORDER BY t.name, e.nom;";
        }
        else
        {
            command.CommandText = @"
                SELECT e.id_equipement,
                        t.name AS type,
                        COALESCE(TRIM(e.code_parc),'') AS code_parc,
                        COALESCE(TRIM(e.numero_serie),'') AS numero_serie,
                        COALESCE(TRIM(e.nom),'(sans nom)') AS nom
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE COALESCE(e.etat_pret,0) = 0
                    AND (e.nom LIKE $p OR e.code_parc LIKE $p OR e.numero_serie LIKE $p OR t.name LIKE $p)
                ORDER BY t.name, e.nom;";
            command.Parameters.AddWithValue("$p", $"%{filter.Trim()}%");
        }

        using var r = command.ExecuteReader();
        
        lvAvailable.SelectedIndexChanged -= LbAvailable_Selected;
        lvAvailable.Items.Clear();
        
        while (r.Read())
        {
            var id = r.GetString(0);
            var type = r.GetString(1);
            var codeParc = r.GetString(2);
            var numeroSerie = r.GetString(3);
            var nom = r.GetString(4);
            
            var item = new ListViewItem(type);
            item.SubItems.Add(codeParc);
            item.SubItems.Add(numeroSerie);
            item.SubItems.Add(nom);
            item.Tag = id;
            
            lvAvailable.Items.Add(item);
        }
        
        lvAvailable.SelectedIndexChanged += LbAvailable_Selected;
    }

    /// <summary>
    /// Charge et remplit la colonne des équipements rendus (prêts).
    /// Accepte un filtre optionnel pour la recherche texte.
    /// </summary>
    private void LoadReturned(string filter = null)
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();

            if (string.IsNullOrWhiteSpace(filter))
                {
                    command.CommandText = @"
                    SELECT e.id_equipement,
                            t.name AS type,
                            COALESCE(TRIM(e.code_parc),'') AS code_parc,
                            COALESCE(TRIM(e.numero_serie),'') AS numero_serie,
                            COALESCE(TRIM(e.nom),'(sans nom)') AS nom
                    FROM ""Equipements"" e
                    JOIN equipment_type t ON t.id = e.type_id
                    WHERE COALESCE(e.etat_pret,0) = 2  -- DSEM uniquement
                    ORDER BY t.name, e.nom;";
            }
            else
            {
                command.CommandText = @"
                    SELECT e.id_equipement,
                            t.name AS type,
                            COALESCE(TRIM(e.code_parc),'') AS code_parc,
                            COALESCE(TRIM(e.numero_serie),'') AS numero_serie,
                            COALESCE(TRIM(e.nom),'(sans nom)') AS nom
                    FROM ""Equipements"" e
                    JOIN equipment_type t ON t.id = e.type_id
                    WHERE COALESCE(e.etat_pret,0) = 2  -- DSEM uniquement
                        AND (e.nom LIKE $p OR e.code_parc LIKE $p OR e.numero_serie LIKE $p OR t.name LIKE $p)
                    ORDER BY t.name, e.nom;";
                command.Parameters.AddWithValue("$p", $"%{filter.Trim()}%");
            }

            using var r = command.ExecuteReader();
            
            lvReturned.SelectedIndexChanged -= LbReturned_Selected;
            lvReturned.Items.Clear();
            
            while (r.Read())
            {
                var id = r.GetString(0);
                var type = r.GetString(1);
                var codeParc = r.GetString(2);
                var numeroSerie = r.GetString(3);
                var nom = r.GetString(4);
                
                var item = new ListViewItem(type);
                item.SubItems.Add(codeParc);
                item.SubItems.Add(numeroSerie);
                item.SubItems.Add(nom);
                item.Tag = id;
                
                lvReturned.Items.Add(item);
            }
            
            lvReturned.SelectedIndexChanged += LbReturned_Selected;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements rendus : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Charge les détails d'un équipement (type, nom, code parc, série, marque, commentaire)
    /// et met à jour les champs d'affichage à droite.
    /// </summary>
    /// <param name="equipmentId">Identifiant de l'équipement à afficher.</param>
    private void LoadDetails(string equipmentId)
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = @"
                SELECT e.type_id, t.name, e.nom, e.code_parc, e.numero_serie, e.marque, e.commentaire, COALESCE(e.etat_pret,0)
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE e.id_equipement = $id;";
            command.Parameters.AddWithValue("$id", equipmentId);

            using var r = command.ExecuteReader();
            if (!r.Read()) { MessageBox.Show("Équipement introuvable."); return; }

            tbType.Text = r.IsDBNull(1) ? "" : r.GetString(1);
            tbName.Text = r.IsDBNull(2) ? "" : r.GetString(2);
            tbCodeParc.Text = r.IsDBNull(3) ? "" : r.GetString(3);
            tbSerial.Text = r.IsDBNull(4) ? "" : r.GetString(4);
            tbBrand.Text = r.IsDBNull(5) ? "" : r.GetString(5);
            tbComment.Text = r.IsDBNull(6) ? "" : r.GetString(6);
            cbxRenduDsem.Tag = equipmentId;

            cbxRenduDsem.CheckedChanged -= CbxRenduDsem_CheckedChanged;
            cbxRenduDsem.Checked = r.GetInt32(7) != 0;
            cbxRenduDsem.CheckedChanged += CbxRenduDsem_CheckedChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des détails de l'équipement : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Bascule l'état "Rendu DSEM" d'un équipement sélectionné et met à jour la base.
    /// Rafraîchit ensuite les deux listes (disponible / rendu).
    /// </summary>
    private void UpdateRenduDsem()
    {
        if (cbxRenduDsem.Tag is not string id) return;

        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = @"UPDATE ""Equipements"" SET etat_pret = $v WHERE id_equipement = $id;";
        command.Parameters.AddWithValue("$v", cbxRenduDsem.Checked ? 2 : 0); // 2 pour DSEM, 0 pour disponible
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();

        //Rafraichir les listes
        LoadAvailable(tbSearchAvailable.Text);
        LoadReturned(tbSearchReturned.Text);
    }

    private void LbAvailable_Selected(object s, EventArgs e)
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

    private void LbReturned_Selected(object s, EventArgs e)
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
    /// Ajoute une ligne de détail au panneau de droite avec un label et un contrôle
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