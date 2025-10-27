# Documentation : SettingsView

## Vue d'ensemble

`SettingsView` est la vue de gestion des **paramètres de l'application**. Elle permet de gérer les listes référentielles utilisées dans l'application :
- **Équipes** (table `Equipes`)
- **Sites** (table `Sites`)  
- **Types d'équipement** (table `equipment_type`)

## Accès

Depuis le **Menu d'Administration**, cliquer sur le bouton **"⚙ Paramètres"** (en haut à droite).

## Architecture

### 1. SettingsView (Vue principale)
- **Rôle** : Conteneur principal avec système d'onglets
- **Structure** :
  - En-tête avec bouton "← Retour" et titre "Paramètres"
  - TabControl avec 3 onglets (Équipes, Sites, Types d'équipement)
  - Chaque onglet contient un `ParameterManagerControl`

### 2. ParameterManagerControl (Gestionnaire CRUD)
- **Rôle** : Gestion complète d'une table de paramètres
- **Fonctionnalités** :
  - ✅ **Ajouter** un nouvel élément
  - ✅ **Modifier** un élément existant
  - ✅ **Supprimer** un élément (avec protection)
  - 📋 **Lister** tous les éléments

## Interface utilisateur

### Layout
```
┌────────────────────────────────────────────────────┐
│  ← Retour              Paramètres                  │
├────────────────────────────────────────────────────┤
│  [Équipes] [Sites] [Types d'équipement]           │
├──────────────────────┬─────────────────────────────┤
│  Liste (60%)         │  Formulaire (40%)           │
│  ┌────────────────┐  │  Nom :                      │
│  │ ID │ Nom       │  │  [_________________]        │
│  ├────┼───────────┤  │                             │
│  │ 1  │ IT        │  │  [Ajouter]                  │
│  │ 2  │ Finance   │  │                             │
│  │ 3  │ RH        │  │  [Modifier]  (désactivé)    │
│  └────────────────┘  │  [Supprimer] (désactivé)    │
└──────────────────────┴─────────────────────────────┘
```

### Comportement
1. **Au chargement** : La liste est remplie, les boutons Modifier/Supprimer sont désactivés
2. **Sélection** : Cliquer sur un élément → le nom s'affiche dans le champ, les boutons s'activent
3. **Ajouter** : Entrer un nom, cliquer "Ajouter" → l'élément est créé
4. **Modifier** : Sélectionner un élément, modifier le nom, cliquer "Modifier" → mise à jour
5. **Supprimer** : Sélectionner un élément, cliquer "Supprimer" → vérification + confirmation

## Protections de suppression

### Règle
**On ne peut PAS supprimer un élément s'il est utilisé** dans la base de données.

### Vérifications
- **Équipes** : Vérifie si des agents sont assignés à cette équipe (`Agents.equipe_id`)
- **Sites** : Vérifie si des agents sont assignés à ce site (`Agents.site_id`)
- **Types d'équipement** : Vérifie si des équipements utilisent ce type (`Equipements.type_id`)

### Message d'erreur
```
┌──────────────────────────────────────┐
│  ⚠ Suppression impossible            │
├──────────────────────────────────────┤
│  Cet élément est utilisé et ne peut  │
│  pas être supprimé.                  │
│  Vous pouvez le modifier si          │
│  nécessaire.                         │
│                                      │
│              [ OK ]                  │
└──────────────────────────────────────┘
```

### Autorisation de modification
✅ **La modification est toujours autorisée**, même si l'élément est utilisé.

**Cas d'usage** : Correction d'une faute d'orthographe sans avoir à recréer tous les agents/équipements associés.

## Requêtes SQL

### Lister
```sql
SELECT id, name FROM {TableName} ORDER BY name
```

### Ajouter
```sql
INSERT INTO {TableName} (name) VALUES ($name)
```

### Modifier
```sql
UPDATE {TableName} SET name = $name WHERE id = $id
```

### Supprimer
```sql
DELETE FROM {TableName} WHERE id = $id
```

### Vérifier l'utilisation (Équipes)
```sql
SELECT COUNT(*) FROM Agents WHERE equipe_id = $id
```

### Vérifier l'utilisation (Sites)
```sql
SELECT COUNT(*) FROM Agents WHERE site_id = $id
```

### Vérifier l'utilisation (Types d'équipement)
```sql
SELECT COUNT(*) FROM Equipements WHERE type_id = $id
```

## Gestion d'erreurs

### Erreurs capturées
- **SqliteException** : Erreurs de base de données (contraintes, connexion, etc.)
- **Validation** : Nom vide ou null

### Messages
- ✅ **Succès** : "Élément ajouté/modifié/supprimé avec succès."
- ⚠ **Avertissement** : "Veuillez entrer un nom." / "Cet élément est utilisé..."
- ❌ **Erreur** : "Erreur lors de l'ajout/modification/suppression : {détails}"

## Intégration

### WelcomePage.cs
```csharp
private void ShowSettings()
{
    content.Controls.Clear();
    content.Controls.Add(new SettingsView(onBack: ShowAdminMenu) { Dock = DockStyle.Fill });
}
```

### AdminMenuView.cs
```csharp
public AdminMenuView(Action onBack, ..., Action onSettings)
{
    ...
    btnSettings = new Button { Text = "⚙ Paramètres", ... };
    Theme.StyleSecondaryButton(btnSettings, setHeight: false);
    btnSettings.Click += (_, __) => _onSettings?.Invoke();
}
```

## Thème

### Couleurs
- **Panneau liste** : `Theme.Colors.Surface`
- **Panneau formulaire** : `Theme.Colors.Surface`
- **Fond général** : `Theme.Colors.Background`

### Boutons
- **Ajouter** : `Theme.StylePrimaryButton` (bleu)
- **Modifier** : `Theme.StyleSecondaryButton` (gris)
- **Supprimer** : `Theme.StyleDangerButton` (rouge)
- **Retour** : `Theme.StyleOutlineButton` (contour)

### Dimensions
- **Boutons** : `Theme.Sizes.ButtonWidth` x `Theme.Sizes.ButtonHeight` (120x40)
- **Champs texte** : `Height = Theme.Sizes.InputHeight` (36px)

## Évolutions futures possibles

1. **Recherche/Filtrage** : Ajouter un champ de recherche dans la liste
2. **Tri** : Permettre de trier par ID ou par nom
3. **Pagination** : Si la liste devient très longue
4. **Import/Export CSV** : Importer des listes depuis un fichier
5. **Historique** : Garder une trace des modifications
6. **Fusion** : Permettre de fusionner deux équipes/sites/types

## Notes techniques

- **Thread-safe** : Utilise `using var conn` pour une gestion propre des connexions
- **Paramètres sécurisés** : Utilise des paramètres SQL (`$name`, `$id`) contre l'injection SQL
- **Validation** : Vérifie les champs vides avant insertion/modification
- **UX** : Désactive les boutons Edit/Delete quand aucune sélection

---

**Auteur** : GitHub Copilot  
**Date** : 27 octobre 2025  
**Version** : 1.0
