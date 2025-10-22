using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;
using ProjetParc.Views.Loan.Models;

namespace ProjetParc.Views.Loan;

/// <summary>
/// Contrôle permettant de sélectionner un équipement avec un bouton de suppression
/// </summary>
public class EquipmentSelectionControl : Panel
{
    private ComboBox cmbEquipment;
    private Button btnRemove;
    public event EventHandler OnRemove;
    private readonly string preselectId;

    public EquipmentItem SelectedEquipment => cmbEquipment.SelectedItem as EquipmentItem;

    public EquipmentSelectionControl() : this(null) { }

    public EquipmentSelectionControl(string preselectedEquipmentId)
    {
        preselectId = preselectedEquipmentId;
        InitializeComponent();
        LoadEquipments();
    }

    private void InitializeComponent()
    {
        Size = new Size(520, 40);
        Margin = new Padding(0, 0, 0, 10);

        cmbEquipment = new ComboBox
        {
            Left = 0,
            Top = 5,
            Width = 475,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(cmbEquipment);

        btnRemove = new Button
        {
            Text = "×",
            Left = 485,
            Top = 5,
            Width = 30,
            Height = 30,
            ForeColor = Color.Red
        };
        btnRemove.Click += (s, e) => OnRemove?.Invoke(this, e);
        Controls.Add(btnRemove);
    }

    private void LoadEquipments()
    {
        try
        {
            using var connection = Database.Open();
            using var command = connection.CreateCommand();
            // First, load available equipments (etat_pret = 0)
            command.CommandText = @"
                SELECT e.id_equipement, e.nom, e.code_parc, t.name as type
                FROM Equipements e
                JOIN equipment_type t ON t.id = e.type_id
                WHERE e.etat_pret = 0
                ORDER BY t.name, e.nom";

            using var reader = command.ExecuteReader();
            var foundPreselect = false;
            while (reader.Read())
            {
                var item = new EquipmentItem
                {
                    Id = reader.GetString(0),
                    DisplayName = $"{reader.GetString(3)} - {reader.GetString(1)} ({reader.GetString(2)})"
                };
                if (preselectId != null && item.Id == preselectId) foundPreselect = true;
                cmbEquipment.Items.Add(item);
            }

            // If we have a preselect id that wasn't in available list, load it explicitly (it may be currently loaned to this agent)
            if (preselectId != null && !foundPreselect)
            {
                using var cmd2 = connection.CreateCommand();
                cmd2.CommandText = @"
                    SELECT e.id_equipement, e.nom, e.code_parc, t.name as type
                    FROM Equipements e
                    JOIN equipment_type t ON t.id = e.type_id
                    WHERE e.id_equipement = $id";
                cmd2.Parameters.AddWithValue("$id", preselectId);
                using var r2 = cmd2.ExecuteReader();
                if (r2.Read())
                {
                    var item = new EquipmentItem
                    {
                        Id = r2.GetString(0),
                        DisplayName = $"{r2.GetString(3)} - {r2.GetString(1)} ({r2.GetString(2)})"
                    };
                    cmbEquipment.Items.Add(item);
                    // select it after adding
                    cmbEquipment.SelectedIndex = cmbEquipment.Items.Count - 1;
                }
            }

            // If preselectId was available in the initial list, select it
            if (preselectId != null && !cmbEquipment.Items.IsReadOnly)
            {
                for (int i = 0; i < cmbEquipment.Items.Count; i++)
                {
                    if (cmbEquipment.Items[i] is EquipmentItem ei && ei.Id == preselectId)
                    {
                        cmbEquipment.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements : {ex.Message}",
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
