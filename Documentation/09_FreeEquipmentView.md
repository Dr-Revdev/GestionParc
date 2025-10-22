# 📘 Documentation de FreeEquipmentView.cs

## 🎯 But de ce fichier
Vue affichant **deux listes d'équipements** : disponibles et rendus DSEM. Permet de basculer un équipement entre ces états et d'afficher ses détails.

💡 **Analogie :** C'est comme un tableau de gestion des stocks avec deux colonnes : "En stock" et "Rendu au dépôt".

---

## 📋 Structure principale

Fichier de **327 lignes** avec interface en 3 colonnes :
- **Colonne gauche** : Équipements disponibles (etat_pret = 0)
- **Colonne milieu** : Équipements rendus DSEM (etat_pret = 2)
- **Colonne droite** : Détails de l'équipement sélectionné

---

## 🎨 Interface utilisateur

### Layout en 3 colonnes égales

```
┌──────────────────────────────────────────────────────────┐
│ [← Retour]  Gestion des équipements                      │
├──────────────────┬──────────────────┬────────────────────┤
│ Disponible       │ Rendu DSEM       │ Détails            │
│ [🔍____] [⌕]     │ [🔍____] [⌕]     │                    │
│ ┌──────────────┐ │ ┌──────────────┐ │ Type: [_________] │
│ │PC Dell [001] │ │ │Écran LG [05] │ │ Nom: [_________]  │
│ │Souris MS [02]│ │ │Clavier HP..  │ │ Code: [_________] │
│ │Écran SA [03] │◄┼─┼─────────────►│ │ N° série: [____]  │
│ │...           │ │ │              │ │ Marque: [_______] │
│ └──────────────┘ │ └──────────────┘ │                   │
│                  │                  │ ☑ Rendre DSEM     │
│                  │                  │ Commentaire:      │
│                  │                  │ [______________]  │
└──────────────────┴──────────────────┴────────────────────┘
```

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
SELECT e.id_equipement, e.nom, e.code_parc, t.name
FROM "Equipements" e
JOIN equipment_type t ON t.id = e.type_id
WHERE COALESCE(e.etat_pret,0) = 0
ORDER BY n, c;
```

### **LoadReturned()** - Liste rendus
```sql
SELECT e.id_equipement, e.nom, e.code_parc, t.name
FROM "Equipements" e
JOIN equipment_type t ON t.id = e.type_id
WHERE COALESCE(e.etat_pret,0) = 2
ORDER BY n, c;
```

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

### **1. Désélection croisée**
Quand on sélectionne dans une liste, désélectionner l'autre :
```csharp
lbAvailabe.Enter += (_, __) => lbReturned.ClearSelected();
lbReturned.Enter += (_, __) => lbAvailabe.ClearSelected();
```

### **2. Événement CheckedChanged**
Détaché temporairement pour éviter les boucles :
```csharp
cbxRenduDsem.CheckedChanged -= CbxRenduDsem_CheckedChanged;
cbxRenduDsem.Checked = r.GetInt32(7) != 0;
cbxRenduDsem.CheckedChanged += CbxRenduDsem_CheckedChanged;
```

### **3. Tag pour stocker l'ID**
```csharp
cbxRenduDsem.Tag = equipmentId;
// Plus tard:
if (cbxRenduDsem.Tag is not string id) return;
```

### **4. BeginUpdate/EndUpdate**
Optimisation pour le remplissage des ListBox :
```csharp
lbAvailabe.BeginUpdate();
lbAvailabe.DataSource = items;
lbAvailabe.SelectedIndex = -1;
lbAvailabe.EndUpdate();
```

### **5. Champs en lecture seule**
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
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `equipment_type`

---

**📌 Prochaine étape :** Consulter `LoanCreationView.cs` pour la création de prêts.
