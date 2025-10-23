using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;

namespace ProjetParc.Views.Equipment;

/// <summary>
/// Vue permettant la modification et la suppression des équipements existants
/// </summary>
public class EquipementEditView : UserControl
{
    private TextBox tbSearch;
    private Button btnSearch;
    private ListBox lbEquipment;
    private TextBox tbSerialNumber, tbName, tbBrand, tbCodeParc, tbComment;
    private ComboBox cbType;

    private Button btnUpdate, btnDelete;

    private readonly Action _onBack;

    /// <summary>
    /// Initialise la vue d'édition d'équipement.
    /// Charge l'interface graphique, les types et la liste d'équipements.
    /// </summary>
    /// <param name="onBack">Callback exécuté lorsqu'on demande le retour à la vue précédente.</param>
    public EquipementEditView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadEquipmentTypes();
        LoadEquipmentList();

        btnSearch.Click += btnSearch_Click;
        lbEquipment.SelectedIndexChanged += lbEquipment_SelectedIndexChanged;
        btnUpdate.Click += (_, __) => SaveEquipmentChanges();
        btnDelete.Click += (_, __) => DeleteSelectedEquipment();
    }

    /// <summary>
    /// Construit et positionne les contrôles UI pour la vue d'édition.
    /// Cette méthode ne touche pas à la logique métier, elle agencement uniquement.
    /// </summary>
    private void BuildUi()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 11f);
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
            Padding = new Padding(10)
        };

        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Bouton retour
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Recherche
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Liste

        var btnBack = new Button { Text = "← Retour", Dock = DockStyle.Left, Height = 36, Width = 120 };
        btnBack.Click += (_, __) => _onBack?.Invoke();

        var searchPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 85),
                new ColumnStyle(SizeType.Absolute, 40)
            }
        };

        tbSearch = new TextBox { Dock = DockStyle.Fill };
        btnSearch = new Button { Dock = DockStyle.Fill, Text = "🔍" };
        searchPanel.Controls.Add(tbSearch, 0, 0);
        searchPanel.Controls.Add(btnSearch, 1, 0);

        lbEquipment = new ListBox { Dock = DockStyle.Fill };

        leftPanel.Controls.Add(btnBack, 0, 0);
        leftPanel.Controls.Add(searchPanel, 0, 1);
        leftPanel.Controls.Add(lbEquipment, 0, 2);

        // Séparateur
        var separator = new Panel { Dock = DockStyle.Fill, BackColor = Color.Silver };

        // Panneau droit (formulaire d'édition)
        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 8,
            ColumnCount = 3,
            Padding = new Padding(20)
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
        tbName = new TextBox { Dock = DockStyle.Fill };
        tbCodeParc = new TextBox { Dock = DockStyle.Fill };
        tbSerialNumber = new TextBox { Dock = DockStyle.Fill };
        tbBrand = new TextBox { Dock = DockStyle.Fill };
        tbComment = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };

        // Ajout des labels et contrôles au panneau droit
        void AddFormRow(string label, Control control, int col, int labelRow)
        {
            var labelControl = new Label 
            { 
                Text = label, 
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Padding = new Padding(5, 0, 0, 5),
                Margin = new Padding(5, 0, 15, 0)
            };
            control.Margin = new Padding(5, 0, 15, 15);
            rightPanel.Controls.Add(labelControl, col, labelRow);
            rightPanel.Controls.Add(control, col, labelRow + 1);
        }

        AddFormRow("Type", cbType, 0, 0);
        AddFormRow("Nom", tbName, 1, 0);
        AddFormRow("Code parc", tbCodeParc, 2, 0);
        AddFormRow("Numéro de série", tbSerialNumber, 0, 2);
        AddFormRow("Marque", tbBrand, 1, 2);

        var commentLabel = new Label 
        { 
            Text = "Commentaire", 
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
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
            WrapContents = false
        };

        btnUpdate = new Button { Text = "Modifier", Width = 120, Height = 40 };
        btnDelete = new Button { Text = "Supprimer", Width = 120, Height = 40, Margin = new Padding(10, 0, 0, 0) };
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
    /// Éléments affichés dans la liste des équipements. Contient les informations utiles pour
    /// l'affichage et la récupération de l'identifiant réel stocké en base.
    /// </summary>
    private sealed class EquipmentListItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Type { get; set; } = "";

        /// <summary>Label calculé pour l'affichage dans la liste.</summary>
        public string Label => string.IsNullOrEmpty(Code) ? $"{Name}  {Type}" : $"{Name}  [{Code}]  {Type}";
        public override string ToString() => Label;
    }
    /// <summary>
    /// Représentation minimale d'un type d'équipement (pour les ComboBox).
    /// </summary>
    private sealed class EquipmentTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    /// <summary>
    /// Convertit une chaîne vide en <see cref="DBNull.Value"/> pour insertion en base.
    /// </summary>
    /// <param name="s">Chaîne source.</param>
    /// <returns>Chaîne trimée ou <see cref="DBNull.Value"/> si vide.</returns>
    private static object ToDbNullable(string s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();

    /// <summary>
    /// Charge les types d'équipement depuis la table <c>equipment_type</c> et renseigne la ComboBox.
    /// Utilise une connexion SQLite via <see cref="Database.Open"/>.
    /// </summary>
    private void LoadEquipmentTypes()
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = "SELECT id, name FROM equipment_type ORDER BY name;";

            using var reader = command.ExecuteReader();
            var items = new List<EquipmentTypeItem>();
            while (reader.Read())
                items.Add(new EquipmentTypeItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });

            cbType.DataSource = items;
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
    /// Charge la liste complète des équipements depuis la base et alimente la ListBox.
    /// Utilise un tri insensible à la casse pour l'affichage.
    /// </summary>
    private void LoadEquipmentList()
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = @"
                SELECT e.id_equipement,
                    COALESCE(TRIM(e.nom), '(sans nom)') AS n,
                    TRIM(COALESCE(e.code_parc, ''))     AS c,
                    t.name AS typ
                FROM Equipements e
                JOIN equipment_type t ON t.id = e.type_id
                ORDER BY n COLLATE NOCASE, typ COLLATE NOCASE, c COLLATE NOCASE;";

            using var reader = command.ExecuteReader();
            var list = new List<EquipmentListItem>();
            while (reader.Read())
            {
                list.Add(new EquipmentListItem
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Code = reader.GetString(2),
                    Type = reader.GetString(3)
                });
            }

            lbEquipment.DataSource = list;
            lbEquipment.DisplayMember = nameof(EquipmentListItem.Label);
            lbEquipment.ValueMember = nameof(EquipmentListItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement de la liste d'équipements : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    /// <summary>
    /// Charge les détails d'un équipement identifié par <paramref name="equipmentId"/>
    /// et renseigne les champs du formulaire de modification.
    /// </summary>
    /// <param name="equipmentId">Identifiant (id_equipement) de l'équipement à charger.</param>
    private void LoadEquipmentById(string equipmentId)
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();
            command.CommandText = @"SELECT type_id, nom, code_parc, numero_serie, marque, commentaire FROM ""Equipements"" WHERE id_equipement = $id;";
            command.Parameters.AddWithValue("$id", equipmentId);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                MessageBox.Show("Équipement introuvable.");
                return;
            }

            var typeId = reader.GetInt32(0);
            tbName.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
            tbCodeParc.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
            tbSerialNumber.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
            tbBrand.Text = reader.IsDBNull(4) ? "" : reader.GetString(4);
            tbComment.Text = reader.IsDBNull(5) ? "" : reader.GetString(5);

            for (int i = 0; i < cbType.Items.Count; i++)
            {
                if (cbType.Items[i] is EquipmentTypeItem t && t.Id == typeId)
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
    /// Événement déclenché lors du changement de sélection dans la ListBox.
    /// Charge les détails de l'élément sélectionné.
    /// </summary>
    private void lbEquipment_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lbEquipment.SelectedItem is EquipmentListItem item)
            LoadEquipmentById(item.Id);
    }

    /// <summary>
    /// Valide les champs obligatoires du formulaire d'édition.
    /// </summary>
    /// <param name="errorMessage">Retourne un message d'erreur en cas d'échec.</param>
    /// <returns>True si le formulaire est valide, false sinon.</returns>
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
    /// Enregistre les modifications effectuées sur l'équipement sélectionné en base.
    /// Effectue la validation avant mise à jour et affiche des messages d'erreur en cas de problème.
    /// </summary>
    private void SaveEquipmentChanges()
    {
        if (lbEquipment.SelectedItem is not EquipmentListItem selected)
        {
            MessageBox.Show("Choisir d'abord un équipement.");
            return;
        }
        if (!ValidateEquipmentForm(out var msg))
        {
            MessageBox.Show(msg);
            return;
        }

        var selectedType = (EquipmentTypeItem)cbType.SelectedItem;

        using var connexion = Database.Open();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
        UPDATE ""Equipements""
        SET type_id = $typeId,
            nom = $name,
            code_parc = $codeParc,
            numero_serie = $serial,
            marque = $brand,
            commentaire = $comment
        WHERE id_equipement = $id;";

        command.Parameters.AddWithValue("$id", selected.Id);
        command.Parameters.AddWithValue("$typeId", selectedType.Id);
        command.Parameters.AddWithValue("$name", tbName.Text.Trim());
        command.Parameters.AddWithValue("$codeParc", tbCodeParc.Text.Trim());
        command.Parameters.AddWithValue("$serial", ToDbNullable(tbSerialNumber.Text));
        command.Parameters.AddWithValue("$brand", ToDbNullable(tbBrand.Text));
        command.Parameters.AddWithValue("$comment", ToDbNullable(tbComment.Text));

        try
        {
            var rows = command.ExecuteNonQuery();
            if (rows == 0)
            {
                MessageBox.Show("Aucune modification effectuée (équipement introuvable ?).");
                return;
            }

            MessageBox.Show("Modifications enregistrées.");

            selected.Name = tbName.Text.Trim();
            selected.Code = tbCodeParc.Text.Trim();
            selected.Type = selectedType.Name;

            lbEquipment.DisplayMember = null;
            lbEquipment.DisplayMember = nameof(EquipmentListItem.Label);

        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Erreur SQL : " + ex.Message);
        }
    }


    /// <summary>
    /// Gestionnaire du clic sur le bouton recherche : lance le filtrage de la liste.
    /// </summary>
    private void btnSearch_Click(object sender, EventArgs e)
    {
        var q = (tbSearch?.Text ?? "").Trim();
        LoadEquipmentListFiltered(q);
    }

    /// <summary>
    /// Charge la liste des équipements en appliquant un filtre facultatif sur le nom, code, numéro de série et type.
    /// </summary>
    /// <param name="query">Texte de recherche (peut être null ou vide).</param>
    private void LoadEquipmentListFiltered(string query)
    {
        try
        {
            using var connexion = Database.Open();
            using var command = connexion.CreateCommand();

            if (string.IsNullOrWhiteSpace(query))
            {
                command.CommandText = @"
                    SELECT e.id_equipement,
                        COALESCE(TRIM(e.nom), '(sans nom)'),
                        TRIM(COALESCE(e.code_parc, '')),
                        t.name
                    FROM Equipements e
                    JOIN equipment_type t ON t.id = e.type_id
                    ORDER BY 2 COLLATE NOCASE, 4 COLLATE NOCASE, 3 COLLATE NOCASE;";
            }
            else
            {
                command.CommandText = @"
                    SELECT e.id_equipement,
                        COALESCE(TRIM(e.nom), '(sans nom)'),
                        TRIM(COALESCE(e.code_parc, '')),
                        t.name
                    FROM Equipements e
                    JOIN equipment_type t ON t.id = e.type_id
                    WHERE e.nom LIKE $p OR e.code_parc LIKE $p OR e.numero_serie LIKE $p OR t.name LIKE $p
                    ORDER BY 2 COLLATE NOCASE, 4 COLLATE NOCASE, 3 COLLATE NOCASE;";
                command.Parameters.AddWithValue("$p", $"%{query}%");
            }

            using var reader = command.ExecuteReader();
            var items = new List<EquipmentListItem>();
            while (reader.Read())
            {
                items.Add(new EquipmentListItem
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Code = reader.GetString(2),
                    Type = reader.GetString(3)
                });
            }

            lbEquipment.DataSource = items;
            lbEquipment.DisplayMember = nameof(EquipmentListItem.Label);
            lbEquipment.ValueMember = nameof(EquipmentListItem.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche d'équipements : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Supprime l'équipement sélectionné après confirmation utilisateur.
    /// Met à jour l'affichage et vide les champs du formulaire.
    /// </summary>
    private void DeleteSelectedEquipment()
    {
        if (lbEquipment.SelectedItem is not EquipmentListItem item)
        { MessageBox.Show("Sélectionne un équipement à supprimer."); return; }

        var confirm = MessageBox.Show(
            $"Supprimer « {item.Label} » ?",
            "Confirmer la suppression",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            using var connexion = Database.Open();
            using var cmd = connexion.CreateCommand();
            cmd.CommandText = @"DELETE FROM ""Equipements"" WHERE id_equipement = $id;";
            cmd.Parameters.AddWithValue("$id", item.Id);
            var rows = cmd.ExecuteNonQuery();

            if (rows == 0)
            {
                MessageBox.Show("Équipement introuvable.");
                return;
            }

            LoadEquipmentListFiltered(tbSearch?.Text?.Trim() ?? "");
            tbName.Clear(); tbCodeParc.Clear(); tbSerialNumber.Clear(); tbBrand.Clear(); tbComment.Clear();
            MessageBox.Show("Équipement supprimé.");
        }
        catch (SqliteException ex)
        {
            MessageBox.Show("Erreur SQL : " + ex.Message);
        }
    }

}