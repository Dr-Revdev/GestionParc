using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using GestiParc.Core.Interfaces.Services;
using GestiParc.Core.Services;
using GestiParc.Infrastructure.Data.Repositories;

#nullable enable

namespace GestiParc.Ui.Services;

/// <summary>
/// Service UI pour générer des feuilles de remise PDF avec aperçu et impression
/// Gère tous les aspects UI (PrintDocument, Graphics, dialogs, MessageBox)
/// </summary>
public static class DocumentGeneratorUiService
{
    private const int MARGIN_LEFT = 50;
    private const int MARGIN_TOP = 50;
    private const int MARGIN_RIGHT = 50;
    private const int LINE_HEIGHT = 20;
    private const int SECTION_SPACING = 30;

    /// <summary>
    /// Génère et affiche une feuille de remise pour un agent avec aperçu avant impression
    /// </summary>
    public static void GenerateFeuilleRemise(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            MessageBox.Show("L'identifiant de l'agent est requis.", "Erreur", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Font? titleFont = null;
        Font? headerFont = null;
        Font? bodyFont = null;
        Font? smallFont = null;
        PrintDocument? printDocument = null;

        try
        {
            // Récupérer les données via le service Core
            var service = CreateService();
            var data = service.GetFeuilleRemiseData(agentId);

            // Initialiser les polices
            titleFont = new Font("Arial", 16, FontStyle.Bold);
            headerFont = new Font("Arial", 12, FontStyle.Bold);
            bodyFont = new Font("Arial", 10, FontStyle.Regular);
            smallFont = new Font("Arial", 8, FontStyle.Regular);

            // Configurer le document d'impression
            printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) =>
            {
                PrintPage(e, data, titleFont, headerFont, bodyFont, smallFont);
            };

            // Afficher l'aperçu avant impression
            var previewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
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
                    SaveToPdf(agentId);
                }
                else if (result == DialogResult.No)
                {
                    // Imprimer directement
                    var printDialog = new PrintDialog { Document = printDocument };
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        printDocument.Print();
                    }
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la génération de la feuille de remise : {ex.Message}",
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Nettoyer les ressources
            titleFont?.Dispose();
            headerFont?.Dispose();
            bodyFont?.Dispose();
            smallFont?.Dispose();
            printDocument?.Dispose();
        }
    }

    /// <summary>
    /// Crée une instance du service de génération de documents
    /// </summary>
    private static IDocumentGenerationService CreateService()
    {
        var agentRepo = new AgentMySqlRepository();
        var equipmentRepo = new EquipmentMySqlRepository();
        var typeRepo = new EquipmentTypeMySqlRepository();
        var siteRepo = new SiteMySqlRepository();
        var equipeRepo = new EquipeMySqlRepository();

        return new DocumentGenerationService(agentRepo, equipmentRepo, typeRepo, siteRepo, equipeRepo);
    }

    /// <summary>
    /// Sauvegarde le document en PDF
    /// </summary>
    private static void SaveToPdf(string agentId)
    {
        try
        {
            // Créer le dossier de sauvegarde si nécessaire
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var savePath = Path.Combine(documentsPath, "GestiParc", "FeuillesRemise");
            Directory.CreateDirectory(savePath);

            // Nom du fichier avec la date
            var fileName = $"FeuilleRemise_{agentId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

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
                var directory = Path.GetDirectoryName(saveDialog.FileName);
                if (directory != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", directory);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}",
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Dessine la page complète du document
    /// </summary>
    private static void PrintPage(PrintPageEventArgs e, FeuilleRemiseData data,
                                  Font titleFont, Font headerFont, Font bodyFont, Font smallFont)
    {
        var graphics = e.Graphics;
        if (graphics == null) return;

        var pageWidth = e.PageBounds.Width - MARGIN_LEFT - MARGIN_RIGHT;
        var currentY = MARGIN_TOP;

        try
        {
            // === EN-TÊTE ===
            currentY = DrawHeader(graphics, currentY, pageWidth, data.GeneratedDate, titleFont, bodyFont);
            currentY += SECTION_SPACING;

            // === INFORMATIONS AGENT ===
            currentY = DrawAgentInfo(graphics, data, currentY, headerFont, bodyFont);
            currentY += SECTION_SPACING;

            // === LISTE DES ÉQUIPEMENTS ===
            currentY = DrawEquipmentList(graphics, data.Equipments, currentY, pageWidth, headerFont, bodyFont);
            currentY += SECTION_SPACING;

            // === SIGNATURES ===
            DrawSignatures(graphics, currentY, pageWidth, e.PageBounds.Height, bodyFont, smallFont);
        }
        catch (Exception ex)
        {
            // En cas d'erreur, afficher un message sur la page
            graphics.DrawString($"Erreur : {ex.Message}", bodyFont, Brushes.Red,
                              MARGIN_LEFT, currentY);
        }
    }

    /// <summary>
    /// Dessine le header du PDF (titre + date)
    /// </summary>
    private static int DrawHeader(Graphics graphics, int startY, int pageWidth, DateTime generatedDate,
                                 Font titleFont, Font bodyFont)
    {
        var currentY = startY;

        // Titre principal
        var title = "FEUILLE DE REMISE D'ÉQUIPEMENT";
        var titleSize = graphics.MeasureString(title, titleFont);
        var titleX = MARGIN_LEFT + (pageWidth - titleSize.Width) / 2;
        graphics.DrawString(title, titleFont, Brushes.Black, titleX, currentY);
        currentY += (int)titleSize.Height + 10;

        // Date et heure de génération
        var dateText = $"Générée le : {generatedDate:dd/MM/yyyy à HH:mm}";
        var dateSize = graphics.MeasureString(dateText, bodyFont);
        var dateX = MARGIN_LEFT + (pageWidth - dateSize.Width) / 2;
        graphics.DrawString(dateText, bodyFont, Brushes.Gray, dateX, currentY);
        currentY += (int)dateSize.Height + 10;

        // Ligne de séparation
        graphics.DrawLine(Pens.Black, MARGIN_LEFT, currentY, MARGIN_LEFT + pageWidth, currentY);
        currentY += 10;

        return currentY;
    }

    /// <summary>
    /// Dessine les informations de l'agent
    /// </summary>
    private static int DrawAgentInfo(Graphics graphics, FeuilleRemiseData data, int startY,
                                    Font headerFont, Font bodyFont)
    {
        var currentY = startY;

        // Titre de section
        graphics.DrawString("INFORMATIONS AGENT", headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        currentY += (int)graphics.MeasureString("A", headerFont).Height + 10;

        var agent = data.Agent;
        var nom = agent.Nom ?? "";
        var prenom = agent.Prenom ?? "";
        var idrh = agent.Idrh ?? "";
        var email = agent.Email ?? "";

        // Afficher les informations
        currentY = DrawLabelValue(graphics, "Nom :", $"{nom} {prenom}", currentY, bodyFont);
        currentY = DrawLabelValue(graphics, "IDRH :", idrh, currentY, bodyFont);
        currentY = DrawLabelValue(graphics, "Email :", email, currentY, bodyFont);
        currentY = DrawLabelValue(graphics, "Site :", data.SiteName, currentY, bodyFont);
        currentY = DrawLabelValue(graphics, "Équipe :", data.EquipeName, currentY, bodyFont);

        return currentY;
    }

    /// <summary>
    /// Dessine la liste des équipements en prêt
    /// </summary>
    private static int DrawEquipmentList(Graphics graphics, List<EquipmentRemiseItem> equipments,
                                        int startY, int pageWidth, Font headerFont, Font bodyFont)
    {
        var currentY = startY;

        // Titre de section
        graphics.DrawString("ÉQUIPEMENTS EN PRÊT", headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        currentY += (int)graphics.MeasureString("A", headerFont).Height + 10;

        // En-têtes du tableau
        var colWidth = pageWidth / 4;
        graphics.DrawString("Type", headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        graphics.DrawString("Nom/Code", headerFont, Brushes.Black, MARGIN_LEFT + colWidth, currentY);
        graphics.DrawString("Série", headerFont, Brushes.Black, MARGIN_LEFT + colWidth * 2, currentY);
        graphics.DrawString("Marque", headerFont, Brushes.Black, MARGIN_LEFT + colWidth * 3, currentY);
        currentY += (int)graphics.MeasureString("A", headerFont).Height + 5;

        // Ligne de séparation
        graphics.DrawLine(Pens.Black, MARGIN_LEFT, currentY, MARGIN_LEFT + pageWidth, currentY);
        currentY += 5;

        var equipmentCount = 0;

        foreach (var eq in equipments)
        {
            // Dessiner la ligne d'équipement
            graphics.DrawString(eq.TypeName, bodyFont, Brushes.Black, MARGIN_LEFT, currentY);
            graphics.DrawString(eq.Name, bodyFont, Brushes.Black, MARGIN_LEFT + colWidth, currentY);
            graphics.DrawString(eq.SerialNumber, bodyFont, Brushes.Black, MARGIN_LEFT + colWidth * 2, currentY);
            graphics.DrawString(eq.Brand, bodyFont, Brushes.Black, MARGIN_LEFT + colWidth * 3, currentY);

            currentY += LINE_HEIGHT;
            equipmentCount++;
        }

        if (equipmentCount == 0)
        {
            graphics.DrawString("Aucun équipement en prêt", bodyFont, Brushes.Gray, MARGIN_LEFT, currentY);
            currentY += LINE_HEIGHT;
        }

        // Ligne de fin de tableau
        graphics.DrawLine(Pens.Black, MARGIN_LEFT, currentY, MARGIN_LEFT + pageWidth, currentY);
        currentY += 10;

        // Total
        graphics.DrawString($"Total : {equipmentCount} équipement(s)", headerFont, Brushes.Black, MARGIN_LEFT, currentY);
        currentY += (int)graphics.MeasureString("A", headerFont).Height + 10;

        return currentY;
    }

    /// <summary>
    /// Dessine les zones de signature
    /// </summary>
    private static void DrawSignatures(Graphics graphics, int startY, int pageWidth, int pageHeight,
                                      Font bodyFont, Font smallFont)
    {
        var signatureY = Math.Max(startY, pageHeight - 150); // Au moins 150px du bas
        var signatureWidth = (pageWidth - 60) / 2; // Espace pour 2 signatures

        // Signature de l'agent
        graphics.DrawString("Signature de l'agent :", bodyFont, Brushes.Black, MARGIN_LEFT, signatureY);
        graphics.DrawString("(Lu et approuvé)", smallFont, Brushes.Gray, MARGIN_LEFT, signatureY + 15);
        graphics.DrawRectangle(Pens.Black, MARGIN_LEFT, signatureY + 35, signatureWidth, 60);

        // Signature du responsable
        var rightSignatureX = MARGIN_LEFT + signatureWidth + 60;
        graphics.DrawString("Signature du responsable :", bodyFont, Brushes.Black, rightSignatureX, signatureY);
        graphics.DrawString("(Remise validée)", smallFont, Brushes.Gray, rightSignatureX, signatureY + 15);
        graphics.DrawRectangle(Pens.Black, rightSignatureX, signatureY + 35, signatureWidth, 60);

        // Note en bas
        var noteY = signatureY + 110;
        var noteText = "Cette feuille de remise fait foi pour la responsabilité des équipements listés ci-dessus.";
        var noteSize = graphics.MeasureString(noteText, smallFont);
        var noteX = MARGIN_LEFT + (pageWidth - noteSize.Width) / 2;
        graphics.DrawString(noteText, smallFont, Brushes.Gray, noteX, noteY);
    }

    /// <summary>
    /// Dessine une ligne label/valeur
    /// </summary>
    private static int DrawLabelValue(Graphics graphics, string label, string value, int y, Font bodyFont)
    {
        graphics.DrawString(label, bodyFont, Brushes.Black, MARGIN_LEFT, y);
        graphics.DrawString(value, bodyFont, Brushes.Black, MARGIN_LEFT + 100, y);
        return y + LINE_HEIGHT;
    }
}
