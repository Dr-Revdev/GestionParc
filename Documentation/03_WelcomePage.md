# 📘 Documentation de WelcomePage.cs

## 🎯 But de ce fichier
C'est la **page d'accueil** de l'application. C'est la première chose que l'utilisateur voit quand il lance le programme. Elle affiche :
- Une **barre d'outils** en haut avec le bouton "Export CSV"
- **3 gros boutons** pour naviguer vers les différentes fonctionnalités

💡 **Analogie :** C'est comme le hall d'entrée d'un bâtiment avec des panneaux indiquant les différents services.

---

## 📦 Les imports (using)

```csharp
using System;
using System.Drawing;  
using System.Windows.Forms;
using ProjetParc.Views.Admin;
using ProjetParc.Views.Agent;
using ProjetParc.Views.Equipment;
using ProjetParc.Views.Loan;
using ProjetParc.Views.Inventory;
```

Tous les `using` importent des fonctionnalités nécessaires :

- **Lignes 1-3** : Bibliothèques de base Windows Forms (dessiner, contrôles UI, événements)
- **Lignes 4-8** : Toutes les autres vues de l'application

💡 Ces imports permettent d'utiliser les autres écrans sans écrire le chemin complet à chaque fois.

---

## 🏷️ Le namespace

```csharp
namespace ProjetParc.Views;
```

Adresse : `ProjetParc.Views` - Le dossier des vues principales.

---

## 🏛️ La classe WelcomePage

```csharp
public class WelcomePage : Form
```

**`Form`** - Hérite de Form = C'est une fenêtre Windows

💡 **Important :** C'est la fenêtre principale affichée par `Program.cs` au démarrage.

---

## 📦 Les variables de classe

```csharp
private Panel content;
private Button btnSetEquipment;
private Button btnFreeEquipment;
private Button btnNewMod;
private Label title;
```

**`content`** - Le panneau qui change de contenu selon la navigation

**`btnSetEquipment`, `btnFreeEquipment`, `btnNewMod`** - Les 3 boutons principaux de la page d'accueil

**`title`** - Le label "Gestion de Parc" en haut

💡 **Pourquoi private ?** Ces contrôles ne sont utilisés que dans cette classe.

---

## 🎨 Le constructeur - Création de l'interface

### **Configuration de la fenêtre**

```csharp
Text = "Gestion Parc";
WindowState = FormWindowState.Maximized;
StartPosition = FormStartPosition.CenterScreen;
MinimumSize = new Size(800, 600);
```

- **Titre de la fenêtre** : "Gestion Parc" (barre supérieure)
- **Fenêtre maximisée** au démarrage
- **Centrée** sur l'écran
- **Taille minimale** : 800x600 pixels

---

### **Layout principal**

```csharp
var mainLayout = new TableLayoutPanel
{
    Dock = DockStyle.Fill,
    BackColor = Color.White,
    RowCount = 2,
    ColumnCount = 1,
    Padding = new Padding(20)
};

mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // En-tête
mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu
```

**TableLayoutPanel** = Grille organisée en lignes et colonnes

**Configuration :**
- 2 lignes, 1 colonne
- Ligne 1 : Hauteur fixe de 120px (pour le titre)
- Ligne 2 : Prend tout l'espace restant (pour le contenu)

---

### **Création du titre**

```csharp
title = new Label
{
    Text = "Gestion de Parc",
    Font = new Font("Segoe UI", 28f, FontStyle.Bold),
    Dock = DockStyle.Fill,
    TextAlign = ContentAlignment.MiddleCenter
};
```

- Police Segoe UI taille 28, en gras
- Centré au milieu (horizontal + vertical)

---

### **Panneau de contenu et layout des boutons**

```csharp
content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

var buttonLayout = new TableLayoutPanel
{
    Dock = DockStyle.Fill,
    ColumnCount = 3,
    RowCount = 1,
    Padding = new Padding(10)
};
```

- `content` est le panneau qui changera selon la navigation
- `buttonLayout` organise les 3 boutons en colonnes

**Boucle pour créer 3 colonnes égales (33.33% chacune)**

---

### **Création des 3 boutons**

```csharp
var tileFont = new Font("Segoe UI", 14f, FontStyle.Bold);
btnSetEquipment = new Button
{
    Text = "Equipements en place",
    Font = tileFont,
    Dock = DockStyle.None,
    Size = new Size(400, 250),
    Anchor = AnchorStyles.None,
    Margin = new Padding(10)
};
```

**Même pattern pour les 3 boutons :**
1. **btnSetEquipment** - "Equipements en place"
2. **btnFreeEquipment** - "Equipements disponibles"
3. **btnNewMod** - "Modification / Création"

**Propriétés importantes :**
- **`Size = new Size(400, 250)`** - Taille fixe des boutons (400px × 250px)
- **`Anchor = AnchorStyles.None`** - Centre les boutons dans leur cellule
- **`Margin = new Padding(10)`** - Espace de 10px autour

---

### **Liaison des événements**

```csharp
btnNewMod.Click += (_, __) => ShowAdminMenu();
btnFreeEquipment.Click += (_, __) => ShowEquipmentFree();
btnSetEquipment.Click += (_, __) => ShowMainInventoryPage();
```

**Syntaxe lambda** : `(_, __) => Méthode()`

Quand on clique sur un bouton, appelle la méthode correspondante :
- `btnNewMod` → `ShowAdminMenu()`
- `btnFreeEquipment` → `ShowEquipmentFree()`
- `btnSetEquipment` → `ShowMainInventoryPage()`

💡 **`_` et `__`** = paramètres ignorés (sender et EventArgs non utilisés)

---

### **🆕 Barre d'outils (ToolStrip)**

```csharp
// Barre d'outils
var toolStrip = new ToolStrip 
{ 
    Dock = DockStyle.Top,
    GripStyle = ToolStripGripStyle.Hidden
};

var btnExportTool = new ToolStripButton
{
    Text = "Export CSV",
    DisplayStyle = ToolStripItemDisplayStyle.Text
};
btnExportTool.Click += (s, e) => ShowExportMenu();

toolStrip.Items.Add(btnExportTool);

// 🆕 v1.1.0 : Bouton de sauvegarde SharePoint
if (Data.Database.SyncManager.IsActive)
{
    var btnSaveTool = new ToolStripButton
    {
        Text = "💾 Sauvegarder",
        DisplayStyle = ToolStripItemDisplayStyle.Text
    };
    btnSaveTool.Click += (s, e) => SaveToSharePoint();
    toolStrip.Items.Add(btnSaveTool);
}

Controls.Add(toolStrip);

// 🆕 v1.1.0 : Confirmation à la fermeture
FormClosing += OnFormClosing;
```

**Nouveauté : Barre d'outils en haut de la fenêtre**

**`ToolStrip`**
- Barre d'outils Windows classique
- **`Dock = DockStyle.Top`** - Positionnée tout en haut de la fenêtre
- **`GripStyle = ToolStripGripStyle.Hidden`** - Pas de poignée de déplacement

**`ToolStripButton "Export CSV"`**
- Bouton dans la barre d'outils
- **Text = "Export CSV"** - Texte du bouton
- **Click** → Appelle `ShowExportMenu()` qui affiche le menu d'export

**🆕 `ToolStripButton "� Sauvegarder"` (v1.1.0)**
- **Visible uniquement si SharePoint est actif**
- Permet de sauvegarder manuellement vers SharePoint
- **Click** → Appelle `SaveToSharePoint()`
- Toujours visible, accessible depuis toutes les pages

**🆕 `FormClosing` (v1.1.0)**
- Événement qui se déclenche avant la fermeture de la fenêtre
- Permet d'afficher une confirmation de sauvegarde
- **Peut annuler la fermeture** si l'utilisateur clique sur "Annuler"

�💡 **Utilité :** Accès rapide aux fonctions importantes depuis n'importe quelle page de l'application.

---

### **Assemblage final**

```csharp
content.Controls.Add(buttonLayout);
mainLayout.Controls.Add(headerPanel, 0, 0);
mainLayout.Controls.Add(content, 0, 1);
Controls.Add(mainLayout);

// Affiche le panneau d'accueil contenant les trois tuiles
ShowHome();
```

**Ordre d'imbrication :**
```
WelcomePage (Form)
├─ toolStrip (ToolStrip)
│  ├─ btnExportTool (ToolStripButton)
│  └─ btnSaveTool (ToolStripButton) 🆕 v1.1.0 (si SharePoint actif)
└─ mainLayout (TableLayoutPanel)
   ├─ headerPanel [ligne 0]
   │  └─ title (Label)
   └─ content [ligne 1]
      └─ buttonLayout (TableLayoutPanel)
         ├─ btnSetEquipment [colonne 0]
         ├─ btnFreeEquipment [colonne 1]
         └─ btnNewMod [colonne 2]
```

**Ligne finale :** Appel de `ShowHome()` pour finaliser l'affichage

---

## 🏠 ShowHome() - Afficher l'accueil

```csharp
private void ShowHome()
{
    content.Controls.Clear();

    var buttonLayout = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 1,
        Padding = new Padding(40, 20, 40, 20)
    };

    for (int i = 0; i < 3; i++)
    {
        buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    }

    buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
    buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
    buttonLayout.Controls.Add(btnNewMod, 2, 0);

    content.Controls.Add(buttonLayout);
}
```

**Étapes :**
1. **Vide** le panneau `content`
2. **Recrée** le layout des boutons
3. **Réajoute** les 3 boutons

💡 **Pourquoi recréer ?** Pour revenir proprement à l'accueil après avoir navigué ailleurs.

---

## 🔧 ShowAdminMenu() - Menu d'administration

```csharp
private void ShowAdminMenu()
{
    content.Controls.Clear();

    var admin = new AdminMenuView(
        onBack: ShowHome, 
        onCreateEquipment: ShowEquipmentCreate, 
        onCreateAgent: ShowAgentCreate, 
        onEditAgent: ShowAgentEdit, 
        onEditEquipment: ShowEquipmentEdit
    );
    admin.Dock = DockStyle.Fill;
    content.Controls.Add(admin);
}
```

**Crée une instance d'`AdminMenuView` avec 5 callbacks :**
- `onBack` → `ShowHome` (revenir à l'accueil)
- `onCreateEquipment` → `ShowEquipmentCreate`
- `onCreateAgent` → `ShowAgentCreate`
- `onEditAgent` → `ShowAgentEdit`
- `onEditEquipment` → `ShowEquipmentEdit`

💡 **Pattern callback :** AdminMenuView appelle ces méthodes quand l'utilisateur clique sur ses boutons.

---

## 📦 Les autres méthodes Show*

Toutes suivent le même pattern :

```csharp
private void ShowXXX()
{
    content.Controls.Clear();
    content.Controls.Add(new XXXView(onBack: ...) { Dock = DockStyle.Fill });
}
```

1. **Vider** le panneau content
2. **Créer** une nouvelle vue
3. **Ajouter** la vue au panneau

**Liste des vues :**

| Méthode | Vue affichée | Callback retour |
|---------|--------------|-----------------|
| `ShowEquipmentFree()` | `FreeEquipmentView` | `ShowHome` |
| `ShowMainInventoryPage()` | `MainInventoryView` | `ShowHome` |
| `ShowEquipmentCreate()` | `EquipmentCreateView` | `ShowAdminMenu` |
| `ShowAgentCreate()` | `AgentCreateView` | `ShowAdminMenu` |
| `ShowAgentEdit()` | `AgentEditView` | `ShowAdminMenu` |
| `ShowEquipmentEdit()` | `EquipementEditView` | `ShowAdminMenu` |

---

## 🆕 ShowExportMenu() - Menu d'export CSV

```csharp
private void ShowExportMenu()
{
    var exportForm = new Form
    {
        Text = "Export CSV",
        Size = new Size(500, 450),
        StartPosition = FormStartPosition.CenterParent,
        FormBorderStyle = FormBorderStyle.Sizable,
        MaximizeBox = true,
        MinimizeBox = true,
        MinimumSize = new Size(450, 400)
    };

    var layout = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 7,
        Padding = new Padding(20)
    };

    for (int i = 0; i < 6; i++)
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
```

**Nouveauté : Fenêtre modale d'export**

**Crée une nouvelle fenêtre popup :**
- **Taille :** 500×450 pixels (redimensionnable)
- **Position :** Centrée sur la fenêtre parent
- **7 lignes :** Titre + 3 exports individuels + séparateur + export complet + espace

---

### **Les boutons d'export**

**1. Export Agents**

```csharp
var btnExpAgents = new Button
{
    Text = "Exporter les Agents",
    Dock = DockStyle.Fill,
    Height = 40
};
btnExpAgents.Click += (s, e) =>
{
    var path = Data.CsvExporter.SelectExportFile("agents.csv");
    if (path != null) Data.CsvExporter.ExportAgents(path);
};
layout.Controls.Add(btnExpAgents, 0, 1);
```

**Fonctionnement :**
1. Affiche un dialogue "Enregistrer sous"
2. Nom par défaut : `agents.csv`
3. Si l'utilisateur valide → Export dans le fichier choisi
4. Appelle `CsvExporter.ExportAgents()`

---

**2. Export Équipements**

```csharp
var btnExpEquip = new Button
{
    Text = "Exporter les Équipements",
    Dock = DockStyle.Fill,
    Height = 40
};
btnExpEquip.Click += (s, e) =>
{
    var path = Data.CsvExporter.SelectExportFile("equipements.csv");
    if (path != null) Data.CsvExporter.ExportEquipements(path);
};
layout.Controls.Add(btnExpEquip, 0, 2);
```

**Export tous les équipements** avec leur type, état, agent assigné, etc.

---

**3. Export Prêts actifs**

```csharp
var btnExpPrets = new Button
{
    Text = "Exporter les Prêts actifs",
    Dock = DockStyle.Fill,
    Height = 40
};
btnExpPrets.Click += (s, e) =>
{
    var path = Data.CsvExporter.SelectExportFile("prets_actifs.csv");
    if (path != null) Data.CsvExporter.ExportPrets(path);
};
layout.Controls.Add(btnExpPrets, 0, 3);
```

**Export format pivot :** Une ligne par agent, équipements en colonnes (max 6 colonnes par équipement).

---

**4. Séparateur "ou"**

```csharp
var separator = new Label
{
    Text = "ou",
    Dock = DockStyle.Fill,
    TextAlign = ContentAlignment.MiddleCenter,
    ForeColor = Color.Gray
};
layout.Controls.Add(separator, 0, 4);
```

Sépare visuellement les exports individuels de l'export complet.

---

**5. Export complet**

```csharp
var btnExpComplet = new Button
{
    Text = "Export complet (tous les fichiers)",
    Dock = DockStyle.Fill,
    Height = 40,
    Font = new Font("Segoe UI", 10f, FontStyle.Bold)
};
btnExpComplet.Click += (s, e) =>
{
    var folder = Data.CsvExporter.SelectExportFolder();
    if (folder != null)
    {
        Data.CsvExporter.ExportComplet(folder);
        exportForm.Close();
    }
};
layout.Controls.Add(btnExpComplet, 0, 5);
```

**Fonctionnement :**
1. Affiche un dialogue de sélection de dossier
2. Crée un sous-dossier avec timestamp : `Export_GestionParc_20251024_143052`
3. Génère **3 fichiers CSV** :
   - `Agents.csv`
   - `Equipements.csv`
   - `Prets_Actifs.csv`
4. Crée un fichier `README.txt` explicatif
5. **Ferme** la fenêtre d'export après succès
6. **Ouvre** l'explorateur Windows sur le dossier créé

💡 **En gras** : Mise en valeur de l'option recommandée.

---

### **Affichage de la fenêtre**

```csharp
exportForm.Controls.Add(layout);
exportForm.ShowDialog();
```

**`ShowDialog()`** - Affiche la fenêtre en mode **modal** :
- Bloque la fenêtre principale
- L'utilisateur doit fermer cette fenêtre avant de continuer
- Parfait pour les actions importantes comme l'export

---

## 💾 SaveToSharePoint() - Sauvegarde vers SharePoint 🆕 v1.1.0

```csharp
private void SaveToSharePoint()
{
    if (!Data.Database.SyncManager.IsActive)
        return;

    try
    {
        Data.Database.SyncManager.CopyToSharePoint();
        MessageBox.Show(
            "Sauvegarde vers SharePoint réussie !",
            "Succès",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    catch (Data.SharePointSyncException ex)
    {
        MessageBox.Show(
            $"Erreur lors de la sauvegarde vers SharePoint :\n\n{ex.Message}\n\nVos modifications locales sont conservées.",
            "Erreur de sauvegarde",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
```

**Nouveauté v1.1.0 : Sauvegarde manuelle**

**Fonctionnement :**
1. Vérifie que SharePoint est actif
2. Appelle `SyncManager.CopyToSharePoint()` :
   - Fait un **checkpoint WAL** (fusionne les modifications SQLite)
   - Copie tous les fichiers de la base locale vers SharePoint
3. Affiche un message de succès ou d'erreur

**Gestion des erreurs :**
- Si échec → Message d'erreur détaillé
- **Rassure l'utilisateur :** "Vos modifications locales sont conservées"
- Les données ne sont pas perdues, juste pas encore synchronisées

💡 **Accessible via :** Bouton "💾 Sauvegarder" dans la barre d'outils

---

## 🚪 OnFormClosing() - Confirmation à la fermeture 🆕 v1.1.0

```csharp
private void OnFormClosing(object sender, FormClosingEventArgs e)
{
    if (!Data.Database.SyncManager.IsActive)
        return;

    var result = MessageBox.Show(
        "Voulez-vous sauvegarder les modifications vers SharePoint avant de quitter ?",
        "Sauvegarder avant de quitter",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question
    );

    if (result == DialogResult.Cancel)
    {
        // Annuler la fermeture
        e.Cancel = true;
        return;
    }

    if (result == DialogResult.Yes)
    {
        try
        {
            // Sauvegarder vers SharePoint
            Data.Database.SyncManager.CopyToSharePoint();
        }
        catch (Data.SharePointSyncException ex)
        {
            var retry = MessageBox.Show(
                $"Erreur lors de la sauvegarde :\n\n{ex.Message}\n\nVoulez-vous quitter quand même sans sauvegarder ?",
                "Erreur de sauvegarde",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error
            );

            if (retry == DialogResult.No)
            {
                // Annuler la fermeture pour réessayer
                e.Cancel = true;
                return;
            }
        }
    }

    // Si on arrive ici (Yes avec succès ou No), on peut fermer
}
```

**Nouveauté v1.1.0 : Confirmation avant fermeture**

**Déclenché quand :**
- L'utilisateur clique sur le X de la fenêtre
- L'utilisateur utilise Alt+F4
- L'application se ferme pour une autre raison

**Fonctionnement :**
1. **Si SharePoint actif** → Affiche un dialogue de confirmation
2. **3 choix possibles :**
   - **Oui** → Sauvegarde puis ferme
   - **Non** → Ferme sans sauvegarder
   - **Annuler** → `e.Cancel = true` **empêche la fermeture** ✨

**Gestion des erreurs de sauvegarde :**
- Si la sauvegarde échoue → Demande "Quitter quand même ?"
- **Non** → Reste ouvert, permet de réessayer
- **Oui** → Ferme sans sauvegarder (modifications en local)

💡 **Important :** C'est le seul endroit où le bouton "Annuler" **empêche réellement** la fermeture de l'application. `e.Cancel = true` est la clé !

---

**Scénario typique :**
```
1. Utilisateur travaille depuis 1h
   ↓
2. Clique sur X pour fermer
   ↓
3. Dialogue : "Sauvegarder avant de quitter ?"
   ↓
4. Clique sur "Annuler" → L'app reste ouverte ✅
   OU
   Clique sur "Oui" → Sauvegarde puis ferme ✅
   OU
   Clique sur "Non" → Ferme directement ✅
```

**Comportement Excel/Word :**
C'est le même principe que Microsoft Office - toujours demander avant de fermer si modifications non sauvegardées !

---

## 🎬 Scénario de navigation

```
1. Démarrage
   → WelcomePage affiche 3 boutons + barre d'outils
   → 🆕 Si SharePoint actif : Bouton "💾 Sauvegarder" visible

2. Clic sur "Modification / Création"
   → ShowAdminMenu() remplace content par AdminMenuView

3. Dans AdminMenuView, clic sur "Création Agent"
   → ShowAgentCreate() remplace content par AgentCreateView

4. Dans AgentCreateView, clic sur "← Retour"
   → ShowAdminMenu() réaffiche AdminMenuView

5. Dans AdminMenuView, clic sur "← Retour"
   → ShowHome() réaffiche les 3 boutons d'accueil

6. Clic sur "Export CSV" (barre d'outils)
   → ShowExportMenu() affiche une fenêtre popup
   → Choix entre 3 exports individuels ou 1 export complet

7. 🆕 Clic sur "💾 Sauvegarder" (barre d'outils) - v1.1.0
   → SaveToSharePoint() sauvegarde vers SharePoint
   → Message de confirmation

8. 🆕 Clic sur X pour fermer l'application - v1.1.0
   → OnFormClosing() affiche dialogue de confirmation
   → Options : Oui (sauvegarde), Non (ferme), Annuler (reste ouvert)
```

---

## 🎓 Concepts clés

### **1. Navigation par remplacement**
- On vide `content.Controls`
- On ajoute une nouvelle vue
- Pas de pile de navigation, tout est remplacé

### **2. Pattern callback**
- Les vues reçoivent des méthodes en paramètre
- Elles les appellent pour naviguer
- Flexibilité : chaque vue peut revenir où on veut

### **3. TableLayoutPanel**
- Grille responsive
- Pourcentages pour adaptation automatique
- Idéal pour layouts propres

### **4. Réutilisation des boutons**
- Les boutons sont créés une fois
- Réajoutés au layout dans `ShowHome()`
- Performance : pas de recréation inutile

### **5. Barre d'outils globale**
- **`ToolStrip`** en haut de la fenêtre
- Accessible depuis toutes les pages
- Accès rapide aux fonctions importantes (export)

### **6. Fenêtre modale pour actions importantes**
- **`ShowDialog()`** bloque l'interface principale
- Force l'utilisateur à choisir avant de continuer
- Évite les erreurs de navigation

### **7. Sauvegarde SharePoint manuelle** 🆕 v1.1.0
- Bouton toujours visible dans la barre d'outils
- Checkpoint WAL avant copie (garantit intégrité)
- Messages clairs de succès/erreur

### **8. Confirmation de fermeture cancellable** 🆕 v1.1.0
- Événement `FormClosing` avant la fermeture
- `e.Cancel = true` peut empêcher la fermeture
- Comportement Excel/Word : toujours demander

---

## 💡 Questions fréquentes

**Q : Pourquoi content est un Panel et pas directement la Form ?**
- R : Pour pouvoir changer facilement le contenu sans toucher au titre et à la barre d'outils.

**Q : Que se passe-t-il si on clique plusieurs fois sur un bouton ?**
- R : `Clear()` supprime l'ancienne vue, puis on ajoute la nouvelle. Pas de doublon.

**Q : Pourquoi les méthodes Show* sont private ?**
- R : Elles ne sont utilisées que dans cette classe, pas besoin de les exposer.

**Q : Comment ajouter un nouveau bouton sur la page d'accueil ?**
- R : 1) Déclarer le bouton (variables de classe), 2) Le créer dans le constructeur, 3) L'ajouter au buttonLayout, 4) Créer une méthode Show*, 5) Lier l'événement Click.

**Q : L'export CSV fonctionne-t-il depuis n'importe quelle page ?**
- R : Oui ! La barre d'outils est toujours visible en haut, donc le bouton "Export CSV" est accessible partout.

**Q : Pourquoi 3 exports séparés + 1 export complet ?**
- R : Flexibilité. Parfois on veut juste les agents, parfois tout. L'export complet regroupe les 3 fichiers dans un dossier daté.

**Q : Le bouton "Sauvegarder" est-il toujours visible ?** 🆕 v1.1.0
- R : Oui, mais uniquement si SharePoint/OneDrive est détecté. Sinon, il n'apparaît pas (mode local normal).

**Q : Que se passe-t-il si je ferme sans sauvegarder ?** 🆕 v1.1.0
- R : Les modifications restent en local. Au prochain démarrage, tu pourras les sauvegarder. Mais attention : d'autres utilisateurs ne verront pas tes modifications !

**Q : Puis-je annuler la fermeture de l'application ?** 🆕 v1.1.0
- R : Oui ! Clique sur "Annuler" dans le dialogue de confirmation. C'est la seule façon de vraiment empêcher la fermeture.

---

## 🔗 Fichiers liés

- **Program.cs** - Lance cette page au démarrage
- **AdminMenuView.cs** - Menu d'administration
- **FreeEquipmentView.cs** - Équipements disponibles
- **MainInventoryView.cs** - Inventaire complet
- **AgentCreateView.cs**, **AgentEditView.cs** - Gestion des agents
- **EquipmentCreateView.cs**, **EquipmentEditView.cs** - Gestion des équipements
- **CsvExporter.cs** 🆕 - Logique d'export CSV

---

**📌 Prochaine étape :** Consulter `AdminMenuView.cs` pour voir comment fonctionnent les sous-menus, ou `CsvExporter.cs` pour comprendre la génération des fichiers CSV.
