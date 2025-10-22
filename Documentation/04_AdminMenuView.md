# 📘 Documentation de AdminMenuView.cs

## 🎯 But de ce fichier
Menu d'administration qui affiche **5 boutons** pour accéder aux fonctions de création et modification des agents et équipements.

💡 **Analogie :** C'est comme le bureau d'un administrateur avec des boutons pour toutes les tâches de gestion.

---

## 📋 Le code complet (lignes principales)

```csharp
1   using System;
2   using System.Drawing;
3   using System.Windows.Forms;
4
5   namespace ProjetParc.Views.Admin;
6       /// <summary>
7       /// Vue du menu d'administration permettant d'accéder aux fonctionnalités de création et modification
8       /// des équipements et des agents
9       /// </summary>
10      public class AdminMenuView : UserControl
11      {
12          private readonly Action _onBack;
13          private readonly Action _onCreateEquipment;
14          private readonly Action _onCreateAgent;
15          private readonly Action _onEditAgent;
16          private readonly Action _onEditEquipment;
17          private Button btnCreateEquipment;
18          private Button btnCreateAgent;
19          private Button btnModificationEquipment;
20          private Button btnModificationAgent;
21          private Button btnExange;
22
23          /// <param name="onBack">Action à exécuter pour revenir à la vue précédente</param>
24          /// <param name="onCreateEquipment">Action à exécuter pour créer un équipement</param>
25          /// <param name="onCreateAgent">Action à exécuter pour créer un agent</param>
26          /// <param name="onEditAgent">Action à exécuter pour modifier un agent</param>
27          /// <param name="onEditEquipment">Action à exécuter pour modifier un équipement</param>
28          public AdminMenuView(Action onBack, Action onCreateEquipment, Action onCreateAgent, Action onEditAgent, Action onEditEquipment)
29          {
30              _onBack = onBack;
31              _onCreateEquipment = onCreateEquipment;
32              _onCreateAgent = onCreateAgent;
33              _onEditAgent = onEditAgent;
34              _onEditEquipment = onEditEquipment;
35
36              // Configuration de base
37              Dock = DockStyle.Fill;
38              Padding = new Padding(20);
39
40              // Layout principal
41              var mainLayout = new TableLayoutPanel
42              {
43                  Dock = DockStyle.Fill,
44                  RowCount = 3,
45                  ColumnCount = 1
46              };
47
48              // Configuration des lignes du layout principal
49              mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // En-tête
50              mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70)); // Zone des boutons principaux
51              mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Zone du bouton échange
52
53              // En-tête
54              var headerPanel = new TableLayoutPanel
55              {
56                  Dock = DockStyle.Fill,
57                  ColumnCount = 2,
58                  RowCount = 1,
59                  Margin = new Padding(0, 0, 0, 20)
60              };
61              headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // Bouton retour
62              headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Titre
63
64              var btnBack = new Button 
65              { 
66                  Text = "← Retour",
67                  Height = 36,
68                  Width = 120,
69                  Dock = DockStyle.Left
70              };
71              btnBack.Click += (_, __) => _onBack?.Invoke();
72
73              var title = new Label 
74              { 
75                  Text = "Menu modification / création",
76                  Font = new Font("Segoe UI", 14, FontStyle.Bold),
77                  Dock = DockStyle.Fill,
78                  TextAlign = ContentAlignment.MiddleLeft,
79                  Padding = new Padding(10, 0, 0, 0)
80              };
81
82              headerPanel.Controls.Add(btnBack, 0, 0);
83              headerPanel.Controls.Add(title, 1, 0);
84
85              // Zone des boutons principaux
86              var buttonLayout = new TableLayoutPanel
87              {
88                  Dock = DockStyle.Fill,
89                  RowCount = 2,
90                  ColumnCount = 2,
91                  Margin = new Padding(0)
92              };
93
94              // Configuration des lignes et colonnes pour les boutons principaux
95              for (int i = 0; i < 2; i++)
96              {
97                  buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
98                  buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
99              }
100
101             var tileFont = new Font("Segoe UI", 14f, FontStyle.Bold);
102
103             // Création des boutons principaux
104             btnCreateEquipment = CreateMenuButton("Création Equipement", tileFont);
105             btnCreateAgent = CreateMenuButton("Création Agent", tileFont);
106             btnModificationEquipment = CreateMenuButton("Modification Equipement", tileFont);
107             btnModificationAgent = CreateMenuButton("Modification agent", tileFont);
108             btnExange = CreateMenuButton("Echange", tileFont);
109
110             // Ajout des boutons au layout avec espacement
111             buttonLayout.Controls.Add(btnCreateEquipment, 0, 0);
112             buttonLayout.Controls.Add(btnCreateAgent, 1, 0);
113             buttonLayout.Controls.Add(btnModificationEquipment, 0, 1);
114             buttonLayout.Controls.Add(btnModificationAgent, 1, 1);
115
116             // Zone du bouton échange
117             var exchangePanel = new Panel { Dock = DockStyle.Fill };
118             btnExange.Dock = DockStyle.None;
119             btnExange.Anchor = AnchorStyles.None;
120             btnExange.Width = 280;
121             btnExange.Height = 80;
122             exchangePanel.Controls.Add(btnExange);
123             
124             // Centrer le bouton échange
125             btnExange.Location = new Point(
126                 (exchangePanel.ClientSize.Width - btnExange.Width) / 2,
127                 (exchangePanel.ClientSize.Height - btnExange.Height) / 2
128             );
129
130             // Assemblage final
131             mainLayout.Controls.Add(headerPanel, 0, 0);
132             mainLayout.Controls.Add(buttonLayout, 0, 1);
133             mainLayout.Controls.Add(exchangePanel, 0, 2);
134
135             Controls.Add(mainLayout);
136
137             // Ajout des gestionnaires d'événements
138             btnCreateEquipment.Click += (_, __) => _onCreateEquipment();
139             btnCreateAgent.Click += (_, __) => _onCreateAgent();
140             btnModificationAgent.Click += (_, __) => _onEditAgent();
141             btnModificationEquipment.Click += (_, __) => _onEditEquipment();
142         }
143
144         private static Button CreateMenuButton(string text, Font font)
145         {
146             return new Button
147             {
148                 Text = text,
149                 Font = font,
150                 Dock = DockStyle.Fill,
151                 Margin = new Padding(20),
152                 AutoSize = false
153             };
154
155         }
156     }
```

---

## 🧩 Structure et concepts clés

### **Lignes 10-21 : Variables de classe**

5 **Actions** (callbacks) pour la navigation :
- `_onBack` - Retour au menu précédent
- `_onCreateEquipment` - Création d'équipement
- `_onCreateAgent` - Création d'agent
- `_onEditAgent` - Modification d'agent
- `_onEditEquipment` - Modification d'équipement

5 **Boutons** pour les actions :
- `btnCreateEquipment`, `btnCreateAgent`
- `btnModificationEquipment`, `btnModificationAgent`
- `btnExange` (échange - bouton distinct en bas)

---

### **Lignes 28-34 : Le constructeur**

Reçoit **5 callbacks** en paramètre et les stocke dans les variables privées.

💡 **Pattern callback :** WelcomePage passe ses méthodes `Show*()` en paramètre.

---

### **Lignes 41-51 : Layout en 3 zones**

```
┌─────────────────────────────────┐
│  En-tête (45px fixe)            │ ← Bouton retour + titre
├─────────────────────────────────┤
│                                 │
│  Boutons principaux (70%)       │ ← Grille 2x2 des boutons
│                                 │
├─────────────────────────────────┤
│  Bouton échange (30%)           │ ← Bouton centré seul
└─────────────────────────────────┘
```

**Pourcentages :**
- En-tête : hauteur absolue 45px
- Boutons : 70% de l'espace restant
- Échange : 30% de l'espace restant

---

### **Lignes 54-83 : En-tête (retour + titre)**

**Structure :**
- 2 colonnes : Bouton retour (140px) + Titre (reste)
- Bouton "← Retour" appelle `_onBack?.Invoke()`
- `?.` = appel sécurisé (si _onBack n'est pas null)

---

### **Lignes 86-114 : Grille 2x2 des boutons**

**Disposition :**
```
┌──────────────────┬──────────────────┐
│ Création         │ Création         │
│ Equipement       │ Agent            │
├──────────────────┼──────────────────┤
│ Modification     │ Modification     │
│ Equipement       │ Agent            │
└──────────────────┴──────────────────┘
```

**Lignes 95-99 :** Boucle pour créer 2 lignes et 2 colonnes de 50% chacune

**Ligne 101 :** Police commune pour tous les boutons (Segoe UI, 14pt, gras)

**Lignes 104-108 :** Création des 5 boutons via `CreateMenuButton()`

**Lignes 111-114 :** Ajout dans la grille (col, row)

---

### **Lignes 117-128 : Bouton échange centré**

**Technique de centrage :**
```csharp
125             btnExange.Location = new Point(
126                 (exchangePanel.ClientSize.Width - btnExange.Width) / 2,
127                 (exchangePanel.ClientSize.Height - btnExange.Height) / 2
128             );
```

**Formule :** `Position = (Taille_Panneau - Taille_Bouton) / 2`

💡 Le bouton fait 280x80px et est positionné au centre mathématique.

---

### **Lignes 137-141 : Liaison des événements**

```csharp
138             btnCreateEquipment.Click += (_, __) => _onCreateEquipment();
139             btnCreateAgent.Click += (_, __) => _onCreateAgent();
140             btnModificationAgent.Click += (_, __) => _onEditAgent();
141             btnModificationEquipment.Click += (_, __) => _onEditEquipment();
```

Chaque bouton appelle son callback correspondant.

⚠️ **Attention :** Le bouton "Echange" n'a pas d'événement (fonctionnalité future ?).

---

### **Lignes 144-155 : CreateMenuButton() - Méthode helper**

```csharp
144         private static Button CreateMenuButton(string text, Font font)
145         {
146             return new Button
147             {
148                 Text = text,
149                 Font = font,
150                 Dock = DockStyle.Fill,
151                 Margin = new Padding(20),
152                 AutoSize = false
153             };
154         }
```

**Méthode static** = Pas besoin d'instance pour l'appeler

**Avantage :** Évite la répétition du code de création des boutons

**Paramètres communs :**
- `Dock = Fill` - Remplit toute la cellule
- `Margin = 20` - Espacement de 20px autour
- `AutoSize = false` - Taille manuelle

---

## 🎬 Scénario d'utilisation

```
1. WelcomePage : Clic sur "Modification / Création"
   ↓
2. WelcomePage.ShowAdminMenu() crée AdminMenuView
   - Passe ShowHome en paramètre onBack
   - Passe ShowAgentCreate en paramètre onCreateAgent
   - etc.
   ↓
3. AdminMenuView affiche 5 boutons
   ↓
4. Utilisateur clique sur "Création Agent"
   ↓
5. btnCreateAgent.Click appelle _onCreateAgent()
   ↓
6. _onCreateAgent() pointe vers ShowAgentCreate()
   ↓
7. WelcomePage.ShowAgentCreate() remplace content
   ↓
8. AgentCreateView s'affiche
```

---

## 🎓 Concepts clés

**1. UserControl au lieu de Form**
- AdminMenuView hérite de `UserControl` (pas `Form`)
- S'intègre dans un panneau parent
- Pas de fenêtre séparée

**2. Pattern callback avancé**
- 5 callbacks différents
- Flexibilité totale de navigation
- Découplage : AdminMenuView ne connaît pas WelcomePage

**3. Layout responsive**
- Pourcentages pour adaptation
- Boutons remplissent les cellules
- Centrage mathématique du bouton échange

**4. Factorisation du code**
- `CreateMenuButton()` évite la répétition
- Tous les boutons ont le même style
- Facilite les modifications futures

---

## 💡 Questions fréquentes

**Q : Pourquoi le bouton "Echange" est séparé ?**
- R : Design choix - le mettre en évidence en bas, probablement pour une fonctionnalité importante future.

**Q : Que fait le bouton "Echange" ?**
- R : Rien pour l'instant, pas d'événement Click attaché. Fonctionnalité à venir.

**Q : Pourquoi `_onBack?.Invoke()` avec `?` ?**
- R : Sécurité - si _onBack est null, ne plante pas. Meilleure pratique C#.

**Q : Comment ajouter un nouveau bouton ?**
- R : 1) Ajouter un Action paramètre, 2) Créer le bouton avec CreateMenuButton(), 3) L'ajouter au layout, 4) Lier l'événement Click.

---

## 🔗 Fichiers liés

- **WelcomePage.cs** - Appelle cette vue avec les callbacks
- **AgentCreateView.cs** - Création d'agent
- **AgentEditView.cs** - Modification d'agent
- **EquipmentCreateView.cs** - Création d'équipement
- **EquipmentEditView.cs** - Modification d'équipement

---

**📌 Prochaine étape :** Consulter `AgentCreateView.cs` pour voir les formulaires de création.
