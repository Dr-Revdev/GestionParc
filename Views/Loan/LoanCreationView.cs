using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using ProjetParc.Data;
using ProjetParc.Views.Loan.Models;

namespace ProjetParc.Views.Loan;

/// <summary>
/// Fenêtre de création d'un nouveau prêt d'équipement
/// </summary>
public class LoanCreationView : Form
{
    private ComboBox cmbAgent;
    private Label lblAgentDisplay; // Pour afficher le nom en mode édition
    private FlowLayoutPanel pnlEquipments;
    private Button btnAddEquipment;
    private Button btnValidate;
    private Button btnCancel;

    private string selectedAgentId = string.Empty;
    private bool isEditMode = false;
    
    // Optional pre-selected agent id when editing
    [System.ComponentModel.Browsable(true)]
    [System.ComponentModel.Category("Data")]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
    public string SelectedAgentId 
    { 
        get => selectedAgentId;
        set 
        {
            selectedAgentId = value ?? string.Empty;
            isEditMode = !string.IsNullOrEmpty(selectedAgentId);
            
            if (IsHandleCreated && !string.IsNullOrEmpty(selectedAgentId))
            {
                SelectAgentById(selectedAgentId);
                if (cmbAgent.SelectedItem != null)
                {
                    LoadAssignedEquipments(selectedAgentId);
                }
                UpdateUIForEditMode();
            }
        }
    }

    public LoanCreationView()
    {
        InitializeComponent();
        LoadAgents();
    }

    protected override void OnShown(System.EventArgs e)
    {
        base.OnShown(e);
        // Force la mise à jour de l'interface si un agent est sélectionné
        if (!string.IsNullOrEmpty(selectedAgentId))
        {
            SelectedAgentId = selectedAgentId;
        }
        UpdateUIForEditMode();
    }

    private void UpdateUIForEditMode()
    {
        if (isEditMode)
        {
            Text = "Édition de prêt";
            cmbAgent.Visible = false;
            lblAgentDisplay.Visible = true;
            
            if (cmbAgent.SelectedItem is AgentItem agent)
            {
                lblAgentDisplay.Text = agent.DisplayName;
            }
        }
        else
        {
            Text = "Nouveau prêt";
            cmbAgent.Visible = true;
            lblAgentDisplay.Visible = false;
        }
    }

    private void SelectAgentById(string id)
    {
        for (int i = 0; i < cmbAgent.Items.Count; i++)
        {
            if (cmbAgent.Items[i] is AgentItem ai && ai.Id == id)
            {
                cmbAgent.SelectedIndex = i;
                return;
            }
        }
    }

    private void LoadAssignedEquipments(string agentId)
    {
        try
        {
            // Clear existing equipment controls
            pnlEquipments.Controls.Clear();
            using var connection = Database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT e.id_equipement
                FROM Equipements e
                WHERE e.idrh = $idrh AND e.etat_pret = 1";
            command.Parameters.AddWithValue("$idrh", agentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(0);
                AddEquipmentControl(id);
            }
            // Ensure at least one control exists
            if (pnlEquipments.Controls.Count == 0) AddEquipmentControl();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements assignés : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeComponent()
    {
        Text = "Nouveau prêt";
        MinimumSize = new Size(600, 500);
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        AutoScroll = true;

        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(20),
            RowStyles = {
                new RowStyle(SizeType.Absolute, 80),  // Agent section
                new RowStyle(SizeType.Absolute, 40),  // Equipment label
                new RowStyle(SizeType.Percent, 100),  // Equipment list
                new RowStyle(SizeType.Absolute, 60)   // Buttons
            }
        };
        Controls.Add(mainLayout);

        // Agent section panel
        TableLayoutPanel agentPanel = new TableLayoutPanel 
        { 
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            RowStyles = {
                new RowStyle(SizeType.Absolute, 25),  // Label
                new RowStyle(SizeType.Absolute, 35)   // ComboBox/Label
            }
        };
        
        var lblAgent = new Label
        {
            Text = "Agent :",
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
        agentPanel.Controls.Add(lblAgent, 0, 0);

        cmbAgent = new ComboBox
        {
            Dock = DockStyle.Fill,
            Height = 30,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        agentPanel.Controls.Add(cmbAgent, 0, 1);
        
        // Label pour affichage en mode édition (invisible par défaut)
        lblAgentDisplay = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            BackColor = SystemColors.Control,
            BorderStyle = BorderStyle.Fixed3D,
            TextAlign = ContentAlignment.MiddleLeft,
            Visible = false
        };
        agentPanel.Controls.Add(lblAgentDisplay, 0, 1);
        
        mainLayout.Controls.Add(agentPanel, 0, 0);

        // Equipment label
        var lblEquipments = new Label
        {
            Text = "Équipements :",
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        mainLayout.Controls.Add(lblEquipments, 0, 1);

        // Equipment panel
        pnlEquipments = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BorderStyle = BorderStyle.FixedSingle
        };
        mainLayout.Controls.Add(pnlEquipments, 0, 2);

        // Buttons panel
        TableLayoutPanel buttonPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 30),  // Add equipment
                new ColumnStyle(SizeType.Percent, 30),  // Delete
                new ColumnStyle(SizeType.Percent, 20),  // Validate
                new ColumnStyle(SizeType.Percent, 20)   // Cancel
            }
        };
        mainLayout.Controls.Add(buttonPanel, 0, 3);

        btnAddEquipment = new Button
        {
            Text = "Ajouter un équipement",
            Dock = DockStyle.Fill,
            Height = 36,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnAddEquipment.Click += (_, _) => AddEquipmentControl();
        buttonPanel.Controls.Add(btnAddEquipment, 0, 0);

        // Buttons
        var btnDelete = new Button
        {
            Text = "Supprimer le prêt",
            Dock = DockStyle.Fill,
            Height = 36,
            ForeColor = Color.Red,
            Margin = new Padding(10, 0, 10, 0)
        };
        btnDelete.Click += (_, _) => DeleteLoan();
        buttonPanel.Controls.Add(btnDelete, 1, 0);

        btnValidate = new Button
        {
            Text = "Valider",
            Dock = DockStyle.Fill,
            Height = 36,
            Margin = new Padding(10, 0, 10, 0)
        };
        btnValidate.Click += (_, _) => ValidateLoan();
        buttonPanel.Controls.Add(btnValidate, 2, 0);

        btnCancel = new Button
        {
            Text = "Annuler",
            Dock = DockStyle.Fill,
            Height = 36,
            Margin = new Padding(10, 0, 0, 0)
        };
        btnCancel.Click += (_, _) => Close();
        buttonPanel.Controls.Add(btnCancel, 3, 0);

        // Add first equipment control by default
        AddEquipmentControl();
    }

    private void LoadAgents()
    {
        try
        {
            using var connection = Database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT idrh, nom, prenom 
                FROM Agents 
                ORDER BY nom, prenom";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var agent = new AgentItem
                {
                    Id = reader.GetString(0),
                    DisplayName = $"{reader.GetString(1)} {reader.GetString(2)}"
                };
                cmbAgent.Items.Add(agent);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des agents : {ex.Message}", "Erreur",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddEquipmentControl(string preselectEquipmentId = null)
    {
        var control = new EquipmentSelectionControl(preselectEquipmentId);
        control.OnRemove += (sender, _) =>
        {
            if (pnlEquipments.Controls.Count > 1) // Keep at least one
            {
                pnlEquipments.Controls.Remove((Control)sender);
            }
        };
        pnlEquipments.Controls.Add(control);
    }

    private void DeleteLoan()
    {
        if (string.IsNullOrEmpty(SelectedAgentId))
        {
            MessageBox.Show("Aucun prêt à supprimer.", "Information",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(
            "Voulez-vous vraiment supprimer ce prêt et rendre tous les équipements disponibles ?",
            "Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using var connection = Database.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE Equipements 
                SET idrh = NULL, etat_pret = 0 
                WHERE idrh = $idrh AND etat_pret = 1";
            command.Parameters.AddWithValue("$idrh", SelectedAgentId);
            command.ExecuteNonQuery();

            transaction.Commit();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression du prêt : {ex.Message}",
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ValidateLoan()
    {
        // En mode édition, on utilise l'agent déjà sélectionné
        if (!isEditMode && cmbAgent.SelectedItem == null)
        {
            MessageBox.Show("Veuillez sélectionner un agent.", "Validation",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedEquipments = pnlEquipments.Controls.OfType<EquipmentSelectionControl>()
            .Where(c => c.SelectedEquipment != null)
            .ToList();

        if (!selectedEquipments.Any())
        {
            MessageBox.Show("Veuillez sélectionner au moins un équipement.", "Validation",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // En mode édition, on utilise selectedAgentId, sinon on prend l'agent sélectionné
            string agentId;
            if (isEditMode)
            {
                agentId = selectedAgentId;
            }
            else
            {
                var agent = (AgentItem)cmbAgent.SelectedItem;
                agentId = agent.Id;
            }
            
            using var connection = Database.Open();
            using var transaction = connection.BeginTransaction();

            // If editing an existing agent assignment, get previously assigned equipment ids
            var previouslyAssigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(selectedAgentId))
            {
                using var prevCmd = connection.CreateCommand();
                prevCmd.CommandText = "SELECT id_equipement FROM Equipements WHERE idrh = $idrh";
                prevCmd.Parameters.AddWithValue("$idrh", selectedAgentId);
                using var prevR = prevCmd.ExecuteReader();
                while (prevR.Read()) previouslyAssigned.Add(prevR.GetString(0));
            }

            var newlySelected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var control in selectedEquipments)
            {
                var id = control.SelectedEquipment.Id;
                newlySelected.Add(id);

                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE Equipements 
                    SET idrh = $idrh, 
                        etat_pret = CASE 
                            WHEN etat_pret = 2 THEN 2  -- Garde l'état DSEM si c'était déjà DSEM
                            ELSE 1                      -- Sinon met en état prêt
                        END 
                    WHERE id_equipement = $id";
                command.Parameters.AddWithValue("$idrh", agentId);
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }

            // For any previously assigned equipment that is no longer selected, clear assignment
            foreach (var prevId in previouslyAssigned)
            {
                if (!newlySelected.Contains(prevId))
                {
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                        UPDATE Equipements
                        SET idrh = NULL, etat_pret = 0
                        WHERE id_equipement = $id";
                    cmd.Parameters.AddWithValue("$id", prevId);
                    cmd.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'enregistrement du prêt : {ex.Message}", "Erreur",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}