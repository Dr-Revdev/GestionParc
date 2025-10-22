# 📘 Documentation de AgentEditView.cs

## 🎯 But de ce fichier
Vue de **modification et suppression** d'agents existants. Affiche une liste recherchable, un formulaire d'édition et des boutons d'action.

💡 **Analogie :** C'est comme un fichier de ressources humaines où on peut consulter et modifier les fiches des employés.

---

## 📋 Structure principale

Ce fichier contient **322 lignes** organisées en :
- **Liste d'agents** (gauche) avec recherche
- **Formulaire d'édition** (droite)
- **Boutons Modifier/Supprimer**

---

## 🎨 Interface utilisateur

### Layout en 2 colonnes

```
┌────────────────────────────────────────────────────┐
│ [← Retour]                                         │
├─────────────────────┬──────────────────────────────┤
│ [🔍 Recherche]  [⌕] │  FORMULAIRE D'ÉDITION        │
│                     │  ┌─────────┬─────────┬──────┐│
│ Liste des agents:   │  │ IDRH    │ Nom     │Prénom││
│ ┌─────────────────┐ │  │         │         │      ││
│ │ Dupont Jean     │ │  ├─────────┼─────────┼──────┤│
│ │ Martin Sophie   │◄┼─►│ Email   │ Équipe  │☑Héb. ││
│ │ Durand Pierre   │ │  │         │         │      ││
│ │ ...             │ │  ├─────────┴─────────┼──────┤│
│ └─────────────────┘ │  │ Commentaire       │ Site ││
│                     │  │                    │      ││
│                     │  └────────────────────┴──────┘│
│                     │        [Modifier] [Supprimer] │
└─────────────────────┴──────────────────────────────┘
```

---

## 🔑 Fonctionnalités principales

### 1. **Recherche d'agents**
- Barre de recherche en haut de la liste
- Filtre sur : IDRH, nom, prénom, email
- Requête SQL avec `LIKE %recherche%`

### 2. **Affichage de la liste**
- Tous les agents triés par nom, prénom
- Format : `Nom Prénom [IDRH]`
- Sélection change le formulaire

### 3. **Modification**
- Champs pré-remplis avec les données de l'agent
- IDRH en lecture seule (pas modifiable)
- Mise à jour par UPDATE SQL

### 4. **Suppression**
- Confirmation avant suppression
- DELETE de la table Agents
- Rafraîchissement de la liste

---

## 💾 Opérations en base

### **LoadAgentList()** - Chargement initial
```sql
SELECT idrh, TRIM(COALESCE(nom,'')), TRIM(COALESCE(prenom,''))
FROM "Agents" ORDER BY 2, 3, 1;
```

### **LoadAgentListFiltered(query)** - Recherche
```sql
SELECT idrh, nom, prenom FROM "Agents"
WHERE idrh LIKE $p OR nom LIKE $p OR prenom LIKE $p OR email LIKE $p
ORDER BY 2, 3, 1;
```

### **LoadAgentById(agentIDRH)** - Détails
```sql
SELECT idrh, nom, prenom, email, equipe_id, heberge, commentaire, site_id
FROM "Agents" WHERE idrh = $IDRH;
```

### **SaveAgentChanges()** - Modification
```sql
UPDATE "Agents" 
SET nom = $name, prenom = $firstName, email = $email,
    equipe_id = $teamId, heberge = $heberge,
    commentaire = $comment, site_id = $siteId
WHERE idrh = $id;
```

### **DeleteSelectedAgent()** - Suppression
```sql
DELETE FROM "Agents" WHERE idrh = $id;
```

---

## 🎬 Scénario d'utilisation

### **Scénario 1 : Modification d'un agent**
```
1. Ouverture de la vue → LoadAgentList()
   ↓
2. Utilisateur sélectionne "Dupont Jean"
   ↓
3. lbAgents_SelectedIndexChanged déclenché
   ↓
4. LoadAgentById("IDRH_DUPONT") charge les données
   ↓
5. Formulaire pré-rempli
   ↓
6. Utilisateur modifie l'email
   ↓
7. Clic "Modifier" → SaveAgentChanges()
   ↓
8. UPDATE en base
   ↓
9. Message de confirmation
   ↓
10. Liste rafraîchie (label mis à jour)
```

### **Scénario 2 : Recherche et suppression**
```
1. Utilisateur tape "Martin" dans la recherche
   ↓
2. Clic sur 🔍 → LoadAgentListFiltered("Martin")
   ↓
3. Liste filtrée affichée
   ↓
4. Sélection "Martin Sophie"
   ↓
5. Clic "Supprimer"
   ↓
6. MessageBox de confirmation
   ↓
7. Si OUI → DELETE FROM Agents
   ↓
8. Liste rafraîchie (agent disparu)
   ↓
9. Formulaire vidé
```

---

## 🎓 Concepts techniques

### **1. Événement SelectedIndexChanged**
Détecte le changement de sélection dans la ListBox :
```csharp
lbAgents.SelectedIndexChanged += lbAgents_SelectedIndexChanged;
```

### **2. Classes internes pour binding**
- `AgentListItem` - Pour la ListBox
- `AgentSiteItem` - Pour ComboBox sites
- `AgentTeamItem` - Pour ComboBox équipes

### **3. Gestion des NULL**
- `COALESCE(nom,'')` en SQL pour gérer les NULL
- `IsDBNull()` pour vérifier avant lecture
- `ToDbNullable()` pour convertir vers DBNull

### **4. Validation avant modification**
- `ValidateAgentForm()` vérifie les champs obligatoires
- Empêche UPDATE avec données invalides

### **5. Rafraîchissement intelligent**
Après modification, met à jour seulement le label dans la liste :
```csharp
it.Label = newLabel;
lbAgents.DisplayMember = null;  // Force refresh
lbAgents.DisplayMember = nameof(AgentListItem.Label);
```

---

## ⚠️ Points d'attention

**IDRH non modifiable :**
```csharp
tbIDRH = new TextBox { Height = 36, ReadOnly = true }
```
L'IDRH est la clé primaire, on ne peut pas la changer.

**Confirmation de suppression :**
```csharp
var confirm = MessageBox.Show(
    $"Supprimer « {item.Label} » ?",
    "Confirmer la suppression",
    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
```

**Gestion des erreurs SQL :**
Try-catch autour de toutes les opérations de base de données.

---

## 🔗 Fichiers liés

- **AdminMenuView.cs** - Appelle cette vue
- **AgentCreateView.cs** - Vue de création
- **DataBase.cs** - Connexion
- **Tables** : `Agents`, `Sites`, `Equipes`

---

**📌 Prochaine étape :** Consulter `EquipmentCreateView.cs` pour la création d'équipements.
