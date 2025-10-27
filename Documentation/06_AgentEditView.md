# 📘 Documentation de AgentEditView.cs

## 🎯 But de ce fichier
Vue de **modification et suppression** d'agents existants. Affiche une liste recherchable, un formulaire d'édition et des boutons d'action.

💡 **Analogie :** C'est comme un fichier de ressources humaines où on peut consulter et modifier les fiches des employés.

---

## 📋 Structure principale

Ce fichier contient **~580 lignes** organisées en :
- **Liste d'agents** (gauche) avec recherche - **ListView triable avec colonnes**
- **Formulaire d'édition** (droite)
- **Boutons Modifier/Supprimer**

**Nouveau** : Utilise `ListView` au lieu de `ListBox` avec 4 colonnes triables.

---

## 🎨 Interface utilisateur

### Layout en 2 colonnes

```
┌──────────────────────────────────────────────────────────────┐
│ [← Retour]                                                   │
├─────────────────────┬────────────────────────────────────────┤
│ [🔍 Recherche]  [⌕] │  FORMULAIRE D'ÉDITION                  │
│                     │  ┌─────────┬─────────┬──────┐          │
│ Liste des agents:   │  │ IDRH    │ Nom     │Prénom│          │
│ ┌─────────────────┐ │  │         │         │      │          │
│ │IDRH│Nom     │E│S│ │  ├─────────┼─────────┼──────┤          │
│ ├────┼────────┼─┼─┤ │  │ Email   │ Équipe  │☑Héb. │          │
│ │1234│Dupont J│A│P│◄┼─►│         │         │      │          │
│ │5678│Martin S│B│L│ │  ├─────────┴─────────┼──────┤          │
│ │... │...     │ │ │ │  │ Commentaire       │ Site │          │
│ └────┴────────┴─┴─┘ │  │                   │      │          │
│                     │  └───────────────────┴──────┘          │
│ ⬆️ Clic pour trier  │         [Modifier] [Supprimer]         │
└─────────────────────┴────────────────────────────────────────┘
```

**Colonnes triables** :
- IDRH
- Nom Prénom
- Equipe
- Site

Cliquer sur une colonne pour trier, re-cliquer pour inverser l'ordre.

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
SELECT 
    a.idrh, 
    TRIM(COALESCE(a.nom,'')) AS n, 
    TRIM(COALESCE(a.prenom,'')) AS p,
    COALESCE(e.name, '-') AS equipe,
    COALESCE(s.name, '-') AS site
FROM "Agents" a
LEFT JOIN "Equipes" e ON a.equipe_id = e.id
LEFT JOIN "Sites" s ON a.site_id = s.id
ORDER BY n, p, a.idrh;
```
**Nouveau** : Inclut l'équipe et le site pour affichage en colonnes.

### **LoadAgentListFiltered(query)** - Recherche
```sql
SELECT 
    a.idrh, 
    TRIM(COALESCE(a.nom,'')), 
    TRIM(COALESCE(a.prenom,'')),
    COALESCE(e.name, '-'),
    COALESCE(s.name, '-')
FROM "Agents" a
LEFT JOIN "Equipes" e ON a.equipe_id = e.id
LEFT JOIN "Sites" s ON a.site_id = s.id
WHERE a.idrh LIKE $p OR a.nom LIKE $p OR a.prenom LIKE $p OR a.email LIKE $p
ORDER BY 2, 3, 1;
```
**Nouveau** : Retourne toutes les colonnes pour le ListView.

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

### **1. ListView avec tri par colonnes**
Utilise `ListViewColumnSorter` :
```csharp
lvAgentsSorter = new ListViewColumnSorter();
lvAgents.ListViewItemSorter = lvAgentsSorter;
lvAgents.ColumnClick += (s, e) => {
    lvAgentsSorter.SetSortColumn(e.Column);
    lvAgents.Sort();
};
```

### **2. Remplissage du ListView**
```csharp
lvAgents.Items.Clear();
var item = new ListViewItem(id);
item.SubItems.AddRange(new[] { nomComplet, equipe, site });
item.Tag = id; // Stocke l'IDRH
lvAgents.Items.Add(item);
```

### **3. Événement SelectedIndexChanged**
Détecte le changement de sélection :
```csharp
if (lvAgents.SelectedItems.Count > 0)
{
    var selectedItem = lvAgents.SelectedItems[0];
    var agentId = (string)selectedItem.Tag;
    LoadAgentById(agentId);
}
```

### **4. Classes internes pour binding**
- `AgentSiteItem` - Pour ComboBox sites
- `AgentTeamItem` - Pour ComboBox équipes

### **5. Gestion des NULL**
- `COALESCE(nom,'')` en SQL pour gérer les NULL
- `IsDBNull()` pour vérifier avant lecture
- `ToDbNullable()` pour convertir vers DBNull

### **6. Validation avant modification**
- `ValidateAgentForm()` vérifie les champs obligatoires
- Empêche UPDATE avec données invalides

### **7. Rafraîchissement après modification**
Recharge toute la liste pour refléter les changements :
```csharp
LoadAgentList(); // Rafraîchit tout le ListView
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

**Format d'affichage :**
Les colonnes Equipe et Site affichent "-" si l'agent n'a pas d'équipe/site associé.

**Bordures et grille :**
```csharp
BorderStyle = BorderStyle.FixedSingle,
GridLines = true
```
Affichage professionnel avec bordures noires et lignes de séparation.

---

## 🔗 Fichiers liés

- **AdminMenuView.cs** - Appelle cette vue
- **AgentCreateView.cs** - Vue de création
- **ListViewColumnSorter.cs** - Tri des colonnes
- **DataBase.cs** - Connexion
- **Tables** : `Agents`, `Sites`, `Equipes`

---

**📌 Prochaine étape :** Consulter `EquipmentCreateView.cs` pour la création d'équipements.
