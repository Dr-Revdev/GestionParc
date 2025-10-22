# 📘 Documentation de LoanCreationView.cs

## 🎯 But de ce fichier
Fenêtre modale pour **créer ou modifier un prêt** d'équipements à un agent. Gère l'attribution de plusieurs équipements à un agent et la suppression de prêts.

💡 **Analogie :** C'est comme un formulaire de prêt de matériel où on sélectionne un employé et on lui attribue des équipements.

---

## 📋 Structure principale

Fichier de **363 lignes** avec :
- **Form** modale (fenêtre popup)
- Sélection d'agent (ComboBox)
- Liste dynamique d'équipements (FlowLayoutPanel)
- Boutons : Ajouter, Supprimer prêt, Valider, Annuler

---

## 🎨 Interface utilisateur

### Fenêtre modale

```
┌─────────────────────────────────────────┐
│  Nouveau prêt                      [×]  │
├─────────────────────────────────────────┤
│ Agent :                                 │
│ [▼ Dupont Jean__________________]       │
│                                         │
│ Équipements :                           │
│ ┌─────────────────────────────────────┐ │
│ │ [▼ Ordinateur Dell [PC-001]] [×]   │ │
│ │ [▼ Écran LG [MON-005]] [×]         │ │
│ │ [▼ Souris Logitech [MOU-12]] [×]   │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Ajouter un équipement]                 │
│ [Supprimer le prêt]                     │
│                                         │
│                   [Valider]  [Annuler]  │
└─────────────────────────────────────────┘
```

---

## 🔑 Fonctionnalités principales

### 1. **Mode Création ou Édition**
- **Création** : `SelectedAgentId` vide → nouveau prêt
- **Édition** : `SelectedAgentId` fourni → modifier prêt existant

### 2. **Sélection d'agent**
ComboBox avec tous les agents :
```sql
SELECT idrh, nom, prenom FROM Agents ORDER BY nom, prenom
```

### 3. **Équipements dynamiques**
- Liste de contrôles `EquipmentSelectionControl`
- Bouton "Ajouter" pour ajouter une ligne
- Bouton × sur chaque ligne pour retirer
- Minimum 1 équipement toujours présent

### 4. **Validation du prêt**
- Vérifie agent sélectionné
- Vérifie au moins 1 équipement
- UPDATE en base avec transaction
- Gère les ajouts/retraits d'équipements

### 5. **Suppression du prêt**
- Libère tous les équipements (`etat_pret = 0`)
- Enlève le lien avec l'agent (`idrh = NULL`)

---

## 💾 Base de données

### **LoadAgents()** - Liste agents
```sql
SELECT idrh, nom, prenom 
FROM Agents 
ORDER BY nom, prenom
```

### **LoadAssignedEquipments()** - Mode édition
```sql
SELECT e.id_equipement
FROM Equipements e
WHERE e.idrh = $idrh AND e.etat_pret = 1
```

### **ValidateLoan()** - Attribution
```sql
-- Pour chaque équipement sélectionné:
UPDATE Equipements 
SET idrh = $idrh, 
    etat_pret = CASE 
        WHEN etat_pret = 2 THEN 2  -- Garde DSEM si déjà DSEM
        ELSE 1                      -- Sinon met en prêt
    END 
WHERE id_equipement = $id
```

### **DeleteLoan()** - Suppression
```sql
UPDATE Equipements 
SET idrh = NULL, etat_pret = 0 
WHERE idrh = $idrh AND etat_pret = 1
```

---

## 🎬 Scénarios d'utilisation

### **Création d'un nouveau prêt**
```
1. MainInventoryView : Clic "Nouveau prêt"
   ↓
2. new LoanCreationView() affiché en modal
   ↓
3. LoadAgents() remplit ComboBox
   ↓
4. 1 EquipmentSelectionControl ajouté par défaut
   ↓
5. Utilisateur sélectionne "Dupont Jean"
   ↓
6. Utilisateur sélectionne "PC Dell [001]"
   ↓
7. Clic "Ajouter un équipement"
   ↓
8. Nouveau EquipmentSelectionControl ajouté
   ↓
9. Utilisateur sélectionne "Écran LG [005]"
   ↓
10. Clic "Valider"
   ↓
11. ValidateLoan() vérifie les données
   ↓
12. Transaction SQL :
    - UPDATE PC-001 : idrh = DUPONT, etat_pret = 1
    - UPDATE MON-005 : idrh = DUPONT, etat_pret = 1
   ↓
13. Commit transaction
   ↓
14. DialogResult = OK
   ↓
15. Fenêtre se ferme
   ↓
16. MainInventoryView rafraîchit les listes
```

### **Modification d'un prêt existant**
```
1. MainInventoryView : Double-clic sur un agent
   ↓
2. new LoanCreationView { SelectedAgentId = "IDRH_DUPONT" }
   ↓
3. OnShown() → LoadAssignedEquipments()
   ↓
4. Équipements actuels chargés (PC et Écran)
   ↓
5. Utilisateur retire l'écran (clic ×)
   ↓
6. Utilisateur ajoute une souris
   ↓
7. Clic "Valider"
   ↓
8. ValidateLoan() compare ancien/nouveau :
    - PC : déjà attribué → pas de changement
    - Écran : retiré → UPDATE idrh = NULL, etat_pret = 0
    - Souris : ajoutée → UPDATE idrh = DUPONT, etat_pret = 1
   ↓
9. Transaction commitée
   ↓
10. Fenêtre fermée avec DialogResult = OK
```

---

## 🎓 Concepts techniques

### **1. Propriété SelectedAgentId**
```csharp
public string SelectedAgentId 
{ 
    get => selectedAgentId;
    set 
    {
        selectedAgentId = value ?? string.Empty;
        if (IsHandleCreated && !string.IsNullOrEmpty(selectedAgentId))
        {
            SelectAgentById(selectedAgentId);
            LoadAssignedEquipments(selectedAgentId);
        }
    }
}
```
Permet de pré-remplir le formulaire en mode édition.

### **2. OnShown() override**
```csharp
protected override void OnShown(System.EventArgs e)
{
    base.OnShown(e);
    if (!string.IsNullOrEmpty(selectedAgentId))
    {
        SelectedAgentId = selectedAgentId;
    }
}
```
Force la mise à jour après affichage.

### **3. FlowLayoutPanel dynamique**
```csharp
pnlEquipments = new FlowLayoutPanel
{
    Dock = DockStyle.Fill,
    AutoScroll = true,
    FlowDirection = FlowDirection.TopDown,
    WrapContents = false
};
```
Gère automatiquement le scroll si trop d'équipements.

### **4. HashSet pour comparaison**
```csharp
var previouslyAssigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var newlySelected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
// Comparaison efficace pour détecter ajouts/retraits
```

### **5. Transaction pour cohérence**
```csharp
using var transaction = connection.BeginTransaction();
// Plusieurs UPDATE
transaction.Commit();
```

---

## 💡 Points importants

**Minimum 1 équipement :**
```csharp
if (pnlEquipments.Controls.Count > 1) // Garde au moins un
{
    pnlEquipments.Controls.Remove((Control)sender);
}
```

**DialogResult :**
```csharp
DialogResult = DialogResult.OK;
Close();
```
Permet au parent de savoir si OK ou Annuler.

**Gestion DSEM :**
Les équipements en état DSEM (2) restent en DSEM même si réattribués.

**Présélection d'équipements :**
`AddEquipmentControl(preselectEquipmentId)` permet de pré-remplir.

---

## ⚠️ Attention

**Transaction importante :**
Tous les UPDATE doivent réussir ou tout est annulé.

**Événement OnRemove :**
Chaque `EquipmentSelectionControl` déclenche un callback pour se retirer.

**Validation stricte :**
- Agent obligatoire
- Au moins 1 équipement avec sélection valide

---

## 🔗 Fichiers liés

- **MainInventoryView.cs** - Ouvre cette fenêtre
- **EquipmentSelectionControl.cs** - Contrôle de sélection
- **Models/AgentItem.cs** - Classe pour ComboBox agent
- **Models/EquipmentItem.cs** - Classe pour ComboBox équipement
- **DataBase.cs** - Connexion
- **Tables** : `Equipements`, `Agents`

---

**📌 Prochaine étape :** Consulter `EquipmentSelectionControl.cs` pour comprendre le contrôle de sélection.
