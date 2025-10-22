# 📘 Documentation de WelcomePage.cs

## 🎯 But de ce fichier
C'est la **page d'accueil** de l'application. C'est la première chose que l'utilisateur voit quand il lance le programme. Elle affiche 3 gros boutons pour naviguer vers les différentes fonctionnalités.

💡 **Analogie :** C'est comme le hall d'entrée d'un bâtiment avec des panneaux indiquant les différents services.

---

## 📋 Le code complet

```csharp
1   using System;
2   using System.Drawing;  
3   using System.Windows.Forms;
4   using ProjetParc.Views.Admin;
5   using ProjetParc.Views.Agent;
6   using ProjetParc.Views.Equipment;
7   using ProjetParc.Views.Loan;
8   using ProjetParc.Views.Inventory;
9   using System.Windows.Forms.Integration;
10
11  namespace ProjetParc.Views;
12
13  /// <summary>
14  /// Page d'accueil principale de l'application de gestion de parc
15  /// Fournit l'accès aux différentes fonctionnalités via une interface graphique
16  /// </summary>
17  public class WelcomePage : Form
18  {
19      private Panel content;
20      private Button btnSetEquipment;
21      private Button btnFreeEquipment;
22      private Button btnNewMod;
23      private Label title;
24
25      /// <summary>
26      /// Initialise la page d'accueil et prépare la navigation entre les vues.
27      /// Définit la taille de la fenêtre, crée les boutons et attache les handlers.
28      /// </summary>
29      public WelcomePage()
30      {
31          Text = "Gestion Parc";
32          WindowState = FormWindowState.Maximized;
33          StartPosition = FormStartPosition.CenterScreen;
34          MinimumSize = new Size(800, 600);
35
36          // Création du layout principal
37          var mainLayout = new TableLayoutPanel
38          {
39              Dock = DockStyle.Fill,
40              BackColor = Color.White,
41              RowCount = 2,
42              ColumnCount = 1,
43              Padding = new Padding(20)
44          };
45
46          // Configuration des lignes
47          mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // En-tête
48          mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu
49
50          // En-tête avec titre
51          var headerPanel = new TableLayoutPanel 
52          { 
53              Dock = DockStyle.Fill,
54              RowCount = 1,
55              ColumnCount = 1
56          };
57          
58          title = new Label
59          {
60              Text = "Gestion de Parc",
61              Font = new Font("Segoe UI", 28f, FontStyle.Bold),
62              Dock = DockStyle.Fill,
63              TextAlign = ContentAlignment.MiddleCenter
64          };
65          headerPanel.Controls.Add(title, 0, 0);
66
67          // Panneau de contenu
68          content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
69
70          // Configuration du layout des boutons
71          var buttonLayout = new TableLayoutPanel
72          {
73              Dock = DockStyle.Fill,
74              ColumnCount = 3,
75              RowCount = 1,
76              Padding = new Padding(10)
77          };
78
79          // Configuration des colonnes pour les boutons (répartition égale)
80          for (int i = 0; i < 3; i++)
81          {
82              buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
83          }
84
85          // Configuration des boutons
86          var tileFont = new Font("Segoe UI", 16f, FontStyle.Bold);
87          btnSetEquipment = new Button
88          {
89              Text = "Equipements en place",
90              Font = tileFont,
91              Dock = DockStyle.Fill,
92              Margin = new Padding(10)
93          };
94          btnFreeEquipment = new Button
95          {
96              Text = "Equipements disponibles",
97              Font = tileFont,
98              Dock = DockStyle.Fill,
99              Margin = new Padding(10)
100         };
101         btnNewMod = new Button
102         {
103             Text = "Modification / Création",
104             Font = tileFont,
105             Dock = DockStyle.Fill,
106             Margin = new Padding(10)
107         };
108
109         // Ajout des boutons au layout
110         buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
111         buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
112         buttonLayout.Controls.Add(btnNewMod, 2, 0);
113
114         // Ajout des événements
115         btnNewMod.Click += (_, __) => ShowAdminMenu();
116         btnFreeEquipment.Click += (_, __) => ShowEquipmentFree();
117         btnSetEquipment.Click += (_, __) => ShowMainInventoryPage();
118
119         // Assemblage final
120         content.Controls.Add(buttonLayout);
121         mainLayout.Controls.Add(headerPanel, 0, 0);
122         mainLayout.Controls.Add(content, 0, 1);
123         Controls.Add(mainLayout);
124
125         // Affiche le panneau d'accueil contenant les trois tuiles
126         ShowHome();
127
128     }
129
130     /// <summary>
131     /// Affiche l'écran d'accueil avec les tuiles de navigation principales.
132     /// Réutilise les boutons créés dans le constructeur pour éviter la recréation.
133     /// </summary>
134     private void ShowHome()
135     {
136         content.Controls.Clear();
137
138         var buttonLayout = new TableLayoutPanel
139         {
140             Dock = DockStyle.Fill,
141             ColumnCount = 3,
142             RowCount = 1,
143             Padding = new Padding(50)
144         };
145
146         // Configuration des colonnes pour les boutons (répartition égale)
147         for (int i = 0; i < 3; i++)
148         {
149             buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
150         }
151
152         buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
153         buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
154         buttonLayout.Controls.Add(btnNewMod, 2, 0);
155
156         content.Controls.Add(buttonLayout);
157     }
158
159     /// <summary>
160     /// Remplace le contenu par la vue d'administration (création / modification).
161     /// La vue admin est initialisée avec des callbacks pointant vers les méthodes Show* de cette classe.
162     /// </summary>
163     private void ShowAdminMenu()
164     {
165         content.Controls.Clear();
166
167         var admin = new AdminMenuView(onBack: ShowHome, onCreateEquipment: ShowEquipmentCreate, onCreateAgent: ShowAgentCreate, onEditAgent: ShowAgentEdit, onEditEquipment: ShowEquipmentEdit);
168         admin.Dock = DockStyle.Fill;
169         content.Controls.Add(admin);
170     }
171
172     /// <summary>
173     /// Affiche la vue des équipements disponibles.
174     /// </summary>
175     private void ShowEquipmentFree()
176     {
177         content.Controls.Clear();
178         content.Controls.Add(new FreeEquipmentView(onBack: ShowHome) { Dock = DockStyle.Fill });
179     }
180
181     /// <summary>
182     /// Affiche la vue principale d'inventaire (liste complète des équipements).
183     /// </summary>
184     private void ShowMainInventoryPage()
185     {
186         content.Controls.Clear();
187
188         content.Controls.Add(new MainInventoryView(onBack: ShowHome) { Dock = DockStyle.Fill });
189     }
190     
191     /// <summary>
192     /// Affiche la vue de création d'équipement.
193     /// Utilisée depuis le menu d'administration.
194     /// </summary>
195     private void ShowEquipmentCreate()
196     {
197         content.Controls.Clear();
198         content.Controls.Add(new EquipmentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
199     }
200
201     /// <summary>
202     /// Affiche la vue de création d'agent.
203     /// </summary>
204     private void ShowAgentCreate()
205     {
206         content.Controls.Clear();
207         content.Controls.Add(new AgentCreateView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
208     }
209     /// <summary>
210     /// Affiche la vue d'édition d'agent.
211     /// </summary>
212     private void ShowAgentEdit()
213     {
214         content.Controls.Clear();
215         content.Controls.Add(new AgentEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
216     }
217     /// <summary>
218     /// Affiche la vue d'édition d'équipement.
219     /// </summary>
220     private void ShowEquipmentEdit()
221     {
222         content.Controls.Clear();
223         content.Controls.Add(new EquipementEditView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
224     }
225 }
```

---

## 📦 Lignes 1-9 : Les imports

Tous les `using` importent des fonctionnalités nécessaires :

- **Lignes 1-3** : Bibliothèques de base Windows Forms (dessiner, contrôles UI, événements)
- **Lignes 4-8** : Toutes les autres vues de l'application
- **Ligne 9** : Intégration Windows Forms (peu utilisé ici)

💡 Ces imports permettent d'utiliser les autres écrans sans écrire le chemin complet à chaque fois.

---

## 🏷️ Ligne 11 : Le namespace

```csharp
11  namespace ProjetParc.Views;
```

Adresse : `ProjetParc.Views` - Le dossier des vues principales.

---

## 🏛️ Ligne 17 : La classe WelcomePage

```csharp
17  public class WelcomePage : Form
```

**`Form`** - Hérite de Form = C'est une fenêtre Windows

💡 **Important :** C'est la fenêtre principale affichée par `Program.cs` au démarrage.

---

## 📦 Lignes 19-23 : Les variables de classe

```csharp
19      private Panel content;
20      private Button btnSetEquipment;
21      private Button btnFreeEquipment;
22      private Button btnNewMod;
23      private Label title;
```

**Ligne 19 :** `content` - Le panneau qui change de contenu selon la navigation

**Lignes 20-22 :** Les 3 boutons principaux de la page d'accueil

**Ligne 23 :** `title` - Le label "Gestion de Parc" en haut

💡 **Pourquoi private ?** Ces contrôles ne sont utilisés que dans cette classe.

---

## 🎨 Lignes 29-128 : Le constructeur - Création de l'interface

### **Lignes 31-34 : Configuration de la fenêtre**

```csharp
31          Text = "Gestion Parc";
32          WindowState = FormWindowState.Maximized;
33          StartPosition = FormStartPosition.CenterScreen;
34          MinimumSize = new Size(800, 600);
```

- **Ligne 31 :** Titre de la fenêtre (barre supérieure)
- **Ligne 32 :** Fenêtre maximisée au démarrage
- **Ligne 33 :** Centrée sur l'écran
- **Ligne 34 :** Taille minimale 800x600 pixels

---

### **Lignes 37-48 : Layout principal**

```csharp
37          var mainLayout = new TableLayoutPanel
38          {
39              Dock = DockStyle.Fill,
40              BackColor = Color.White,
41              RowCount = 2,
42              ColumnCount = 1,
43              Padding = new Padding(20)
44          };
45
46          // Configuration des lignes
47          mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // En-tête
48          mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Contenu
```

**TableLayoutPanel** = Grille organisée en lignes et colonnes

**Configuration :**
- 2 lignes, 1 colonne
- Ligne 1 : Hauteur fixe de 120px (pour le titre)
- Ligne 2 : Prend tout l'espace restant (pour le contenu)

---

### **Lignes 51-65 : Création du titre**

```csharp
51          var headerPanel = new TableLayoutPanel 
58          title = new Label
59          {
60              Text = "Gestion de Parc",
61              Font = new Font("Segoe UI", 28f, FontStyle.Bold),
62              Dock = DockStyle.Fill,
63              TextAlign = ContentAlignment.MiddleCenter
64          };
```

- Police Segoe UI taille 28, en gras
- Centré au milieu (horizontal + vertical)

---

### **Lignes 68-83 : Panneau de contenu et layout des boutons**

```csharp
68          content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
71          var buttonLayout = new TableLayoutPanel
72          {
73              Dock = DockStyle.Fill,
74              ColumnCount = 3,
75              RowCount = 1,
76              Padding = new Padding(10)
77          };
```

- `content` est le panneau qui changera selon la navigation
- `buttonLayout` organise les 3 boutons en colonnes

**Lignes 80-83 :** Boucle pour créer 3 colonnes égales (33.33% chacune)

---

### **Lignes 86-112 : Création des 3 boutons**

```csharp
86          var tileFont = new Font("Segoe UI", 16f, FontStyle.Bold);
87          btnSetEquipment = new Button
88          {
89              Text = "Equipements en place",
90              Font = tileFont,
91              Dock = DockStyle.Fill,
92              Margin = new Padding(10)
93          };
```

Même pattern pour les 3 boutons :
1. **btnSetEquipment** - "Equipements en place"
2. **btnFreeEquipment** - "Equipements disponibles"
3. **btnNewMod** - "Modification / Création"

**`Dock = DockStyle.Fill`** - Le bouton remplit toute sa case

**`Margin = new Padding(10)`** - Espace de 10px autour

---

### **Lignes 114-117 : Liaison des événements**

```csharp
114         // Ajout des événements
115         btnNewMod.Click += (_, __) => ShowAdminMenu();
116         btnFreeEquipment.Click += (_, __) => ShowEquipmentFree();
117         btnSetEquipment.Click += (_, __) => ShowMainInventoryPage();
```

**Syntaxe lambda** : `(_, __) => Méthode()`

Quand on clique sur un bouton, appelle la méthode correspondante :
- `btnNewMod` → `ShowAdminMenu()`
- `btnFreeEquipment` → `ShowEquipmentFree()`
- `btnSetEquipment` → `ShowMainInventoryPage()`

💡 **`_` et `__`** = paramètres ignorés (sender et EventArgs non utilisés)

---

### **Lignes 119-127 : Assemblage final**

```csharp
119         // Assemblage final
120         content.Controls.Add(buttonLayout);
121         mainLayout.Controls.Add(headerPanel, 0, 0);
122         mainLayout.Controls.Add(content, 0, 1);
123         Controls.Add(mainLayout);
125         // Affiche le panneau d'accueil contenant les trois tuiles
126         ShowHome();
```

**Ordre d'imbrication :**
```
WelcomePage (Form)
└─ mainLayout (TableLayoutPanel)
   ├─ headerPanel [ligne 0]
   │  └─ title (Label)
   └─ content [ligne 1]
      └─ buttonLayout (TableLayoutPanel)
         ├─ btnSetEquipment [colonne 0]
         ├─ btnFreeEquipment [colonne 1]
         └─ btnNewMod [colonne 2]
```

**Ligne 126 :** Appel de `ShowHome()` pour finaliser l'affichage

---

## 🏠 Lignes 134-157 : ShowHome() - Afficher l'accueil

```csharp
134     private void ShowHome()
135     {
136         content.Controls.Clear();
138         var buttonLayout = new TableLayoutPanel
143             Padding = new Padding(50)
152         buttonLayout.Controls.Add(btnSetEquipment, 0, 0);
153         buttonLayout.Controls.Add(btnFreeEquipment, 1, 0);
154         buttonLayout.Controls.Add(btnNewMod, 2, 0);
156         content.Controls.Add(buttonLayout);
157     }
```

**Ligne 136 :** Vide le panneau `content`

**Lignes 138-144 :** Recrée le layout des boutons

**Lignes 152-154 :** Réajoute les 3 boutons

💡 **Pourquoi recréer ?** Pour revenir proprement à l'accueil après avoir navigué ailleurs.

---

## 🔧 Lignes 163-170 : ShowAdminMenu() - Menu d'administration

```csharp
163     private void ShowAdminMenu()
164     {
165         content.Controls.Clear();
167         var admin = new AdminMenuView(onBack: ShowHome, onCreateEquipment: ShowEquipmentCreate, onCreateAgent: ShowAgentCreate, onEditAgent: ShowAgentEdit, onEditEquipment: ShowEquipmentEdit);
168         admin.Dock = DockStyle.Fill;
169         content.Controls.Add(admin);
170     }
```

**Ligne 167 :** Crée une instance d'`AdminMenuView` avec 5 callbacks :
- `onBack` → `ShowHome` (revenir à l'accueil)
- `onCreateEquipment` → `ShowEquipmentCreate`
- `onCreateAgent` → `ShowAgentCreate`
- `onEditAgent` → `ShowAgentEdit`
- `onEditEquipment` → `ShowEquipmentEdit`

💡 **Pattern callback :** AdminMenuView appelle ces méthodes quand l'utilisateur clique sur ses boutons.

---

## 📦 Lignes 175-224 : Les autres méthodes Show*

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

## 🎬 Scénario de navigation

```
1. Démarrage
   → WelcomePage affiche 3 boutons

2. Clic sur "Modification / Création"
   → ShowAdminMenu() remplace content par AdminMenuView

3. Dans AdminMenuView, clic sur "Création Agent"
   → ShowAgentCreate() remplace content par AgentCreateView

4. Dans AgentCreateView, clic sur "← Retour"
   → ShowAdminMenu() réaffiche AdminMenuView

5. Dans AdminMenuView, clic sur "← Retour"
   → ShowHome() réaffiche les 3 boutons d'accueil
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

---

## 💡 Questions fréquentes

**Q : Pourquoi content est un Panel et pas directement la Form ?**
- R : Pour pouvoir changer facilement le contenu sans toucher au titre et au layout principal.

**Q : Que se passe-t-il si on clique plusieurs fois sur un bouton ?**
- R : `Clear()` supprime l'ancienne vue, puis on ajoute la nouvelle. Pas de doublon.

**Q : Pourquoi les méthodes Show* sont private ?**
- R : Elles ne sont utilisées que dans cette classe, pas besoin de les exposer.

**Q : Comment ajouter un nouveau bouton sur la page d'accueil ?**
- R : 1) Déclarer le bouton (ligne 19-23), 2) Le créer dans le constructeur, 3) L'ajouter au buttonLayout, 4) Créer une méthode Show*, 5) Lier l'événement Click.

---

## 🔗 Fichiers liés

- **Program.cs** - Lance cette page au démarrage
- **AdminMenuView.cs** - Menu d'administration
- **FreeEquipmentView.cs** - Équipements disponibles
- **MainInventoryView.cs** - Inventaire complet
- **AgentCreateView.cs**, **AgentEditView.cs** - Gestion des agents
- **EquipmentCreateView.cs**, **EquipmentEditView.cs** - Gestion des équipements

---

**📌 Prochaine étape :** Consulter `AdminMenuView.cs` pour voir comment fonctionnent les sous-menus.
