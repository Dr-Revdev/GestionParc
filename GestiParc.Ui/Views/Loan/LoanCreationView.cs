using GestiParc.Ui.Services;
using GestiParc.Ui.Views.Loan.Models;
using GestiParc.Ui.Services.Api;
using System.Net.Http;

namespace GestiParc.Ui.Views.Loan;

/// <summary>
/// Fenêtre pour créer/modifier un prêt - choix de l'agent et ajout/suppression d'équipements
/// Peut aussi générer une feuille de remise en mode édition
/// </summary>
public class LoanCreationView : Form
{
    private readonly EquipmentApiClient _equipmentApiClient = new EquipmentApiClient();
    private readonly AgentApiClient _agentApiClient = new AgentApiClient();
    private ComboBox cmbAgent = null!;
    private Label lblAgentDisplay = null!; // Pour afficher le nom en mode édition
    private FlowLayoutPanel pnlEquipments = null!;
    private Button btnAddEquipment = null!;
    private Button btnValidate = null!;
    private Button btnCancel = null!;
    private Button btnFeuilleRemise = null!;

    private string selectedAgentId = string.Empty;
    private bool isEditMode = false;
    

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
                    BeginInvoke(new Action(async () => await LoadAssignedEquipmentsAsync(selectedAgentId)));
                }
                UpdateUIForEditMode();
            }
        }
    }

    public LoanCreationView()
    {
        BuildUi();
        Load += async (sender, e) => await LoadAgentsAsync();
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
            
            // Activer le bouton feuille de remise en mode édition
            if (btnFeuilleRemise != null)
            {
                btnFeuilleRemise.Enabled = true;
            }
        }
        else
        {
            Text = "Nouveau prêt";
            cmbAgent.Visible = true;
            lblAgentDisplay.Visible = false;
            
            // Désactiver le bouton feuille de remise en mode création
            if (btnFeuilleRemise != null)
            {
                btnFeuilleRemise.Enabled = false;
            }
        }
    }

    private void SelectAgentById(string id)
    {
        for (int i = 0; i < cmbAgent.Items.Count; i++)
        {
            if (cmbAgent.Items[i] is AgentItem ai && ai.Idrh == id)
            {
                cmbAgent.SelectedIndex = i;
                return;
            }
        }
    }

    private async Task LoadAssignedEquipmentsAsync(string agentId)
    {
        try
        {
            // Vider les contrôles d'équipements existants
            pnlEquipments.Controls.Clear();
            
            var allEquipments = await _equipmentApiClient.GetAllAsync();
            var equipments = allEquipments.Where(e => e.Idrh == agentId);
            
            // Filtrer uniquement ceux en état prêt (etat_pret = 1)
            var loanedEquipments = equipments.Where(e => e.EtatPret == 1);
            
            foreach (var equipment in loanedEquipments)
            {
                AddEquipmentControl(equipment.IdEquipement);
            }
            
            // S'assurer qu'au moins un contrôle existe
            if (pnlEquipments.Controls.Count == 0) AddEquipmentControl();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les équipements.\n\n{ex.Message}", 
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements assignés : {ex.Message}", 
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BuildUi()
    {
        Text = "Nouveau prêt";
        MinimumSize = new Size(600, 500);
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        AutoScroll = true;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;

        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(20),
            BackColor = Theme.Colors.Background,
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
            BackColor = Theme.Colors.Surface,
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
            TextAlign = ContentAlignment.BottomLeft,
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary
        };
        agentPanel.Controls.Add(lblAgent, 0, 0);

        cmbAgent = new ComboBox
        {
            Dock = DockStyle.Fill,
            Height = 30,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Theme.StyleComboBox(cmbAgent);
        agentPanel.Controls.Add(cmbAgent, 0, 1);
        
        // Label pour affichage en mode édition (invisible par défaut)
        lblAgentDisplay = new Label
        {
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Body,
            BackColor = Theme.Colors.Surface,
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
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.Label,
            ForeColor = Theme.Colors.TextSecondary
        };
        mainLayout.Controls.Add(lblEquipments, 0, 1);

        // Equipment panel
        pnlEquipments = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Theme.Colors.Surface
        };
        mainLayout.Controls.Add(pnlEquipments, 0, 2);

        // Buttons panel
        TableLayoutPanel buttonPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = Theme.Colors.Background,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 25),  // Add equipment
                new ColumnStyle(SizeType.Percent, 25),  // Delete
                new ColumnStyle(SizeType.Percent, 25),  // Feuille remise
                new ColumnStyle(SizeType.Percent, 12.5f),  // Validate
                new ColumnStyle(SizeType.Percent, 12.5f)   // Cancel
            }
        };
        mainLayout.Controls.Add(buttonPanel, 0, 3);

        btnAddEquipment = new Button
        {
            Text = "Ajouter un équipement",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0)
        };
        Theme.StyleSecondaryButton(btnAddEquipment);
        btnAddEquipment.Click += (_, _) => AddEquipmentControl();
        buttonPanel.Controls.Add(btnAddEquipment, 0, 0);

        // Buttons
        var btnDelete = new Button
        {
            Text = "Supprimer le prêt",
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 0, 10, 0)
        };
        Theme.StyleDangerButton(btnDelete);
        btnDelete.Click += btnDelete_Click;
        buttonPanel.Controls.Add(btnDelete, 1, 0);

        btnFeuilleRemise = new Button
        {
            Text = "Feuille de remise",
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 0, 10, 0),
            Enabled = isEditMode  // Seulement activé en mode édition
        };
        Theme.StylePrimaryButton(btnFeuilleRemise);
        btnFeuilleRemise.Click += (_, _) => GenerateFeuilleRemise();
        buttonPanel.Controls.Add(btnFeuilleRemise, 2, 0);

        btnValidate = new Button
        {
            Text = "Valider",
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 0, 10, 0)
        };
        Theme.StyleSuccessButton(btnValidate);
        btnValidate.Click += btnValidate_Click;
        buttonPanel.Controls.Add(btnValidate, 3, 0);

        btnCancel = new Button
        {
            Text = "Annuler",
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 0, 0, 0)
        };
        Theme.StyleSecondaryButton(btnCancel);
        btnCancel.Click += (_, _) => Close();
        buttonPanel.Controls.Add(btnCancel, 4, 0);

        // Add first equipment control by default
        AddEquipmentControl();
    }

    private async Task LoadAgentsAsync()
    {
        try
        {
            var agents = await _agentApiClient.GetAllAsync();
            
            // Trier par nom, prénom
            var sortedAgents = agents
                .OrderBy(a => a.Nom ?? "")
                .ThenBy(a => a.Prenom ?? "")
                .ThenBy(a => a.Idrh);
            
            foreach (var agent in sortedAgents)
            {
                var agentItem = new AgentItem
                {
                    Idrh = agent.Idrh,
                    DisplayName = $"{agent.Nom} {agent.Prenom}"
                };
                cmbAgent.Items.Add(agentItem);
            }
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de charger les agents.\n\n : {ex.Message}",
            "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des agents : {ex.Message}",
            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddEquipmentControl(string? preselectEquipmentId = null)
    {
        var control = new EquipmentSelectionControl(preselectEquipmentId);
        control.OnRemove += (sender, _) =>
        {
            if (pnlEquipments.Controls.Count > 1 && sender != null) // Keep at least one
            {
                pnlEquipments.Controls.Remove((Control)sender);
            }
        };
        pnlEquipments.Controls.Add(control);
    }

    private async Task DeleteLoanAsync()
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

        var allEquipments = await _equipmentApiClient.GetAllAsync();
        var equipments = allEquipments.Where(e => e.Idrh == SelectedAgentId);
        
        // Récupérer tous les équipements prêtés à cet agent
        
        var loanedEquipments = equipments.Where(e => e.EtatPret == 1);
        
        // Mettre à jour chaque équipement pour le rendre disponible
        foreach (var equipment in loanedEquipments)
        {
            var updatedEquipment = equipment with 
            { 
                Idrh = string.Empty, 
                EtatPret = 0 
            };
            await _equipmentApiClient.UpdateAsync(equipment.IdEquipement, updatedEquipment);
        }
        
        DialogResult = DialogResult.OK;
        Close();

    }

    private async void btnDelete_Click(object? sender, EventArgs e)
    {
        try
        {
            await DeleteLoanAsync();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de joindre le serveur : {ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression du prêt : {ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ValidateLoanAsync()
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

        // En mode édition, on utilise selectedAgentId, sinon on prend l'agent sélectionné
        string agentId;
        if (isEditMode)
        {
            agentId = selectedAgentId;
        }
        else
        {
            var agent = (AgentItem?)cmbAgent.SelectedItem;
            agentId = agent?.Idrh ?? string.Empty;
        }

        // En mode édition, récupérer les équipements précédemment assignés
        var previouslyAssigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(selectedAgentId))
        {
            var allEquipments = await _equipmentApiClient.GetAllAsync();
            var prevEquipments = allEquipments.Where(e => e.Idrh == selectedAgentId);
            foreach (var eq in prevEquipments)
            {
                previouslyAssigned.Add(eq.IdEquipement);
            }
        }

        var newlySelected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var control in selectedEquipments)
        {
            var id = control.SelectedEquipment!.Id;
            newlySelected.Add(id);

            // Récupérer l'équipement existant
            var equipment = await _equipmentApiClient.GetByIdAsync(id);
            if (equipment == null) continue;
            
            // Mise à jour : garde l'état DSEM (2) si c'était déjà DSEM, sinon met en état prêt (1)
            var newEtatPret = equipment.EtatPret == 2 ? 2 : 1;
            
            var updatedEquipment = equipment with 
            { 
                Idrh = agentId, 
                EtatPret = newEtatPret 
            };
            await _equipmentApiClient.UpdateAsync(id, updatedEquipment);
        }

        // Pour tout équipement précédemment assigné qui n'est plus sélectionné, retirer l'assignation
        foreach (var prevId in previouslyAssigned)
        {
            if (!newlySelected.Contains(prevId))
            {
                var equipment = await _equipmentApiClient.GetByIdAsync(prevId);
                if (equipment == null) continue;
                
                var updatedEquipment = equipment with 
                { 
                    Idrh = string.Empty, 
                    EtatPret = 0 
                };
                await _equipmentApiClient.UpdateAsync(prevId, updatedEquipment);
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private async void btnValidate_Click(object? sender, EventArgs e)
    {
        try
        {
            await ValidateLoanAsync();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Impossible de joindre le serveur : {ex.Message}",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'enregistrement du prêt : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Génère le PDF de feuille de remise (seulement disponible en mode édition)
    /// </summary>
    private async void GenerateFeuilleRemise()
    {
        if (!isEditMode || string.IsNullOrEmpty(selectedAgentId))
        {
            MessageBox.Show("La génération de feuille de remise n'est disponible qu'en mode édition.", 
                          "Non disponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await DocumentGeneratorUiService.GenerateFeuilleRemiseAsync(selectedAgentId);
    }
}
