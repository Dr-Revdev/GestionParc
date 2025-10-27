# 📘 Documentation de EquipmentEditView.cs

## 🎯 But de ce fichier
Vue de **modification et suppression** d'équipements existants. Interface en 3 parties : liste avec recherche, séparateur visuel, formulaire d'édition.

💡 **Analogie :** C'est comme un catalogue d'inventaire où on peut consulter, modifier et retirer des articles.

---

## 📋 Structure principale

Fichier de **~590 lignes** avec :
- **Liste d'équipements** (gauche, 30%) - **ListView triable avec colonnes**
- **Séparateur gris** (2px)
- **Formulaire d'édition** (droite, 70%)
- Recherche, modification, suppression

**Nouveau** : Utilise `ListView` avec 4 colonnes triables au lieu de `ListBox`.

---

## 🎨 Interface utilisateur

### Layout en 3 colonnes

```
┌────────────────────────────────────────────────────────────┐
│ [← Retour]                                                 │
├─────────────────────┬──┬────────────────────────────────────┤
│ [🔍 Recherche]  [⌕] │░░│  FORMULAIRE D'ÉDITION             │
│                     │░░│  ┌──────┬──────┬──────────┐       │
│ Liste équipements:  │░░│  │ Type │ Nom  │Code parc │       │
│ ┌─────────────────┐ │░░│  ├──────┼──────┼──────────┤       │
│ │Type│Code│N°│Nom │ │░░│  │ N°   │Marque│          │       │
│ ├────┼────┼──┼────┤ │░░│  │série │      │          │       │
│ │PC  │001 │..│Dell│◄┼░░┼─►├──────┴──────┴──────────┤       │
│ │Écrn│02  │..│LG  │ │░░│  │ Commentaire (multiligne)│       │
│ │... │    │  │    │ │░░│  │                         │       │
│ └────┴────┴──┴────┘ │░░│  └─────────────────────────┘       │
│ ⬆️ Clic pour trier   │░░│        [Modifier] [Supprimer]    │
└─────────────────────┴──┴────────────────────────────────────┘
```

**Colonnes triables** :
- Type
- Code Parc
- N° Série
- Nom

Cliquer sur une colonne pour trier, re-cliquer pour inverser l'ordre.

---

## 🔑 Fonctionnalités principales

### 1. **Recherche d'équipements**
Filtre sur : nom, code parc, numéro de série, type
```sql
WHERE e.nom LIKE $p OR e.code_parc LIKE $p 
   OR e.numero_serie LIKE $p OR t.name LIKE $p
```

### 2. **Affichage liste (ListView avec colonnes)**
**Nouveau** : Format en colonnes séparées au lieu d'un label unique.

Requête avec JOIN et colonnes :
```sql
SELECT e.id_equipement,
       t.name AS type,
       TRIM(COALESCE(e.code_parc, '-')) AS code,
       TRIM(COALESCE(e.numero_serie, '-')) AS serial,
       COALESCE(TRIM(e.nom), '(sans nom)') AS nom
FROM Equipements e
JOIN equipment_type t ON t.id = e.type_id
ORDER BY type, code, serial;
```
ORDER BY e.nom COLLATE NOCASE
```

### 3. **Modification**
- Tous les champs modifiables
- UPDATE SQL avec tous les paramètres
- Rafraîchissement du label dans la liste

### 4. **Suppression**
- Confirmation obligatoire
- DELETE de la base
- Rafraîchissement complet de la liste

---

## 💾 Opérations en base

### **LoadEquipmentList()** - Liste complète
```sql
SELECT e.id_equipement,
    COALESCE(TRIM(e.nom), '(sans nom)') AS n,
    TRIM(COALESCE(e.code_parc, ''))     AS c,
    t.name AS typ
FROM Equipements e
JOIN equipment_type t ON t.id = e.type_id
ORDER BY n COLLATE NOCASE, typ COLLATE NOCASE, c COLLATE NOCASE;
```

### **LoadEquipmentById(equipmentId)** - Détails
```sql
SELECT type_id, nom, code_parc, numero_serie, marque, commentaire 
FROM "Equipements" WHERE id_equipement = $id;
```

### **SaveEquipmentChanges()** - Modification
```sql
UPDATE "Equipements"
SET type_id = $typeId,
    nom = $name,
    code_parc = $codeParc,
    numero_serie = $serial,
    marque = $brand,
    commentaire = $comment
WHERE id_equipement = $id;
```

### **DeleteSelectedEquipment()** - Suppression
```sql
DELETE FROM "Equipements" WHERE id_equipement = $id;
```

---

## 🎬 Scénario d'utilisation

### **Modification d'un équipement**
```
1. Vue ouverte → LoadEquipmentTypes() et LoadEquipmentList()
   ↓
2. Utilisateur cherche "Dell" → btnSearch_Click
   ↓
3. LoadEquipmentListFiltered("Dell")
   ↓
4. Liste filtrée affichée
   ↓
5. Sélection "Dell Latitude [PC-001]"
   ↓
6. lbEquipment_SelectedIndexChanged
   ↓
7. LoadEquipmentById(id) charge les détails
   ↓
8. Formulaire pré-rempli
   ↓
9. Modification du nom "Dell Latitude 5420 i7"
   ↓
10. Clic "Modifier" → SaveEquipmentChanges()
   ↓
11. UPDATE en base
   ↓
12. Label mis à jour dans la liste
   ↓
13. Message "Modifications enregistrées"
```

---

## 🎓 Concepts techniques

### **1. Classes internes**
```csharp
// Pour la ListBox
private sealed class EquipmentListItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string Label => string.IsNullOrEmpty(Code) 
        ? $"{Name}  {Type}" 
        : $"{Name}  [{Code}]  {Type}";
}

// Pour la ComboBox
private sealed class EquipmentTypeItem
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### **2. Séparateur visuel**
```csharp
var separator = new Panel { 
    Dock = DockStyle.Fill, 
    BackColor = Color.Silver 
};
```
2px de largeur dans le layout.

### **3. COLLATE NOCASE**
Tri insensible à la casse en SQLite :
```sql
ORDER BY n COLLATE NOCASE
```

### **4. Rafraîchissement intelligent**
Après modification :
```csharp
selected.Name = tbName.Text.Trim();
selected.Code = tbCodeParc.Text.Trim();
selected.Type = selectedType.Name;

lbEquipment.DisplayMember = null;  // Force le refresh
lbEquipment.DisplayMember = nameof(EquipmentListItem.Label);
```

---

## ⚠️ Points d'attention

**ID non modifiable :**
L'`id_equipement` est géré en interne, pas visible dans le formulaire.

**Validation obligatoire :**
- Nom, Type, Code parc doivent être remplis
- `ValidateEquipmentForm()` avant UPDATE

**Confirmation suppression :**
```csharp
var confirm = MessageBox.Show(
    $"Supprimer « {item.Label} » ?",
    "Confirmer la suppression",
    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
```

**Gestion des NULL :**
`ToDbNullable()` pour les champs optionnels.

---

## 🔗 Fichiers liés

- **AdminMenuView.cs** - Appelle cette vue
- **EquipmentCreateView.cs** - Création
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `equipment_type`

---

**📌 Prochaine étape :** Consulter `FreeEquipmentView.cs` pour les équipements disponibles.
