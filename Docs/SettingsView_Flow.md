# Flux de navigation - Paramètres

```
┌─────────────────────────────────────────────────────────────────┐
│                         WelcomePage                             │
│                     (Page d'accueil)                            │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            │ Clic sur "Création / Modification"
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                       AdminMenuView                             │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ← Retour      Administration         ⚙ Paramètres       │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────┬──────────────┐                               │
│  │  Création    │  Création    │                               │
│  │  Équipement  │  Agent       │                               │
│  ├──────────────┼──────────────┤                               │
│  │ Modification │ Modification │                               │
│  │  Équipement  │  Agent       │                               │
│  └──────────────┴──────────────┘                               │
│            │          Échange         │                         │
│            └───────────────────────────┘                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            │ Clic sur "⚙ Paramètres"
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SettingsView                             │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ← Retour              Paramètres                         │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ [Équipes] [Sites] [Types d'équipement]                   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌─────────────────────┬───────────────────────────────────┐  │
│  │      Liste          │        Formulaire                 │  │
│  │  ┌────────────────┐ │  Nom : [_________________]        │  │
│  │  │ ID │ Nom       │ │                                   │  │
│  │  ├────┼───────────┤ │  [Ajouter]                        │  │
│  │  │ 1  │ IT        │ │                                   │  │
│  │  │ 2  │ Finance   │ │  [Modifier]  (si sélection)       │  │
│  │  │ 3  │ RH        │ │  [Supprimer] (si sélection)       │  │
│  │  └────────────────┘ │                                   │  │
│  └─────────────────────┴───────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Fonctionnalités par onglet

### 📋 Onglet "Équipes"
- **Table** : `Equipes`
- **Utilisé par** : `Agents.equipe_id`
- **Exemples** : IT, Finance, RH, Support, Marketing, Ventes

### 🏢 Onglet "Sites"
- **Table** : `Sites`
- **Utilisé par** : `Agents.site_id`
- **Exemples** : Paris, Lyon, Marseille, Toulouse, Bordeaux

### 🖥️ Onglet "Types d'équipement"
- **Table** : `equipment_type`
- **Utilisé par** : `Equipements.type_id`
- **Exemples** : Ordinateur portable, Écran, Souris, Clavier, Casque, Webcam

---

## Matrice des opérations

| Opération  | Équipes | Sites | Types d'équipement | Protection |
|------------|---------|-------|-------------------|------------|
| **Lister** | ✅      | ✅    | ✅                | Aucune     |
| **Ajouter**| ✅      | ✅    | ✅                | Validation nom non vide |
| **Modifier**| ✅     | ✅    | ✅                | Aucune (toujours autorisé) |
| **Supprimer**| ✅    | ✅    | ✅                | ❌ Bloqué si utilisé |

---

## Exemple de scénario : Correction d'une faute

### Problème
L'équipe "Finanse" a été créée avec une faute d'orthographe.

### Solution (AVANT ce système)
1. ❌ Impossible de modifier directement
2. ❌ Impossible de supprimer (utilisée par 50 agents)
3. ❌ Solution : Créer "Finance", réassigner 50 agents, supprimer "Finanse"
4. ❌ **Temps perdu : ~30 minutes**

### Solution (AVEC ce système)
1. ✅ Aller dans Paramètres → Équipes
2. ✅ Sélectionner "Finanse"
3. ✅ Modifier en "Finance"
4. ✅ Cliquer "Modifier"
5. ✅ **Temps : 10 secondes** 🚀

Tous les 50 agents sont automatiquement mis à jour (foreign key) !

---

## Schéma de la base de données (relations)

```
┌──────────────┐         ┌──────────────┐
│   Equipes    │◄───────┐│    Agents    │
├──────────────┤        ││              │
│ id (PK)      │        │├──────────────┤
│ name         │        ││ idrh (PK)    │
└──────────────┘        ││ equipe_id FK │◄───┐
                        ││ site_id FK   │◄─┐ │
                        │└──────────────┘  │ │
                        │                  │ │
┌──────────────┐        │                  │ │
│    Sites     │◄───────┘                  │ │
├──────────────┤                           │ │
│ id (PK)      │                           │ │
│ name         │                           │ │
└──────────────┘                           │ │
                                           │ │
┌──────────────┐         ┌──────────────┐ │ │
│equipment_type│◄────────│ Equipements  │ │ │
├──────────────┤         ├──────────────┤ │ │
│ id (PK)      │         │ id_equip (PK)│ │ │
│ name         │         │ type_id FK   │ │ │
└──────────────┘         │ idrh FK ─────┼─┘ │
                         └──────────────┘   │
                                            │
                        ┌──────────────┐    │
                        │    Prets     │    │
                        ├──────────────┤    │
                        │ id (PK)      │    │
                        │ idrh FK ─────┼────┘
                        │ date_debut   │
                        │ date_fin     │
                        └──────────────┘
```

**Légende :**
- `PK` : Primary Key (Clé primaire)
- `FK` : Foreign Key (Clé étrangère)
- `◄───` : Relation (un-à-plusieurs)

---

## Impact de la suppression bloquée

### Exemple : Tentative de suppression de "IT"

#### État de la base
```
Equipes:
  id=1, name="IT"     ← On veut supprimer ceci
  id=2, name="Finance"
  id=3, name="RH"

Agents:
  idrh=100, nom="Dupont", equipe_id=1  ← Référence IT
  idrh=101, nom="Martin", equipe_id=1  ← Référence IT
  idrh=102, nom="Durand", equipe_id=2
```

#### Résultat
```sql
SELECT COUNT(*) FROM Agents WHERE equipe_id = 1
-- Résultat : 2  (> 0, donc UTILISÉ)
```

**Message affiché :**
```
⚠ Suppression impossible

Cet élément est utilisé et ne peut pas être supprimé.
Vous pouvez le modifier si nécessaire.
```

#### Solutions possibles
1. ✅ **Modifier** le nom "IT" en "Informatique" (autorisé)
2. ✅ **Réassigner** Dupont et Martin à une autre équipe, puis supprimer
3. ❌ **Supprimer directement** (bloqué par la protection)
