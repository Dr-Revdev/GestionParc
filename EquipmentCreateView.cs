using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ProjetParc.Views;

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

    public EquipmentCreateView(Action onBack)
    {
        _onBack = onBack;
        BuildUi();
        LoadEquipmentTypes();
    }

    private void BuildUi()
    {

        Dock = DockStyle.Fill;

        var btnBack = new Button { Text = "← Retour", Left = 24, Top = 24, Width = 120, Height = 36 };
        btnBack.Click += (_, __) => _onBack?.Invoke();
        Controls.Add(btnBack);

        Font = new Font("Segoe UI", 11f, FontStyle.Regular);


        // Button
        cbType = new ComboBox { Left = 160, Top = 160, Width = 420, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList };
        tbName = new TextBox { Left = 720, Top = 160, Width = 420, Height = 36 };
        tbCodeParc = new TextBox { Left = 1280, Top = 160, Width = 420, Height = 36 };

        tbSerial = new TextBox { Left = 160, Top = 240, Width = 420, Height = 36 };
        tbBrand = new TextBox { Left = 720, Top = 240, Width = 420, Height = 36 };

        tbComment = new TextBox { Left = 160, Top = 320, Width = 420, Height = 160, Multiline = true };

        btnCreate = new Button { Text = "Créer", Left = 1630, Top = 600, Width = 180, Height = 52, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };

        Controls.AddRange([cbType, tbName, tbCodeParc, tbSerial, tbBrand, tbComment, btnCreate]);

        // Labels
        var lblType = new Label { Text = "Type", Left = 160, Top = 132, Width = 420, Height = 20 };
        var lblName = new Label { Text = "Nom", Left = 720, Top = 132, Width = 420, Height = 20 };
        var lblCodeParc = new Label { Text = "Code parc", Left = 1280, Top = 132, Width = 420, Height = 20 };

        var lblSerial = new Label { Text = "Numéro de série", Left = 160, Top = 212, Width = 420, Height = 20 };
        var lblBrand = new Label { Text = "Marque", Left = 720, Top = 212, Width = 420, Height = 20 };

        var lblComment = new Label { Text = "Commentaire", Left = 160, Top = 296, Width = 420, Height = 20 };

        Controls.AddRange([lblType, lblName, lblCodeParc, lblSerial, lblBrand, lblComment]);

        cbType.TabIndex = 0; tbName.TabIndex = 1; tbCodeParc.TabIndex = 2; tbSerial.TabIndex = 3; tbBrand.TabIndex = 4; tbComment.TabIndex = 5; btnCreate.TabIndex = 6;

        btnCreate.Click += btnCreate_Click;
    }

    private sealed class EquipmentTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    private void LoadEquipmentTypes()
    {
        using var connection = Database.Open();
        using var command = connection.CreateCommand();
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
    private static object ToDbNullable(string s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();

    private static string GenerateEquipmentId() => Guid.NewGuid().ToString("N");

    private void InsertEquipment()
    {
        if (!ValidateEquipmentForm(out var errorMessage))
        {
            MessageBox.Show(errorMessage);
            return;
        }

        var SelectedType = (EquipmentTypeItem)cbType.SelectedItem;
        var newId = GenerateEquipmentId();

        using var connection = Database.Open();
        using var command = connection.CreateCommand();
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

    private void btnCreate_Click(object sender, EventArgs e)
    {
        InsertEquipment();
    }
}