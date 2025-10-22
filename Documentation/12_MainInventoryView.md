# 📘 Documentation de MainInventoryView.cs

## 🎯 But de ce fichier
Vue **principale de l'inventaire** affichant tous les prêts en cours dans un tableau. Permet de créer, modifier des prêts et afficher des diagnostics.

💡 **Analogie :** C'est comme un tableau de bord RH qui liste tous les équipements prêtés à chaque employé.

---

## 📋 Structure principale

Fichier de **311 lignes** avec interface en 2 parties :
- **Haut** : Tableau des prêts (ListView avec colonnes dynamiques)
- **Bas** : Boutons d'action (Nouveau prêt, Diagnostic)

---

## 🎨 Interface utilisateur

### Layout vertical

```
┌────────────────────────────────────────────────────┐
│ [← Retour]  Equipements en place                   │
├────────────────────────────────────────────────────┤
│ TABLEAU DES PRÊTS                                  │
│ ┌──────────┬────────────────┬────────────────┬───┐ │
│ │ Agent    │ Équipement 1   │ Équipement 2   │...│ │
│ ├──────────┼────────────────┼────────────────┼───┤ │
│ │Dupont J. │PC Dell [PC-001]│Écran LG [M-05] │   │ │
│ │Martin S. │Souris MS [03]  │                │   │ │
│ │Durand P. │PC HP [PC-010]  │Écran SA [M-08] │..│ │
│ └──────────┴────────────────┴────────────────┴───┘ │
│                                                    │
│                     [Diag DB] [Nouveau prêt]      │
└────────────────────────────────────────────────────┘
```

**Double-clic sur une ligne → Ouvre l'éditeur de prêt**

---

## 🔑 Fonctionnalités principales

### 1. **Tableau dynamique**
- Colonnes créées selon le nombre max d'équipements
- 1 colonne "Agent" + N colonnes "Équipement X"
- ListView en mode Details

### 2. **Création de prêt**
Bouton "Nouveau prêt" → Ouvre `LoanCreationView`.

### 3. **Modification de prêt**
Double-clic sur un agent → Ouvre `LoanCreationView` en mode édition.

### 4. **Diagnostic DB**
Bouton "Diag DB" → Affiche statistiques :
- Total d'équipements
- Disponibles (etat_pret = 0)
- Prêtés (etat_pret = 1)
- DSEM (etat_pret = 2)

---

## 💾 Base de données

### **LoadLoans()** - Chargement des prêts

#### **Étape 1 : Compter le max d'équipements par agent**
```sql
SELECT COALESCE(MAX(equipment_count), 0)
FROM (
    SELECT COUNT(*) as equipment_count
    FROM Equipements e
    WHERE e.etat_pret = 1 AND e.idrh IS NOT NULL
    GROUP BY e.idrh
)
```

#### **Étape 2 : Récupérer les prêts**
```sql
SELECT 
    a.idrh,
    a.nom || ' ' || a.prenom as agent_name,
    GROUP_CONCAT(t.name || ' - ' || e.nom || ' (' || e.code_parc || ')', '||') as equipments
FROM Equipements e
JOIN Agents a ON a.idrh = e.idrh
JOIN equipment_type t ON t.id = e.type_id
WHERE e.etat_pret = 1
GROUP BY a.idrh, a.nom, a.prenom
ORDER BY a.nom, a.prenom
```

**GROUP_CONCAT** agrège tous les équipements d'un agent séparés par `||`.

#### **Étape 3 : Remplissage du ListView**
```csharp
var equipments = reader.GetString(2).Split(new[] { "||" }, ...);
foreach (var eq in equipments)
{
    item.SubItems.Add(eq.Trim());
}
```

### **ShowDbDiagnostic()** - Statistiques
```sql
SELECT
  SUM(CASE WHEN etat_pret = 0 THEN 1 ELSE 0 END) as available,
  SUM(CASE WHEN etat_pret = 1 THEN 1 ELSE 0 END) as loaned,
  SUM(CASE WHEN etat_pret = 2 THEN 1 ELSE 0 END) as dsem,
  COUNT(*) as total
FROM Equipements
```

---

## 🎬 Scénarios d'utilisation

### **Affichage initial**
```
1. WelcomePage : Clic "Equipements en place"
   ↓
2. new MainInventoryView(ShowHome)
   ↓
3. InitializeComponent() crée l'interface
   ↓
4. LoadLoans() appelé
   ↓
5. Étape 1 : Compte max équipements par agent → 3
   ↓
6. Création des colonnes :
    - Agent
    - Équipement 1
    - Équipement 2
    - Équipement 3
   ↓
7. Étape 2 : Requête avec GROUP_CONCAT
   ↓
8. Pour chaque agent :
    - Crée ListViewItem
    - Split équipements par "||"
    - Ajoute chaque équipement dans une SubItem
   ↓
9. ListView rempli
```

### **Création d'un nouveau prêt**
```
1. Clic "Nouveau prêt"
   ↓
2. ShowLoanCreationDialog()
   ↓
3. new LoanCreationView() en modal
   ↓
4. dialog.ShowDialog()
   ↓
5. Utilisateur crée le prêt
   ↓
6. DialogResult = OK
   ↓
7. LoadLoans() refresh
   ↓
8. Nouvelle ligne apparaît dans le tableau
```

### **Modification d'un prêt existant**
```
1. Double-clic sur "Dupont Jean"
   ↓
2. OnLoanDoubleClick déclenché
   ↓
3. Récupération agentId depuis item.Tag
   ↓
4. OpenLoanEditor(agentId)
   ↓
5. new LoanCreationView { SelectedAgentId = agentId }
   ↓
6. Modal affiché avec équipements pré-chargés
   ↓
7. Utilisateur modifie les équipements
   ↓
8. DialogResult = OK
   ↓
9. LoadLoans() refresh
   ↓
10. Ligne mise à jour
```

### **Diagnostic DB**
```
1. Clic "Diag DB"
   ↓
2. ShowDbDiagnostic()
   ↓
3. Requête avec SUM et CASE
   ↓
4. MessageBox affiche :
    "Equipements: total=150
     Disponible=80
     Prêt=60
     DSEM=10"
```

---

## 🎓 Concepts techniques

### **1. Colonnes dynamiques**
Nombre de colonnes = max d'équipements par agent + 1.
```csharp
for (int i = 1; i <= maxEquipments; i++)
{
    lvEquipments.Columns.Add(new ColumnHeader { 
        Text = $"Équipement {i}", Width = 250 
    });
}
```

### **2. GROUP_CONCAT en SQLite**
Agrège plusieurs valeurs en une chaîne :
```sql
GROUP_CONCAT(expression, separator)
```
Résultat : `"PC Dell||Écran LG||Souris MS"`

### **3. Tag pour stocker l'ID**
```csharp
var item = new ListViewItem(agentName) { Tag = agentId };
// Plus tard:
if (lvEquipments.SelectedItems[0].Tag is string agentId)
```

### **4. DoubleClick event**
```csharp
lvEquipments.DoubleClick -= OnLoanDoubleClick;
lvEquipments.DoubleClick += OnLoanDoubleClick;
```
Évite les doublons d'événements.

### **5. Gestion du cas "Aucun prêt"**
```csharp
if (lvEquipments.Items.Count == 0)
{
    lvEquipments.Columns.Clear();
    lvEquipments.Columns.Add(new ColumnHeader { Text = "État", Width = 200 });
    var item = new ListViewItem("Aucun prêt en cours") { 
        ForeColor = Color.Gray 
    };
    lvEquipments.Items.Add(item);
}
```

---

## 💡 Points importants

**Colonnes vides :**
```csharp
while (item.SubItems.Count < lvEquipments.Columns.Count)
{
    item.SubItems.Add(string.Empty);
}
```
Remplit les colonnes manquantes pour garder l'alignement.

**Refresh après modification :**
```csharp
if (dialog.ShowDialog() == DialogResult.OK)
{
    LoadLoans();
}
```

**SuspendLayout/ResumeLayout :**
Pas utilisé ici mais recommandé pour les gros changements UI.

---

## ⚠️ Attention

**GROUP_CONCAT limité :**
Si un agent a 50 équipements, la chaîne peut être très longue.
Pas de problème en pratique pour cet usage.

**DoubleClick sur ligne vide :**
Vérifie `lvEquipments.SelectedItems.Count > 0` avant.

**Variables non utilisées :**
```csharp
private FlowLayoutPanel pnlLoans;  // Non utilisé
private Label lblLoansTitle;       // Non utilisé
```
Probablement prévues pour une évolution future.

---

## 🔗 Fichiers liés

- **WelcomePage.cs** - Appelle cette vue (bouton "Equipements en place")
- **LoanCreationView.cs** - Création/modification de prêt
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `Agents`, `equipment_type`

---

## 📊 Exemple de résultat GROUP_CONCAT

**Données en base :**
```
Agent: Dupont Jean (IDRH123)
- PC Dell [PC-001]
- Écran LG [MON-005]
- Souris Logitech [MOU-012]
```

**Résultat GROUP_CONCAT :**
```
"Ordinateur - PC Dell (PC-001)||Écran - Écran LG (MON-005)||Souris - Souris Logitech (MOU-012)"
```

**Après Split et affichage :**
```
Agent        | Équipement 1             | Équipement 2           | Équipement 3
-------------|--------------------------|------------------------|---------------------------
Dupont Jean  | Ordinateur - PC Dell ... | Écran - Écran LG ...   | Souris - Souris Logi...
```

---

**📌 Félicitations !** Tu as maintenant la documentation complète de tous les fichiers du projet ProjetParc ! 🎉

Pour naviguer, retourne au [README.md](README.md) et explore les fichiers qui t'intéressent.
