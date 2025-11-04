using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace ProjetParc.Data;

/// <summary>
/// Générateur de feuilles de remise pour les prêts d'équipement
/// </summary>
public class FeuilleRemiseGenerator
{
    private string _agentId;
    private PrintDocument _printDocument;
    private Font _titleFont;
    private Font _headerFont;
    private Font _bodyFont;
    private Font _smallFont;
    
    private const int MARGIN_LEFT = 50;
    private const int MARGIN_TOP = 50;
    private const int MARGIN_RIGHT = 50;
    private const int LINE_HEIGHT = 20;
    private const int SECTION_SPACING = 30;

    /// <summary>
    /// Génère une feuille de remise pour un agent donné
    /// </summary>
    /// <param name="agentId">ID de l'agent</param>
    public void GenerateFeuilleRemise(string agentId)
    {
        _agentId = agentId;
        
        try
        {
            // Initialiser les polices
            _titleFont = new Font("Arial", 16, FontStyle.Bold);
            _headerFont = new Font("Arial", 12, FontStyle.Bold);
            _bodyFont = new Font("Arial", 10, FontStyle.Regular);
            _smallFont = new Font("Arial", 8, FontStyle.Regular);

            // Configurer le document d'impression
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
            
            // Afficher l'aperçu avant impression
            var previewDialog = new PrintPreviewDialog
            {
                Document = _printDocument,
                WindowState = FormWindowState.Maximized,
                UseAntiAlias = true
            };
            
            if (previewDialog.ShowDialog() == DialogResult.OK)
            {
                // L'utilisateur peut choisir d'imprimer ou de sauvegarder en PDF
                var result = MessageBox.Show(
                    "Voulez-vous sauvegarder cette feuille de remise en PDF ?", 
                    "Sauvegarder", 
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    SaveToPdf();
                }
                else if (result == DialogResult.No)
                {
                    // Imprimer directement
                    var printDialog = new PrintDialog { Document = _printDocument };
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        _printDocument.Print();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la génération de la feuille de remise : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Nettoyer les ressources
            _titleFont?.Dispose();
            _headerFont?.Dispose();
            _bodyFont?.Dispose();
            _smallFont?.Dispose();
            _printDocument?.Dispose();
        }
    }

    /// <summary>
    /// Sauvegarde la feuille de remise en PDF
    /// </summary>
    private void SaveToPdf()
    {
        try
        {
            // Créer le dossier de sauvegarde si nécessaire
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var savePath = Path.Combine(documentsPath, "GestiParc", "FeuillesRemise");
            Directory.CreateDirectory(savePath);

            // Nom du fichier avec la date
            var fileName = $"FeuilleRemise_{_agentId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var fullPath = Path.Combine(savePath, fileName);

            // Pour une vraie implémentation PDF, il faudrait utiliser une librairie comme iTextSharp
            // Pour l'instant, on simule en sauvegardant comme image
            var saveDialog = new SaveFileDialog
            {
                Title = "Sauvegarder la feuille de remise",
                Filter = "Fichiers PDF (*.pdf)|*.pdf|Fichiers Image (*.png)|*.png",
                FileName = fileName,
                InitialDirectory = savePath
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Simulation - dans la vraie vie, on utiliserait iTextSharp ou similaire
                MessageBox.Show($"Feuille de remise sauvegardée :\n{saveDialog.FileName}", 
                              "Sauvegarde réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Ouvrir le dossier de destination
                System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(saveDialog.FileName));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Gestionnaire d'événement pour l'impression de la page
    /// </summary>
    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        var graphics = e.Graphics;
        var pageWidth = e.PageBounds.Width - MARGIN_LEFT - MARGIN_RIGHT;
        var currentY = MARGIN_TOP;

        try
        {
            // Charger les données depuis les repositories
            var agentRepo = new Repositories.MySQL.AgentMySqlRepository();
            var equipmentRepo = new Repositories.MySQL.EquipmentMySqlRepository();
            var typeRepo = new Repositories.MySQL.EquipmentTypeMySqlRepository();
            var siteRepo = new Repositories.MySQL.SiteMySqlRepository();
            var equipeRepo = new Repositories.MySQL.EquipeMySqlRepository();

            var agent = agentRepo.GetById(_agentId);
            var equipments = equipmentRepo.GetByAgent(_agentId).Where(e => e.EtatPret == 1).ToList();
            var types = typeRepo.GetAll();
            var sites = siteRepo.GetAll();
            var equipes = equipeRepo.GetAll();

            var typeDict = types.ToDictionary(t => t.Id, t => t.Name);
            var siteDict = sites.ToDictionary(s => s.Id, s => s.Name);
            var equipeDict = equipes.ToDictionary(e => e.Id, e => e.Name);

            // === EN-TÊTE ===
            currentY = DrawHeader(graphics, currentY, pageWidth);
            currentY += SECTION_SPACING;

            // === INFORMATIONS AGENT ===
            currentY = DrawAgentInfo(graphics, agent, siteDict, equipeDict, currentY, pageWidth);
            currentY += SECTION_SPACING;

            // === LISTE DES ÉQUIPEMENTS ===
            currentY = DrawEquipmentList(graphics, equipments, typeDict, currentY, pageWidth);
            currentY += SECTION_SPACING;

            // === SIGNATURES ===
            DrawSignatures(graphics, currentY, pageWidth, e.PageBounds.Height);
        }
        catch (Exception ex)
        {
            // En cas d'erreur, afficher un message sur la page
            graphics.DrawString($"Erreur : {ex.Message}", _bodyFont, Brushes.Red, 
                              MARGIN_LEFT, currentY);
        }
    }

    /// <summary>
    /// Dessine l'en-tête du document
    /// </summary>
    private int DrawHeader(Graphics graphics, int startY, int pageWidth)
    {
        var currentY = startY;
        
        // Titre principal
        var title = "FEUILLE DE REMISE D'ÉQUIPEMENT";
        var titleSize = graphics.MeasureString(title, _titleFont);
        var titleX = MARGIN_LEFT + (pageWidth - titleSize.Width) / 2;
        graphics.DrawString(title, _titleFont, Brushes.Black, titleX, currentY);
        currentY += (int)titleSize.Height + 10;

        // Date et heure de génération
        var dateText = $"Générée le : {DateTime.Now:dd/MM/yyyy à HH:mm}";
        var dateSize = graphics.MeasureString(dateText, _bodyFont);
        var dateX = MARGIN_LEFT + (pageWidth - dateSize.Width) / 2;
        graphics.DrawString(dateText, _bodyFont, Brushes.Gray, dateX, currentY);
        currentY += (int)dateSize.Height + 10;

        // Ligne de séparation
        graphics.DrawLine(Pens.Black, MARGIN_LEFT, currentY, MARGIN_LEFT + pageWidth, currentY);
        currentY += 10;

        return currentY;
    }

    /// <summary>
    /// Dessine les informations de l'agent
    /// </summary>
    private int DrawAgentInfo(Graphics graphics, DTOs.AgentDto agent, Dictionary<int, string> siteDict, 
                             Dictionary<int, string> equipeDict, int startY, int pageWidth)
    {
        var currentY = startY;

        // Titre de section
        graphics.DrawString("INFORMATIONS AGENT", _headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        currentY += (int)graphics.MeasureString("A", _headerFont).Height + 10;

        if (agent != null)
        {
            var nom = agent.Nom ?? "";
            var prenom = agent.Prenom ?? "";
            var idrh = agent.Idrh ?? "";
            var email = agent.Email ?? "";
            var site = agent.SiteId.HasValue && siteDict.ContainsKey(agent.SiteId.Value) 
                ? siteDict[agent.SiteId.Value] : "Non assigné";
            var equipe = agent.EquipeId.HasValue && equipeDict.ContainsKey(agent.EquipeId.Value)
                ? equipeDict[agent.EquipeId.Value] : "Non assignée";

            // Afficher les informations
            currentY = DrawLabelValue(graphics, "Nom :", $"{nom} {prenom}", currentY);
            currentY = DrawLabelValue(graphics, "IDRH :", idrh, currentY);
            currentY = DrawLabelValue(graphics, "Email :", email, currentY);
            currentY = DrawLabelValue(graphics, "Site :", site, currentY);
            currentY = DrawLabelValue(graphics, "Équipe :", equipe, currentY);
        }

        return currentY;
    }

    /// <summary>
    /// Dessine la liste des équipements en prêt
    /// </summary>
    private int DrawEquipmentList(Graphics graphics, List<DTOs.EquipmentDto> equipments, 
                                 Dictionary<int, string> typeDict, int startY, int pageWidth)
    {
        var currentY = startY;

        // Titre de section
        graphics.DrawString("ÉQUIPEMENTS EN PRÊT", _headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        currentY += (int)graphics.MeasureString("A", _headerFont).Height + 10;

        // En-têtes du tableau
        var colWidth = pageWidth / 4;
        graphics.DrawString("Type", _headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        graphics.DrawString("Nom/Code", _headerFont, Brushes.Black, MARGIN_LEFT + colWidth, currentY);
        graphics.DrawString("Série", _headerFont, Brushes.Black, MARGIN_LEFT + colWidth * 2, currentY);
        graphics.DrawString("Marque", _headerFont, Brushes.Black, MARGIN_LEFT + colWidth * 3, currentY);
        currentY += (int)graphics.MeasureString("A", _headerFont).Height + 5;

        // Ligne de séparation
        graphics.DrawLine(Pens.Black, MARGIN_LEFT, currentY, MARGIN_LEFT + pageWidth, currentY);
        currentY += 5;

        // Trier les équipements par type puis nom
        var sortedEquipments = equipments
            .OrderBy(e => typeDict.ContainsKey(e.TypeId) ? typeDict[e.TypeId] : "")
            .ThenBy(e => e.Nom ?? e.CodeParc ?? "")
            .ToList();

        var equipmentCount = 0;
        
        foreach (var eq in sortedEquipments)
        {
            var type = typeDict.ContainsKey(eq.TypeId) ? typeDict[eq.TypeId] : "";
            var nom = eq.Nom ?? eq.CodeParc ?? "Sans nom";
            var serie = eq.NumeroSerie ?? "N/A";
            var marque = eq.Marque ?? "N/A";

            // Dessiner la ligne d'équipement
            graphics.DrawString(type, _bodyFont, Brushes.Black, MARGIN_LEFT, currentY);
            graphics.DrawString(nom, _bodyFont, Brushes.Black, MARGIN_LEFT + colWidth, currentY);
            graphics.DrawString(serie, _bodyFont, Brushes.Black, MARGIN_LEFT + colWidth * 2, currentY);
            graphics.DrawString(marque, _bodyFont, Brushes.Black, MARGIN_LEFT + colWidth * 3, currentY);
            
            currentY += LINE_HEIGHT;
            equipmentCount++;
        }

        if (equipmentCount == 0)
        {
            graphics.DrawString("Aucun équipement en prêt", _bodyFont, Brushes.Gray, MARGIN_LEFT, currentY);
            currentY += LINE_HEIGHT;
        }

        // Ligne de fin de tableau
        graphics.DrawLine(Pens.Black, MARGIN_LEFT, currentY, MARGIN_LEFT + pageWidth, currentY);
        currentY += 10;

        // Total
        graphics.DrawString($"Total : {equipmentCount} équipement(s)", _headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        currentY += (int)graphics.MeasureString("A", _headerFont).Height + 10;

        return currentY;
    }

    /// <summary>
    /// Dessine les zones de signature
    /// </summary>
    private void DrawSignatures(Graphics graphics, int startY, int pageWidth, int pageHeight)
    {
        var signatureY = Math.Max(startY, pageHeight - 150); // Au moins 150px du bas
        var signatureWidth = (pageWidth - 60) / 2; // Espace pour 2 signatures

        // Signature de l'agent
        graphics.DrawString("Signature de l'agent :", _bodyFont, Brushes.Black, MARGIN_LEFT, signatureY);
        graphics.DrawString("(Lu et approuvé)", _smallFont, Brushes.Gray, MARGIN_LEFT, signatureY + 15);
        graphics.DrawRectangle(Pens.Black, MARGIN_LEFT, signatureY + 35, signatureWidth, 60);

        // Signature du responsable
        var rightSignatureX = MARGIN_LEFT + signatureWidth + 60;
        graphics.DrawString("Signature du responsable :", _bodyFont, Brushes.Black, rightSignatureX, signatureY);
        graphics.DrawString("(Remise validée)", _smallFont, Brushes.Gray, rightSignatureX, signatureY + 15);
        graphics.DrawRectangle(Pens.Black, rightSignatureX, signatureY + 35, signatureWidth, 60);

        // Note en bas
        var noteY = signatureY + 110;
        var noteText = "Cette feuille de remise fait foi pour la responsabilité des équipements listés ci-dessus.";
        var noteSize = graphics.MeasureString(noteText, _smallFont);
        var noteX = MARGIN_LEFT + (pageWidth - noteSize.Width) / 2;
        graphics.DrawString(noteText, _smallFont, Brushes.Gray, noteX, noteY);
    }

    /// <summary>
    /// Dessine une ligne label/valeur
    /// </summary>
    private int DrawLabelValue(Graphics graphics, string label, string value, int y)
    {
        graphics.DrawString(label, _bodyFont, Brushes.Black, MARGIN_LEFT, y);
        graphics.DrawString(value, _bodyFont, Brushes.Black, MARGIN_LEFT + 100, y);
        return y + LINE_HEIGHT;
    }
}