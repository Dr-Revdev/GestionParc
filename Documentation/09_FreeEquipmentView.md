# 📘 Documentation de FreeEquipmentView.cs

## 🎯 But de ce fichier
Vue affichant **deux listes d'équipements** : disponibles et rendus DSEM. Permet de basculer un équipement entre ces états et d'afficher ses détails.

💡 **Analogie :** C'est comme un tableau de gestion des stocks avec deux colonnes : "En stock" et "Rendu au dépôt".

---

## 📋 Structure principale

Fichier de **~630 lignes** avec interface en 3 colonnes :
- **Colonne gauche** : Équipements disponibles (etat_pret = 0) - **ListView triable**
- **Colonne milieu** : Équipements rendus DSEM (etat_pret = 2) - **ListView triable**
- **Colonne droite** : Détails de l'équipement sélectionné

**Nouveau** : Les listes utilisent maintenant `ListView` au lieu de `ListBox` avec colonnes triables.

---

## 🎨 Interface utilisateur

### Layout en 3 colonnes égales

```
┌──────────────────────────────────────────────────────────────────┐
│ [← Retour]  Gestion des équipements                              │
├──────────────────┬──────────────────┬──────────────────────────┤
│ Disponible       │ Rendu DSEM       │ Détails                  │
│ [🔍____] [⌕]     │ [🔍____] [⌕]     │                          │
│ ┌────────────────┐ ┌────────────────┐                          │
│ │Type│Code│N°│Nom│ │Type│Code│N°│Nom│ Type: [_________]       │
│ ├────┼────┼──┼───┤ ├────┼────┼──┼───┤ Nom: [_________]        │
│ │PC  │001 │..│..│◄┼─│Écrn│05 │..│..│ Code: [_________]       │
│ │Sour│02  │..│..│ │ │Clav│HP │..│..│ N° série: [____]        │
│ │Écrn│03  │..│..│ │ │... │   │  │  │ Marque: [_______]       │
│ └────┴────┴──┴───┘ └────┴────┴──┴───┘                          │
│                                       ☑ Rendre DSEM             │
│ ⬆️ Cliquer colonne pour trier         Commentaire:             │
│                                       [______________]          │
└──────────────────┴──────────────────┴──────────────────────────┘
```

**Colonnes triables** :
- Type
- Code Parc
- N° Série
- Nom

Cliquer sur une colonne pour trier, re-cliquer pour inverser l'ordre.

---

## 🔑 Fonctionnalités principales

### 1. **Deux listes séparées**
- **Disponible** : `etat_pret = 0` (prêt à être attribué)
- **Rendu DSEM** : `etat_pret = 2` (retourné, marqué spécialement)

### 2. **Recherche indépendante**
Chaque liste a sa propre barre de recherche avec filtre.

### 3. **Affichage des détails**
Tous les champs en lecture seule + checkbox "Rendre DSEM".

### 4. **Bascule d'état**
Cocher/décocher "Rendre DSEM" :
- ☑ Checked → `etat_pret = 2` (va dans "Rendu DSEM")
- ☐ Unchecked → `etat_pret = 0` (va dans "Disponible")

---

## 💾 Base de données

### **États des équipements (etat_pret)**
- `0` = Disponible (libre)
- `1` = Prêté à un agent
- `2` = Rendu DSEM (retourné mais marqué)

### **LoadAvailable()** - Liste disponibles
```sql
SELECT e.id_equipement, 
       t.name AS type,
       TRIM(COALESCE(e.code_parc, '-')) AS code,
       TRIM(COALESCE(e.numero_serie, '-')) AS serial,
       COALESCE(TRIM(e.nom), '(sans nom)') AS nom
FROM "Equipements" e
JOIN equipment_type t ON t.id = e.type_id
WHERE COALESCE(e.etat_pret,0) = 0
ORDER BY type, code, serial;
```
**Nouveau** : Retourne les colonnes séparées pour le ListView.

### **LoadReturned()** - Liste rendus
```sql
SELECT e.id_equipement, 
       t.name AS type,
       TRIM(COALESCE(e.code_parc, '-')) AS code,
       TRIM(COALESCE(e.numero_serie, '-')) AS serial,
       COALESCE(TRIM(e.nom), '(sans nom)') AS nom
FROM "Equipements" e
JOIN equipment_type t ON t.id = e.type_id
WHERE COALESCE(e.etat_pret,0) = 2
ORDER BY type, code, serial;
```
**Nouveau** : Retourne les colonnes séparées pour le ListView.

### **LoadDetails()** - Détails équipement
```sql
SELECT e.type_id, t.name, e.nom, e.code_parc, 
       e.numero_serie, e.marque, e.commentaire, 
       COALESCE(e.etat_pret,0)
FROM "Equipements" e
JOIN equipment_type t ON t.id = e.type_id
WHERE e.id_equipement = $id;
```

### **UpdateRenduDsem()** - Bascule état
```sql
UPDATE "Equipements" 
SET etat_pret = $v 
WHERE id_equipement = $id;
```
Avec `$v = 2` si checked, `$v = 0` si unchecked.

---

## 🎬 Scénario d'utilisation

### **Marquer un équipement comme "Rendu DSEM"**
```
1. Vue ouverte → LoadAvailable() et LoadReturned()
   ↓
2. Listes remplies
   ↓
3. Utilisateur cherche "Dell" dans "Disponible"
   ↓
4. Clic 🔍 → LoadAvailable("Dell")
   ↓
5. Sélection "PC Dell [001]"
   ↓
6. LbAvailable_Selected → LoadDetails(id)
   ↓
7. Détails affichés à droite
   ↓
8. cbxRenduDsem.Checked = false (car etat_pret = 0)
   ↓
9. Utilisateur coche "Rendre DSEM"
   ↓
10. CbxRenduDsem_CheckedChanged → UpdateRenduDsem()
   ↓
11. UPDATE etat_pret = 2
   ↓
12. LoadAvailable() et LoadReturned() refresh
   ↓
13. L'équipement disparaît de "Disponible"
   ↓
14. L'équipement apparaît dans "Rendu DSEM"
```

---

## 🎓 Concepts techniques

### **1. ListView avec tri par colonnes**
Utilise `ListViewColumnSorter` pour le tri :
```csharp
lvAvailableSorter = new ListViewColumnSorter();
lvAvailable.ListViewItemSorter = lvAvailableSorter;
lvAvailable.ColumnClick += (s, e) => {
    lvAvailableSorter.SetSortColumn(e.Column);
    lvAvailable.Sort();
};
```

### **2. Remplissage du ListView**
```csharp
lvAvailable.Items.Clear();
var item = new ListViewItem(type);
item.SubItems.AddRange(new[] { code, serial, nom });
item.Tag = id; // Stocke l'ID pour récupération
lvAvailable.Items.Add(item);
```

### **3. Sélection croisée**
Quand on sélectionne dans une liste, désélectionner l'autre :
```csharp
lvAvailable.Enter += (_, __) => lvReturned.SelectedItems.Clear();
lvReturned.Enter += (_, __) => lvAvailable.SelectedItems.Clear();
```

### **4. Événement CheckedChanged**
Détaché temporairement pour éviter les boucles :
```csharp
cbxRenduDsem.CheckedChanged -= CbxRenduDsem_CheckedChanged;
cbxRenduDsem.Checked = r.GetInt32(7) != 0;
cbxRenduDsem.CheckedChanged += CbxRenduDsem_CheckedChanged;
```

### **5. Tag pour stocker l'ID**
```csharp
cbxRenduDsem.Tag = equipmentId;
// Plus tard:
if (cbxRenduDsem.Tag is not string id) return;
```

### **6. Champs en lecture seule**
```csharp
tbType = new TextBox { ReadOnly = true }
```

---

## 💡 Points importants

**Rafraîchissement après modification :**
```csharp
LoadAvailable(tbSearchAvailable.Text);
LoadReturned(tbSearchReturned.Text);
```
Les deux listes sont rafraîchies pour refléter le changement.

**Format des labels :**
```csharp
var label = string.IsNullOrEmpty(c) 
    ? $"{n} | {ty}" 
    : $"{n} | {c} | {ty}";
```
Code parc affiché seulement s'il existe.

**Recherche persistante :**
Le texte de recherche est conservé lors des rafraîchissements.

**Bordures et grille :**
```csharp
BorderStyle = BorderStyle.FixedSingle,
GridLines = true
```
Affichage professionnel avec bordures noires et lignes de séparation.

---

## ⚠️ Attention

**État "Prêté" (1) non affiché :**
Cette vue ne montre que les états 0 et 2.
Les équipements prêtés (état 1) sont gérés dans `MainInventoryView`.

**Pas de modification des détails :**
Tous les champs sont en lecture seule.
Seul le checkbox "Rendre DSEM" est interactif.

---

## 🔗 Fichiers liés

- **WelcomePage.cs** - Appelle cette vue (bouton "Equipements disponibles")
- **EquipmentEditView.cs** - Pour modification complète
- **MainInventoryView.cs** - Pour voir les prêts
- **ListViewColumnSorter.cs** - Gestion du tri des colonnes
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `equipment_type`

---

**📌 Prochaine étape :** Consulter `LoanCreationView.cs` pour la création de prêts.
