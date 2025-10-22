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
    private ListBox lbAvailabe;

    // Affichage milieu
    private TextBox tbSearchReturned;
    private Button btnSearchReturned;
    private ListBox lbReturned;

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
        lbAvailabe.SelectedIndexChanged += LbAvailable_Selected;
        lbReturned.SelectedIndexChanged += LbReturned_Selected;

        // Maj et rafraichisement des 2 listes

        cbxRenduDsem.CheckedChanged += CbxRenduDsem_CheckedChanged;

        // Mise à zéro de la sélection
        lbAvailabe.Enter += (_, __) => lbReturned.ClearSelected();
        lbReturned.Enter += (_, __) => lbAvailabe.ClearSelected();
    }

    /// <summary>
    /// Construit et positionne les contrôles de l'interface utilisateur (colonnes gauche/milieu/droite).
    /// </summary>
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 11f, FontStyle.Regular);
        
        // Layout principal avec en-tête
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };

        // Configuration des lignes
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // En-tête
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu

        Controls.Add(mainLayout);

        // En-tête avec bouton retour
        var headerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 10)
        };
        headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
        headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre

        var btnBack = new Button { Text = "← Retour", Height = 36, Width = 120, Dock = DockStyle.Left };
        btnBack.Click += (_, __) => _onBack?.Invoke();

        var lblTitle = new Label
        {
            Text = "Gestion des équipements",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
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
            Padding = new Padding(5),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 40),  // Titre
                new RowStyle(SizeType.Absolute, 40),  // Recherche
                new RowStyle(SizeType.Percent, 100)   // Liste
            }
        };
        contentLayout.Controls.Add(leftPanel, 0, 0);

        // Titre gauche
        var lblAvailable = new Label { Text = "Disponible", Dock = DockStyle.Fill };
        leftPanel.Controls.Add(lblAvailable, 0, 0);

        // Recherche gauche
        var searchPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 85),
                new ColumnStyle(SizeType.Percent, 15)
            }
        };
        tbSearchAvailable = new TextBox { Dock = DockStyle.Fill, Height = 32 };
        btnSearchAvailable = new Button { Text = "🔍", Dock = DockStyle.Fill };
        searchPanel.Controls.Add(tbSearchAvailable, 0, 0);
        searchPanel.Controls.Add(btnSearchAvailable, 1, 0);
        leftPanel.Controls.Add(searchPanel, 0, 1);

        // Liste gauche
        lbAvailabe = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        leftPanel.Controls.Add(lbAvailabe, 0, 2);

        // Panneau milieu (DSEM)
        TableLayoutPanel middlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(5),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 40),  // Titre
                new RowStyle(SizeType.Absolute, 40),  // Recherche
                new RowStyle(SizeType.Percent, 100)   // Liste
            }
        };
        mainLayout.Controls.Add(middlePanel, 1, 0);

        // Titre milieu
        var lblReturned = new Label { Text = "Rendu DSEM", Dock = DockStyle.Fill };
        middlePanel.Controls.Add(lblReturned, 0, 0);

        // Recherche milieu
        var searchPanel2 = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 85),
                new ColumnStyle(SizeType.Percent, 15)
            }
        };
        tbSearchReturned = new TextBox { Dock = DockStyle.Fill, Height = 32 };
        btnSearchReturned = new Button { Text = "🔍", Dock = DockStyle.Fill };
        searchPanel2.Controls.Add(tbSearchReturned, 0, 0);
        searchPanel2.Controls.Add(btnSearchReturned, 1, 0);
        contentLayout.Controls.Add(middlePanel, 1, 0);

        // Liste milieu
        lbReturned = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        middlePanel.Controls.Add(lbReturned, 0, 2);

        // Panneau droit (Détails)
        TableLayoutPanel rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 14,
            ColumnCount = 1,
            Padding = new Padding(10),
            AutoScroll = true
        };
        contentLayout.Controls.Add(rightPanel, 2, 0);



        // Labels et TextBox pour les détails
        AddDetailRow(rightPanel, 1, "Type", tbType = new TextBox { ReadOnly = true });
        AddDetailRow(rightPanel, 3, "Nom", tbName = new TextBox { ReadOnly = true });
        AddDetailRow(rightPanel, 5, "Code parc", tbCodeParc = new TextBox { ReadOnly = true });
        AddDetailRow(rightPanel, 7, "Numéro de série", tbSerial = new TextBox { ReadOnly = true });
        AddDetailRow(rightPanel, 9, "Marque", tbBrand = new TextBox { ReadOnly = true });
        
        // Case à cocher DSEM
        var dsemPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        dsemPanel.Controls.Add(new Label { Text = "Rendre DSEM", AutoSize = true });
        cbxRenduDsem = new CheckBox { AutoSize = true };
        dsemPanel.Controls.Add(cbxRenduDsem);
        rightPanel.Controls.Add(dsemPanel);
        rightPanel.SetRow(dsemPanel, 11);

        // Commentaire
        AddDetailRow(rightPanel, 13, "Commentaire", tbComment = new TextBox { ReadOnly = true, Multiline = true, Height = 160, ScrollBars = ScrollBars.Vertical });

        ResumeLayout(false);
    }
    /// <summary>
    /// Gestionnaire d'événement pour le CheckBox "Rendu DSEM" qui délègue vers <see cref="UpdateRenduDsem"/>.
    /// </summary>
    private void CbxRenduDsem_CheckedChanged(object sender, EventArgs e) => UpdateRenduDsem();

    private sealed class EquipmentListItem
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public override string ToString() => Label;
    }

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
                        COALESCE(TRIM(e.nom),'(sans nom)') AS n,
                        TRIM(COALESCE(e.code_parc,''))     AS c,
                        t.name                              AS type
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE COALESCE(e.etat_pret,0) = 0
                ORDER BY n, c;";
        }
        else
        {
            command.CommandText = @"
                SELECT e.id_equipement,
                        COALESCE(TRIM(e.nom),'(sans nom)') AS n,
                        TRIM(COALESCE(e.code_parc,''))     AS c,
                        t.name                              AS type
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE COALESCE(e.etat_pret,0) = 0
                    AND (e.nom LIKE $p OR e.code_parc LIKE $p OR e.numero_serie LIKE $p OR t.name LIKE $p)
                ORDER BY n, c;";
            command.Parameters.AddWithValue("$p", $"%{filter.Trim()}%");
        }

        using var r = command.ExecuteReader();
        var items = new List<EquipmentListItem>();
        while (r.Read())
        {
            var id = r.GetString(0);
            var n = r.GetString(1);
            var c = r.GetString(2);
            var ty = r.GetString(3);
            var label = string.IsNullOrEmpty(c) ? $"{n} | {ty}" : $"{n} | {c} | {ty}";
            items.Add(new EquipmentListItem { Id = id, Label = label });
        }
        lbAvailabe.SelectedIndexChanged -= LbAvailable_Selected;
        lbAvailabe.BeginUpdate();
        lbAvailabe.DataSource = items;
        lbAvailabe.SelectedIndex = -1;
        lbAvailabe.EndUpdate();
        lbAvailabe.SelectedIndexChanged += LbAvailable_Selected;
    }

    /// <summary>
    /// Charge et remplit la colonne des équipements rendus (prêts).
    /// Accepte un filtre optionnel pour la recherche texte.
    /// </summary>
    private void LoadReturned(string filter = null)
    {
        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();

        if (string.IsNullOrWhiteSpace(filter))
            {
                command.CommandText = @"
                SELECT e.id_equipement,
                        COALESCE(TRIM(e.nom),'(sans nom)') AS n,
                        TRIM(COALESCE(e.code_parc,''))     AS c,
                        t.name                              AS type
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE COALESCE(e.etat_pret,0) = 2  -- DSEM uniquement
                ORDER BY n, c;";
        }
        else
        {
            command.CommandText = @"
                SELECT e.id_equipement,
                        COALESCE(TRIM(e.nom),'(sans nom)') AS n,
                        TRIM(COALESCE(e.code_parc,''))     AS c,
                        t.name                              AS type
                FROM ""Equipements"" e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE COALESCE(e.etat_pret,0) = 2  -- DSEM uniquement
                    AND (e.nom LIKE $p OR e.code_parc LIKE $p OR e.numero_serie LIKE $p OR t.name LIKE $p)
                ORDER BY n, c;";
            command.Parameters.AddWithValue("$p", $"%{filter.Trim()}%");
        }

        using var r = command.ExecuteReader();
        var items = new List<EquipmentListItem>();
        while (r.Read())
        {
            var id = r.GetString(0);
            var n = r.GetString(1);
            var c = r.GetString(2);
            var ty = r.GetString(3);
            var label = string.IsNullOrEmpty(c) ? $"{n} | {ty}" : $"{n} | {c} | {ty}";
            items.Add(new EquipmentListItem { Id = id, Label = label });
        }
        lbReturned.SelectedIndexChanged -= LbReturned_Selected;
        lbReturned.BeginUpdate();
        lbReturned.DataSource = items;
        lbReturned.SelectedIndex = -1;
        lbReturned.EndUpdate();
        lbReturned.SelectedIndexChanged += LbReturned_Selected;
    }

    /// <summary>
    /// Charge les détails d'un équipement (type, nom, code parc, série, marque, commentaire)
    /// et met à jour les champs d'affichage à droite.
    /// </summary>
    /// <param name="equipmentId">Identifiant de l'équipement à afficher.</param>
    private void LoadDetails(string equipmentId)
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
        if (lbAvailabe.SelectedItem is EquipmentListItem it)
        {
            // enlève la surbrillance à droite
            if (lbReturned.SelectedIndex != -1) lbReturned.SelectedIndex = -1;

            LoadDetails(it.Id);
        }
    }

    private void LbReturned_Selected(object s, EventArgs e)
    {
        if (lbReturned.SelectedItem is EquipmentListItem it)
        {
            // enlève la surbrillance à gauche
            if (lbAvailabe.SelectedIndex != -1) lbAvailabe.SelectedIndex = -1;

            LoadDetails(it.Id);
        }
    }

    /// <summary>
    /// Ajoute une ligne de détail au panneau de droite avec un label et un contrôle
    /// </summary>
    private void AddDetailRow(TableLayoutPanel panel, int row, string labelText, Control control)
    {
        var label = new Label { Text = labelText, Dock = DockStyle.Fill };
        panel.Controls.Add(label, 0, row);
        
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 0, row + 1);
    }
}