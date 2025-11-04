using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
            var equipmentRepo = new Data.Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Data.Repositories.MySQL.EquipmentTypeMySqlRepository();

            var equipments = equipmentRepo.GetAll();
            var types = typeRepo.GetAll();
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

            // Charger les équipements disponibles (etat_pret = 0)
            var availableEquipments = equipments
                .Where(e => e.EtatPret == 0)
                .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
                .ThenBy(e => e.Nom ?? "")
                .ToList();

            var foundPreselect = false;
            foreach (var eq in availableEquipments)
            {
                var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";
                var item = new EquipmentItem
                {
                    Id = eq.IdEquipement,
                    DisplayName = $"{typeName} - {eq.Nom ?? ""} ({eq.CodeParc ?? ""})"
                };
                if (preselectId != null && item.Id == preselectId) foundPreselect = true;
                cmbEquipment.Items.Add(item);
            }

            // Si on a un ID présélectionné qui n'est pas dans la liste des disponibles, le charger explicitement
            if (preselectId != null && !foundPreselect)
            {
                var preselectedEq = equipmentRepo.GetById(preselectId);
                if (preselectedEq != null)
                {
                    var typeName = typeDict.ContainsKey(preselectedEq.TypeId) ? typeDict[preselectedEq.TypeId] : "Inconnu";
                    var item = new EquipmentItem
                    {
                        Id = preselectedEq.IdEquipement,
                        DisplayName = $"{typeName} - {preselectedEq.Nom ?? ""} ({preselectedEq.CodeParc ?? ""})"
                    };
                    cmbEquipment.Items.Add(item);
                    // Le sélectionner après l'avoir ajouté
                    cmbEquipment.SelectedIndex = cmbEquipment.Items.Count - 1;
                }
            }

            // Si l'ID présélectionné était disponible dans la liste initiale, le sélectionner
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
