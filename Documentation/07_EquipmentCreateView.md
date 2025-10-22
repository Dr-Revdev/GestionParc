# 📘 Documentation de EquipmentCreateView.cs

## 🎯 But de ce fichier
Formulaire pour **créer un nouvel équipement** dans le système. Permet de saisir type, nom, code parc, numéro de série, marque et commentaire.

💡 **Analogie :** C'est comme enregistrer un nouveau matériel dans l'inventaire d'une entreprise.

---

## 📋 Structure principale

Fichier de **184 lignes** organisé en :
- Interface avec 3 colonnes
- ComboBox pour le type d'équipement
- Validation et insertion en base
- Génération automatique d'ID unique

---

## 🎨 Interface utilisateur

### Formulaire en 3 colonnes

```
┌─────────────────────────────────────────────────┐
│ [← Retour]                                      │
├───────────────┬───────────────┬─────────────────┤
│ Type          │ Nom           │ Code parc       │
│ [▼ Ordinateur]│ [________]    │ [________]      │
├───────────────┼───────────────┼─────────────────┤
│ N° de série   │ Marque        │                 │
│ [________]    │ [________]    │                 │
├───────────────┴───────────────┴─────────────────┤
│ Commentaire (multiligne)                        │
│ [___________________________________________]   │
│ [___________________________________________]   │
└─────────────────────────────────────────────────┘
                                      [   Créer   ]
```

---

## 🔑 Fonctionnalités clés

### 1. **Type d'équipement**
- ComboBox chargée depuis table `equipment_type`
- Affiche le nom, stocke l'ID
- Obligatoire

### 2. **Champs obligatoires**
- Type, Nom, Code parc
- Validation avant insertion

### 3. **Champs optionnels**
- Numéro de série, Marque, Commentaire
- Convertis en NULL si vides

### 4. **ID unique automatique**
- Généré avec `Guid.NewGuid().ToString("N")`
- Format : 32 caractères hexadécimaux
- Exemple : `a1b2c3d4e5f6789012345678901234ab`

---

## 💾 Base de données

### **LoadEquipmentTypes()** - Chargement des types
```sql
SELECT id, name FROM equipment_type ORDER BY name;
```

Stocke dans `EquipmentTypeItem` :
```csharp
private sealed class EquipmentTypeItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => Name;
}
```

### **InsertEquipment()** - Insertion
```sql
INSERT INTO "Equipements" 
    (id_equipement, type_id, nom, code_parc, numero_serie, marque, commentaire) 
VALUES ($id, $typeId, $name, $codeParc, $serial, $brand, $comment);
```

**Paramètres :**
- `$id` : GUID généré
- `$typeId` : ID du type sélectionné
- `$name`, `$codeParc` : Valeurs obligatoires
- `$serial`, `$brand`, `$comment` : NULL si vides

---

## 🎬 Scénario d'utilisation

```
1. AdminMenuView : Clic "Création Equipement"
   ↓
2. AgentCreateView affichée
   ↓
3. LoadEquipmentTypes() charge la ComboBox
   ↓
4. Utilisateur remplit le formulaire:
   - Type: "Ordinateur portable"
   - Nom: "Dell Latitude 5420"
   - Code parc: "PC-2024-001"
   - N° série: "ABC123XYZ"
   - Marque: "Dell"
   ↓
5. Clic "Créer"
   ↓
6. ValidateEquipmentForm() vérifie les champs
   ↓
7. GenerateEquipmentId() crée un GUID
   ↓
8. INSERT en base de données
   ↓
9. Message "Équipement créé"
   ↓
10. Formulaire réinitialisé (prêt pour un autre)
```

---

## 🎓 Méthodes importantes

### **ValidateEquipmentForm()**
Vérifie :
- Nom non vide
- Type sélectionné
- Code parc non vide

Retourne `false` et un message d'erreur si échec.

### **GenerateEquipmentId()**
```csharp
private static string GenerateEquipmentId() 
    => Guid.NewGuid().ToString("N");
```
**Format "N"** : 32 chiffres sans tirets ni accolades.

### **ToDbNullable(string s)**
```csharp
private static object ToDbNullable(string s) 
    => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();
```
Gère les champs optionnels (NULL en base).

### **AddFormRow()**
Méthode helper pour ajouter label + contrôle :
```csharp
private void AddFormRow(TableLayoutPanel panel, int row, 
    string labelText, Control control, int col = 0, int colSpan = 1)
```

---

## 💡 Points importants

**Ordre de tabulation :**
Les contrôles ont un `TabIndex` (0-6) pour navigation clavier.

**Reset après création :**
```csharp
tbSerial.Clear();
tbName.Clear();
tbBrand.Clear();
tbCodeParc.Clear();
tbComment.Clear();
if (cbType.Items.Count > 0) cbType.SelectedIndex = 0;
```

**Gestion d'erreurs :**
```csharp
try
{
    command.ExecuteNonQuery();
    MessageBox.Show("Équipement créé.");
}
catch (SqliteException ex)
{
    MessageBox.Show("Erreur SQL : " + ex.Message);
}
```

---

## 🔗 Fichiers liés

- **AdminMenuView.cs** - Appelle cette vue
- **EquipmentEditView.cs** - Vue de modification
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `equipment_type`

---

**📌 Prochaine étape :** Consulter `EquipmentEditView.cs` pour la modification d'équipements.
