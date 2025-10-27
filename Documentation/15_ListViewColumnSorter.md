# ListViewColumnSorter.cs

## Vue d'ensemble
Classe utilitaire permettant de trier les colonnes d'un `ListView` par ordre alphabétique ou numérique. Implémente `IComparer` pour être utilisée directement avec la propriété `ListViewItemSorter`.

## Namespace
`ProjetParc.Views`

## Fonctionnalités principales

### 1. **Tri intelligent**
- **Tri numérique** : Détecte automatiquement les colonnes contenant des nombres et effectue un tri numérique
- **Tri alphabétique** : Pour les textes, tri insensible à la casse (A = a)
- **Tri réversible** : Premier clic = croissant, deuxième clic = décroissant

### 2. **Intégration facile**
```csharp
// Initialisation
var sorter = new ListViewColumnSorter();
listView.ListViewItemSorter = sorter;

// Gestion du clic sur colonne
listView.ColumnClick += (s, e) => {
    sorter.SetSortColumn(e.Column);
    listView.Sort();
};
```

## Propriétés

| Propriété | Type | Description |
|-----------|------|-------------|
| `Order` | `SortOrder` | Ordre de tri actuel (`Ascending`, `Descending`, `None`) |
| `SortColumn` | `int` | Index de la colonne actuellement triée |

## Méthodes

### `Compare(object x, object y) : int`
Implémente l'interface `IComparer`. Compare deux `ListViewItem` selon la colonne et l'ordre définis.

**Retour** :
- `< 0` : x précède y
- `= 0` : x égal à y
- `> 0` : x suit y

### `SetSortColumn(int column) : void`
Définit la colonne à trier et gère l'inversion de l'ordre automatiquement.

**Paramètres** :
- `column` : Index de la colonne à trier (0-based)

**Comportement** :
- Si nouvelle colonne → Tri croissant
- Si même colonne → Inverse l'ordre (croissant ↔ décroissant)

## Logique de tri

```
┌─────────────────────────────┐
│   Clic sur colonne N        │
└──────────┬──────────────────┘
           │
           ├─→ Nouvelle colonne ?
           │   └─→ OUI : Tri croissant sur colonne N
           │
           └─→ Même colonne ?
               └─→ OUI : Inverse l'ordre actuel
                         (croissant → décroissant)
                         (décroissant → croissant)
```

## Algorithme de comparaison

```csharp
1. Extraire le texte de la colonne pour les deux items
2. Tenter conversion en nombre (int.TryParse)
   ├─→ Succès pour les 2 : Comparaison numérique
   └─→ Échec : Comparaison alphabétique (insensible casse)
3. Appliquer l'ordre (inverse si décroissant)
4. Retourner le résultat
```

## Exemple d'utilisation

### Cas simple
```csharp
private ListView lvData;
private ListViewColumnSorter lvSorter;

private void InitializeListView()
{
    lvData = new ListView
    {
        View = View.Details,
        FullRowSelect = true
    };
    
    lvData.Columns.Add("ID", 60);
    lvData.Columns.Add("Nom", 200);
    lvData.Columns.Add("Age", 80);
    
    // Configuration du tri
    lvSorter = new ListViewColumnSorter();
    lvData.ListViewItemSorter = lvSorter;
    lvData.ColumnClick += (s, e) => {
        lvSorter.SetSortColumn(e.Column);
        lvData.Sort();
    };
}
```

### Cas avec données mixtes
```csharp
// Colonnes : ID (numérique) | Nom (texte) | Code (mixte)
lvData.Items.Add(new ListViewItem(new[] { "10", "Alice", "A123" }));
lvData.Items.Add(new ListViewItem(new[] { "2", "Bob", "B45" }));
lvData.Items.Add(new ListViewItem(new[] { "100", "Charlie", "C789" }));

// Tri sur ID : 2, 10, 100 (numérique)
// Tri sur Nom : Alice, Bob, Charlie (alphabétique)
// Tri sur Code : A123, B45, C789 (alphabétique car mixte)
```

## Vues utilisant cette classe

1. **FreeEquipmentView.cs** : 2 tableaux (Disponibles + Rendus DSEM)
2. **AgentEditView.cs** : Liste des agents
3. **EquipmentEditView.cs** : Liste des équipements
4. **MainInventoryView.cs** : Vue d'inventaire principal
5. **SettingsView.cs** : Gestion Équipes/Sites/Types

## Améliorations possibles

- [ ] Support du tri par date (DateTime.TryParse)
- [ ] Indicateur visuel de la colonne triée (flèche ▲/▼)
- [ ] Tri multi-colonnes (Shift+Clic)
- [ ] Tri naturel alphanumérique (A1, A2, A10 au lieu de A1, A10, A2)
- [ ] Mémorisation de l'ordre de tri entre les sessions

## Notes techniques

- **Performance** : Utilise `string.Compare` avec `OrdinalIgnoreCase` pour optimiser le tri
- **Sécurité** : Vérifie les types avant conversion pour éviter les exceptions
- **Compatibilité** : WinForms .NET 9.0+
