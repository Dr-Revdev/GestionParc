using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestiParc.Ui.Data;
using GestiParc.Ui.Services;
using GestiParc.Ui.Views.Loan.Models;
using GestiParc.Infrastructure.Data.Repositories;

namespace GestiParc.Ui.Views.Equipment;

/// <summary>
/// Fenêtre pour échanger des équipements entre 2 agents - chaque agent a sa liste cochable d'équipements
/// </summary>
public class EquipmentExchangeView : Form
{
    private ComboBox cbAgent1 = null!;
    private ComboBox cbAgent2 = null!;
    private CheckedListBox clbAgent1Equipment = null!;
    private CheckedListBox clbAgent2Equipment = null!;
    private Label lblAgent1Count = null!;
    private Label lblAgent2Count = null!;
    private Button btnCancel = null!;
    private Button btnExchange = null!;

    public EquipmentExchangeView()
    {
        BuildUi();
        LoadAgents();
    }

    private void BuildUi()
    {
        Text = "Échange d'équipements entre agents";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;
        Font = Theme.Fonts.Body;
        BackColor = Theme.Colors.Background;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(20),
            BackColor = Theme.Colors.Background
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Titre
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Bouton Échanger (centre)
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Bouton Valider

        // Titre
        var lblTitle = new Label
        {
            Text = "Échange d'équipements entre agents",
            Font = Theme.Fonts.H3,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        mainLayout.Controls.Add(lblTitle, 0, 0);

        // Contenu principal : 2 colonnes pour les 2 agents
        var contentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };

        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47.5f)); // Agent 1
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5f));    // Séparateur
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47.5f)); // Agent 2

        // === AGENT 1 ===
        var agent1Panel = CreateAgentPanel("Agent 1", out cbAgent1, out clbAgent1Equipment, out lblAgent1Count);
        cbAgent1.SelectedIndexChanged += (s, e) => LoadAgent1Equipment();
        clbAgent1Equipment.ItemCheck += (s, e) => 
        {
            BeginInvoke(new Action(() => UpdateSelectionCount(clbAgent1Equipment, lblAgent1Count)));
        };

        // === SÉPARATEUR ===
        var separatorPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Colors.Border,
            Padding = new Padding(20, 0, 20, 0)
        };
        var lblSeparator = new Label
        {
            Text = "⇄",
            Font = Theme.Fonts.H1,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Colors.TextSecondary
        };
        separatorPanel.Controls.Add(lblSeparator);

        // === AGENT 2 ===
        var agent2Panel = CreateAgentPanel("Agent 2", out cbAgent2, out clbAgent2Equipment, out lblAgent2Count);
        cbAgent2.SelectedIndexChanged += (s, e) => LoadAgent2Equipment();
        clbAgent2Equipment.ItemCheck += (s, e) => 
        {
            BeginInvoke(new Action(() => UpdateSelectionCount(clbAgent2Equipment, lblAgent2Count)));
        };

        contentPanel.Controls.Add(agent1Panel, 0, 0);
        contentPanel.Controls.Add(separatorPanel, 1, 0);
        contentPanel.Controls.Add(agent2Panel, 2, 0);

        mainLayout.Controls.Add(contentPanel, 0, 1);

        // === BOUTON ÉCHANGER (CENTRE) ===
        var exchangeButtonPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Colors.Background
        };

        btnExchange = new Button
        {
            Text = "Échanger",
            Width = 150
        };
        Theme.StylePrimaryButton(btnExchange);
        btnExchange.Click += btnExchange_Click;

        // Centrer le bouton
        exchangeButtonPanel.Resize += (s, e) =>
        {
            btnExchange.Location = new Point(
                (exchangeButtonPanel.Width - btnExchange.Width) / 2,
                (exchangeButtonPanel.Height - btnExchange.Height) / 2
            );
        };
        exchangeButtonPanel.Controls.Add(btnExchange);

        mainLayout.Controls.Add(exchangeButtonPanel, 0, 2);

        // === BOUTON VALIDER (BAS DROITE) ===
        var validateButtonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Colors.Background
        };

        btnCancel = new Button
        {
            Text = "Valider",
            Width = 120,
            Margin = new Padding(5)
        };
        Theme.StyleSuccessButton(btnCancel);
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

        validateButtonPanel.Controls.Add(btnCancel);

        mainLayout.Controls.Add(validateButtonPanel, 0, 3);

        Controls.Add(mainLayout);
    }

    /// <summary>
    /// Helper qui crée un panel avec combobox (choix agent) + liste à cocher (ses équipements)
    /// </summary>
    private Panel CreateAgentPanel(string title, out ComboBox comboBox, out CheckedListBox checkedListBox, out Label countLabel)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(10),
            BackColor = Theme.Colors.Surface
        };

        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Titre
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // ComboBox
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Liste
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Compteur

        // Titre
        var lblTitle = new Label
        {
            Text = title,
            Font = Theme.Fonts.H5,
            ForeColor = Theme.Colors.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        // ComboBox
        comboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Theme.StyleComboBox(comboBox);

        // CheckedListBox
        checkedListBox = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            Font = Theme.Fonts.BodySmall,
            BackColor = Theme.Colors.Surface,
            CheckOnClick = true
        };

        // Compteur
        countLabel = new Label
        {
            Text = "Sélectionnés : 0",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = Theme.Fonts.Caption,
            ForeColor = Theme.Colors.TextSecondary
        };

        panel.Controls.Add(lblTitle, 0, 0);
        panel.Controls.Add(comboBox, 0, 1);
        panel.Controls.Add(checkedListBox, 0, 2);
        panel.Controls.Add(countLabel, 0, 3);

        return panel;
    }

    /// <summary>
    /// Remplit les 2 combobox avec la liste de tous les agents
    /// </summary>
    private void LoadAgents()
    {
        try
        {
            var agentRepo = new AgentMySqlRepository();
            var equipeRepo = new EquipeMySqlRepository();

            var agents = agentRepo.GetAll().OrderBy(a => a.Nom).ThenBy(a => a.Prenom).ToList();
            var equipes = equipeRepo.GetAll();
            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);

            var agentItems = agents.Select(a =>
            {
                var equipe = a.EquipeId.HasValue && equipeDict.ContainsKey(a.EquipeId.Value)
                    ? $" ({equipeDict[a.EquipeId.Value]})"
                    : "";
                return new AgentItem
                {
                    Idrh = a.Idrh,
                    DisplayName = $"{a.Nom} {a.Prenom}{equipe}"
                };
            }).ToList();

            cbAgent1.Items.Clear();
            cbAgent2.Items.Clear();

            foreach (var agent in agentItems)
            {
                cbAgent1.Items.Add(agent);
                cbAgent2.Items.Add(agent);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des agents : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Charge les équipements de l'agent 1</summary>
    private void LoadAgent1Equipment()
    {
        LoadAgentEquipment(cbAgent1, clbAgent1Equipment, lblAgent1Count);
    }

    /// <summary>Charge les équipements de l'agent 2</summary>
    private void LoadAgent2Equipment()
    {
        LoadAgentEquipment(cbAgent2, clbAgent2Equipment, lblAgent2Count);
    }

    /// <summary>
    /// Charge les équipements prêtés d'un agent dans sa CheckedListBox
    /// </summary>
    private void LoadAgentEquipment(ComboBox comboBox, CheckedListBox listBox, Label countLabel)
    {
        listBox.Items.Clear();
        countLabel.Text = "Sélectionnés : 0";

        if (comboBox.SelectedItem is not AgentItem agent)
        {
            return;
        }

        try
        {
            var equipmentRepo = new EquipmentMySqlRepository();
            var typeRepo = new EquipmentTypeMySqlRepository();

            var equipments = equipmentRepo.GetByAgent(agent.Idrh).Where(e => e.EtatPret == 1).ToList();
            var types = typeRepo.GetAll();
            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);

            var sortedEquipments = equipments
                .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
                .ThenBy(e => e.Nom ?? "");

            foreach (var eq in sortedEquipments)
            {
                var typeName = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "Inconnu";
                var equipmentItem = new EquipmentItem
                {
                    Id = eq.IdEquipement,
                    DisplayName = $"{typeName} - {eq.Nom ?? ""} ({eq.CodeParc ?? ""})"
                };
                listBox.Items.Add(equipmentItem);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des équipements : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Met à jour le label "Sélectionnés : X"
    /// </summary>
    private void UpdateSelectionCount(CheckedListBox listBox, Label countLabel)
    {
        int count = listBox.CheckedItems.Count;
        countLabel.Text = $"Sélectionnés : {count}";
    }

    /// <summary>
    /// Handler du bouton Échanger - fait la permutation des équipements cochés entre les 2 agents
    /// </summary>
    private void btnExchange_Click(object? sender, EventArgs e)
    {
        // Validation
        if (cbAgent1.SelectedItem is not AgentItem agent1)
        {
            MessageBox.Show("Veuillez sélectionner l'agent 1.", "Attention", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cbAgent2.SelectedItem is not AgentItem agent2)
        {
            MessageBox.Show("Veuillez sélectionner l'agent 2.", "Attention", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (agent1.Idrh == agent2.Idrh)
        {
            MessageBox.Show("Les deux agents doivent être différents.", "Attention", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var agent1Equipments = clbAgent1Equipment.CheckedItems.Cast<EquipmentItem>().ToList();
        var agent2Equipments = clbAgent2Equipment.CheckedItems.Cast<EquipmentItem>().ToList();

        if (agent1Equipments.Count == 0 && agent2Equipments.Count == 0)
        {
            MessageBox.Show("Veuillez sélectionner au moins un équipement à échanger.", "Attention", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Confirmation
        string message = "Voulez-vous vraiment effectuer cet échange ?\n\n";
        
        if (agent1Equipments.Count > 0)
        {
            message += $"De {agent1.DisplayName} vers {agent2.DisplayName} :\n";
            foreach (var eq in agent1Equipments)
            {
                message += $"  • {eq.DisplayName}\n";
            }
            message += "\n";
        }

        if (agent2Equipments.Count > 0)
        {
            message += $"De {agent2.DisplayName} vers {agent1.DisplayName} :\n";
            foreach (var eq in agent2Equipments)
            {
                message += $"  • {eq.DisplayName}\n";
            }
        }

        var result = MessageBox.Show(message, "Confirmation", 
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return;
        }

        // Exécution de l'échange
        try
        {
            var equipmentRepo = new EquipmentMySqlRepository();

            // Transférer les équipements de Agent1 vers Agent2
            foreach (var equipment in agent1Equipments)
            {
                var eq = equipmentRepo.GetById(equipment.Id);
                if (eq != null)
                {
                    var updatedEq = eq with { Idrh = agent2.Idrh };
                    equipmentRepo.Update(updatedEq);
                }
            }

            // Transférer les équipements de Agent2 vers Agent1
            foreach (var equipment in agent2Equipments)
            {
                var eq = equipmentRepo.GetById(equipment.Id);
                if (eq != null)
                {
                    var updatedEq = eq with { Idrh = agent1.Idrh };
                    equipmentRepo.Update(updatedEq);
                }
            }

            MessageBox.Show("Échange effectué avec succès !", "Succès", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Rafraîchir les listes sans fermer la fenêtre
            LoadAgent1Equipment();
            LoadAgent2Equipment();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'échange : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
