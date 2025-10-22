# 📘 Documentation de EquipmentSelectionControl.cs

## 🎯 But de ce fichier
Contrôle **réutilisable** composé d'une ComboBox pour sélectionner un équipement + un bouton × pour le retirer. Utilisé dans `LoanCreationView`.

💡 **Analogie :** C'est comme une ligne dans un formulaire de commande où on choisit un article et on peut le retirer.

---

## 📋 Structure principale

Fichier court de **116 lignes** :
- Hérite de `Panel`
- ComboBox (475px) + Bouton × (30px)
- Événement `OnRemove` pour se retirer
- Charge les équipements disponibles
- Possibilité de pré-sélection

---

## 🎨 Interface visuelle

### Contrôle horizontal

```
┌───────────────────────────────────────────┬────┐
│ [▼ Ordinateur Dell [PC-001]_____________]│ × │
└───────────────────────────────────────────┴────┘
    475px                                    30px
```

**Dimensions :**
- Largeur totale : 520px
- Hauteur : 40px
- Margin bottom : 10px

---

## 🔑 Fonctionnalités

### 1. **Chargement des équipements disponibles**
```sql
SELECT e.id_equipement, e.nom, e.code_parc, t.name as type
FROM Equipements e
JOIN equipment_type t ON t.id = e.type_id
WHERE e.etat_pret = 0
ORDER BY t.name, e.nom
```

### 2. **Format d'affichage**
```
Type - Nom (Code)
Exemple: Ordinateur portable - Dell Latitude (PC-001)
```

### 3. **Pré-sélection d'équipement**
Si un ID est fourni au constructeur :
- Cherche d'abord dans les disponibles
- Si pas trouvé (équipement déjà prêté), charge spécifiquement
- Sélectionne automatiquement

### 4. **Événement de retrait**
```csharp
public event EventHandler OnRemove;
```
Déclenché quand on clique sur ×.

---

## 💾 Base de données

### **LoadEquipments()** - Liste disponibles
```sql
SELECT e.id_equipement, e.nom, e.code_parc, t.name as type
FROM Equipements e
JOIN equipment_type t ON t.id = e.type_id
WHERE e.etat_pret = 0
ORDER BY t.name, e.nom
```

### **Chargement présélection** - Si équipement déjà prêté
```sql
SELECT e.id_equipement, e.nom, e.code_parc, t.name as type
FROM Equipements e
JOIN equipment_type t ON t.id = e.type_id
WHERE e.id_equipement = $id
```
Permet d'éditer un prêt existant.

---

## 🎬 Scénario d'utilisation

### **Ajout d'un contrôle dans LoanCreationView**
```
1. LoanCreationView : AddEquipmentControl()
   ↓
2. new EquipmentSelectionControl(null)
   ↓
3. InitializeComponent() crée ComboBox + bouton
   ↓
4. LoadEquipments() remplit la ComboBox
   ↓
5. Contrôle ajouté à pnlEquipments
   ↓
6. Utilisateur sélectionne un équipement
   ↓
7. SelectedEquipment retourne l'EquipmentItem
```

### **Retrait d'un contrôle**
```
1. Utilisateur clique sur ×
   ↓
2. btnRemove.Click déclenché
   ↓
3. OnRemove?.Invoke(this, e)
   ↓
4. LoanCreationView reçoit l'événement
   ↓
5. LoanCreationView vérifie count > 1
   ↓
6. pnlEquipments.Controls.Remove(control)
   ↓
7. Contrôle supprimé de l'interface
```

### **Pré-sélection en mode édition**
```
1. LoanCreationView.LoadAssignedEquipments()
   ↓
2. Pour chaque équipement prêté:
   AddEquipmentControl(equipmentId)
   ↓
3. new EquipmentSelectionControl(equipmentId)
   ↓
4. LoadEquipments() charge disponibles
   ↓
5. Équipement pas dans la liste (déjà prêté)
   ↓
6. Requête spécifique avec WHERE id = $id
   ↓
7. Ajout à la ComboBox
   ↓
8. Sélection automatique
```

---

## 🎓 Concepts techniques

### **1. Hérite de Panel**
```csharp
public class EquipmentSelectionControl : Panel
```
Contient les contrôles enfants (ComboBox + Button).

### **2. Propriété SelectedEquipment**
```csharp
public EquipmentItem SelectedEquipment 
    => cmbEquipment.SelectedItem as EquipmentItem;
```
Accès direct à l'item sélectionné.

### **3. Constructeurs multiples**
```csharp
public EquipmentSelectionControl() : this(null) { }
public EquipmentSelectionControl(string preselectedEquipmentId)
```
Pattern avec/sans présélection.

### **4. Événement personnalisé**
```csharp
public event EventHandler OnRemove;
btnRemove.Click += (s, e) => OnRemove?.Invoke(this, e);
```

### **5. Position absolue des contrôles**
```csharp
cmbEquipment = new ComboBox
{
    Left = 0,
    Top = 5,
    Width = 475
};

btnRemove = new Button
{
    Left = 485,
    Top = 5,
    Width = 30,
    Height = 30
};
```

---

## 💡 Points importants

**ComboBox en lecture seule :**
```csharp
DropDownStyle = ComboBoxStyle.DropDownList
```
Empêche la saisie manuelle, seulement sélection.

**Bouton × rouge :**
```csharp
ForeColor = Color.Red
```
Indique visuellement la suppression.

**Gestion des équipements prêtés :**
Si l'équipement est déjà prêté (mode édition), il est chargé spécifiquement et ajouté à la liste.

**Pas de validation interne :**
La validation se fait dans `LoanCreationView`, pas dans ce contrôle.

---

## ⚠️ Attention

**Dépendance à EquipmentItem :**
```csharp
using ProjetParc.Views.Loan.Models;
```
Nécessite la classe `EquipmentItem` pour le binding.

**OnRemove nullable :**
```csharp
OnRemove?.Invoke(this, e)
```
Vérifie toujours si l'événement est attaché.

**Pas de vérification de doublon :**
Ce contrôle ne vérifie pas si un équipement est sélectionné plusieurs fois.
C'est `LoanCreationView` qui doit gérer.

---

## 🔗 Fichiers liés

- **LoanCreationView.cs** - Utilise ce contrôle
- **Models/EquipmentItem.cs** - Classe pour binding
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `equipment_type`

---

## 📝 Classe EquipmentItem (rappel)

```csharp
public class EquipmentItem
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public override string ToString() => DisplayName;
}
```

Format DisplayName : `Type - Nom (Code)`

---

**📌 Prochaine étape :** Consulter `MainInventoryView.cs` pour la vue d'inventaire complète.
