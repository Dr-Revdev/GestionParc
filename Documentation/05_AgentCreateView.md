# 📘 Documentation de AgentCreateView.cs

## 🎯 But de ce fichier
Formulaire pour **créer un nouvel agent** dans la base de données. Permet de saisir toutes les informations (IDRH, nom, prénom, email, équipe, site, etc.).

💡 **Analogie :** C'est comme remplir une fiche d'inscription pour un nouvel employé.

---

## 📋 Structure principale

Ce fichier contient **262 lignes** organisées en sections :
- **Lignes 1-8** : Imports
- **Lignes 15-32** : Variables de contrôles (champs du formulaire)
- **Lignes 34-41** : Constructeur
- **Lignes 43-126** : Construction de l'interface (`BuildUi`)
- **Lignes 128-130** : Méthodes utilitaires (conversion)
- **Lignes 132-155** : Classes internes (AgentSiteItem, AgentTeamItem)
- **Lignes 157-178** : Chargement des données (sites, équipes)
- **Lignes 180-189** : Validation du formulaire
- **Lignes 191-244** : Insertion en base de données
- **Lignes 246-262** : Méthode helper pour les lignes de formulaire

---

## 🎨 Interface utilisateur

### Structure du formulaire (3 colonnes)

```
┌─────────────────────────────────────────────────┐
│ [← Retour]                                      │
├─────────────────┬─────────────────┬─────────────┤
│ IDRH            │ Nom             │ Prénom      │
│ [________]      │ [________]      │ [________]  │
├─────────────────┼─────────────────┼─────────────┤
│ Email           │ Équipe          │ ☑ Hébergé   │
│ [________]      │ [▼_______]      │             │
├─────────────────┴─────────────────┼─────────────┤
│ Commentaire (multiligne)          │ Site        │
│ [_________________________]       │ [▼_______]  │
│                                   │             │
└───────────────────────────────────┴─────────────┘
                                        [  Créer  ]
```

---

## 🔑 Concepts clés

### 1. **Champs obligatoires**
- IDRH, Nom, Prénom, Email, Site, Équipe
- Validation avant insertion

### 2. **ComboBox pour Site et Équipe**
- Chargées depuis la base de données
- `AgentSiteItem` et `AgentTeamItem` pour l'affichage

### 3. **CheckBox "Hébergé"**
- Stocké comme 0/1 en base (méthode `ToBit`)

### 4. **Gestion des valeurs NULL**
- Méthode `ToDbNullable` pour les champs optionnels
- Convertit chaîne vide en `DBNull.Value`

### 5. **Transaction pour table Travail**
- Après insertion agent, gère la table `Travail`
- `DELETE` puis `INSERT` pour éviter doublons

---

## 💾 Insertion en base

### Étapes de `InsertAgent()` (lignes 191-244)

```csharp
1. Validation du formulaire
   ↓
2. Récupération des valeurs sélectionnées
   ↓
3. INSERT dans table "Agents"
   ↓
4. Transaction pour table "Travail"
   - DELETE ancien lien (si existe)
   - INSERT nouveau lien idrh ↔ site
   ↓
5. Commit transaction
   ↓
6. Reset du formulaire
```

**Requête SQL principale :**
```sql
INSERT INTO "Agents"
    (idrh, nom, prenom, email, equipe_id, heberge, commentaire, site_id)
VALUES ($idrh, $nom, $prenom, $email, $equipeId, $heberge, $comment, $siteId);
```

---

## 🎓 Méthodes importantes

### **LoadAgentSite() et LoadAgentTeam()**
Chargent les listes déroulantes depuis la base :

```csharp
SELECT id, name FROM Sites ORDER BY name;
SELECT id, name FROM Equipes ORDER BY name;
```

Utilisation de classes internes `AgentSiteItem` et `AgentTeamItem` pour binding.

### **ValidateTeamForm()**
Vérifie que tous les champs obligatoires sont remplis.

### **ToBit(bool b)**
Convertit `true` → 1, `false` → 0 pour SQLite.

### **ToDbNullable(string s)**
Convertit chaîne vide en `DBNull.Value` pour les champs NULL en base.

---

## 🎬 Scénario d'utilisation

```
1. AdminMenuView : Clic "Création Agent"
   ↓
2. WelcomePage affiche AgentCreateView
   ↓
3. Chargement des ComboBox (sites, équipes)
   ↓
4. Utilisateur remplit le formulaire
   ↓
5. Clic sur "Créer"
   ↓
6. Validation des champs
   ↓
7. Insertion dans "Agents"
   ↓
8. Gestion table "Travail" (transaction)
   ↓
9. Message de confirmation
   ↓
10. Reset du formulaire (prêt pour un nouvel agent)
```

---

## 💡 Points importants

**Ordre de tabulation :**
Les champs ont un `TabIndex` défini (0 à 8) pour navigation au clavier.

**Gestion d'erreurs :**
Try-catch autour de `ExecuteNonQuery()` pour capturer les erreurs SQL.

**Reset après création :**
Le formulaire est vidé après succès (prêt pour un nouvel agent).

**Transaction atomique :**
La gestion de la table `Travail` utilise une transaction pour garantir la cohérence.

---

## 🔗 Fichiers liés

- **AdminMenuView.cs** - Appelle cette vue
- **AgentEditView.cs** - Vue de modification
- **DataBase.cs** - Connexion à la base
- **Tables utilisées** : `Agents`, `Sites`, `Equipes`, `Travail`

---

**📌 Prochaine étape :** Consulter `AgentEditView.cs` pour la modification d'agents.
