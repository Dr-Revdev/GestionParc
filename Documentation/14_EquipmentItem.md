# 📋 EquipmentItem.cs - Classe modèle pour les équipements

## 🎯 But du fichier

Ce fichier définit une **classe modèle** simple pour représenter un équipement dans les listes déroulantes (ComboBox) et les contrôles de sélection.

**Analogie :** C'est comme une **étiquette d'inventaire simplifiée** qui contient juste l'essentiel : un identifiant et un nom à afficher.

---

## 📋 Code complet avec numéros de ligne

```csharp
1   namespace ProjetParc.Views.Loan.Models;
2   
3   /// <summary>
4   /// Représente un équipement sélectionnable dans l'interface
5   /// </summary>
6   public class EquipmentItem
7   {
8       public string Id { get; set; }
9       public string DisplayName { get; set; }
10      public override string ToString() => DisplayName;
11  }
```

---

## 📦 Explications groupées par section

### 🏷️ Ligne 1 : Namespace (Espace de noms)
```csharp
namespace ProjetParc.Views.Loan.Models;
```

**Explication :**
- **`namespace`** = Organisation du code (comme une adresse postale)
- **`ProjetParc.Views.Loan.Models`** = Chemin : Projet → Vues → Prêts → Modèles
- Le **point-virgule** à la fin = syntaxe moderne C# (déclaration de namespace sur une ligne)

**Analogie :** C'est comme dire "ce fichier se trouve dans le tiroir Modèles du classeur Prêts".

---

### 📝 Lignes 3-5 : Commentaire de documentation
```csharp
/// <summary>
/// Représente un équipement sélectionnable dans l'interface
/// </summary>
```

**Explication :**
- **`/// <summary>`** = Commentaire spécial pour la documentation
- Apparaît quand tu survoles `EquipmentItem` dans le code
- Aide les autres développeurs à comprendre à quoi sert la classe

**Analogie :** C'est comme l'étiquette sur un carton qui explique ce qu'il contient.

---

### 🏗️ Ligne 6 : Déclaration de la classe
```csharp
public class EquipmentItem
```

**Explication :**
- **`public`** = Accessible de partout dans le code
- **`class`** = C'est un modèle/plan de construction
- **`EquipmentItem`** = Nom de la classe (convention : commence par une majuscule)

**Analogie :** C'est le plan de construction d'une étiquette d'inventaire pour un équipement.

---

### 🔑 Ligne 8 : Propriété Id
```csharp
public string Id { get; set; }
```

**Explication :**
- **`public string Id`** = Propriété accessible partout, de type texte
- **`{ get; set; }`** = On peut lire (`get`) et modifier (`set`) cette propriété
- **`Id`** = Identifiant unique de l'équipement (GUID de la base de données)

**Analogie :** C'est le code-barres ou numéro de série sur l'étiquette.

---

### 📛 Ligne 9 : Propriété DisplayName
```csharp
public string DisplayName { get; set; }
```

**Explication :**
- **`DisplayName`** = Nom à afficher dans l'interface (ex: "PC Portable - Dell Latitude - SN:ABC123")
- Généralement construit à partir du type, modèle et numéro de série
- C'est ce que l'utilisateur verra dans la liste déroulante

**Analogie :** C'est la description lisible écrite sur l'étiquette.

---

### 🔍 Ligne 10 : Méthode ToString()
```csharp
public override string ToString() => DisplayName;
```

**Explication :**
- **`override`** = Remplace la méthode par défaut de la classe de base
- **`ToString()`** = Méthode appelée automatiquement pour convertir l'objet en texte
- **`=> DisplayName`** = Syntaxe courte (lambda) qui retourne `DisplayName`

**Pourquoi c'est important :**
Quand tu ajoutes un `EquipmentItem` à une ComboBox, Windows Forms appelle automatiquement `ToString()` pour savoir quoi afficher. Grâce à cette ligne, c'est la description complète qui s'affiche !

**Analogie :** Quand quelqu'un te demande "c'est quoi cet équipement ?", tu réponds avec sa description complète, pas juste son code-barres.

---

## 🎬 Scénarios d'utilisation

### Scénario 1 : Création d'un EquipmentItem depuis la base de données
```csharp
// Dans LoanCreationView.cs ou EquipmentSelectionControl.cs
using (var cmd = new SqliteCommand(@"
    SELECT id_equipement, type_equipement, model, n_serie 
    FROM Equipement 
    WHERE etat_pret = 0", 
    DataBase.Connection))
{
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            // Création d'un EquipmentItem
            var equipment = new EquipmentItem
            {
                Id = reader.GetString(0),  // id_equipement (GUID)
                DisplayName = $"{reader.GetString(1)} - {reader.GetString(2)} - SN:{reader.GetString(3)}"
                              // ex: "PC Portable - Dell Latitude - SN:ABC123"
            };
            
            comboBox.Items.Add(equipment);
        }
    }
}
```

**Résultat :** La ComboBox affiche "PC Portable - Dell Latitude - SN:ABC123" grâce à `ToString()`.

---

### Scénario 2 : Récupération de l'équipement sélectionné
```csharp
// Quand l'utilisateur sélectionne un équipement
private void EquipmentComboBox_SelectedIndexChanged(object sender, EventArgs e)
{
    var selectedEquipment = (EquipmentItem)equipmentComboBox.SelectedItem;
    
    // On peut maintenant utiliser l'ID pour faire des requêtes
    string equipmentId = selectedEquipment.Id;
    
    // Par exemple, vérifier si l'équipement est disponible
    CheckEquipmentAvailability(equipmentId);
}
```

---

### Scénario 3 : Utilisation dans EquipmentSelectionControl
```csharp
// Dans EquipmentSelectionControl.cs
public void SetEquipment(string equipmentId)
{
    // Trouver l'EquipmentItem correspondant dans la ComboBox
    foreach (EquipmentItem item in equipmentComboBox.Items)
    {
        if (item.Id == equipmentId)
        {
            equipmentComboBox.SelectedItem = item;
            break;
        }
    }
}
```

---

### Scénario 4 : Comparaison d'équipements
```csharp
// Vérifier si deux contrôles ont le même équipement sélectionné
EquipmentItem eq1 = (EquipmentItem)control1.SelectedEquipment;
EquipmentItem eq2 = (EquipmentItem)control2.SelectedEquipment;

if (eq1.Id == eq2.Id)
{
    MessageBox.Show("Vous avez sélectionné le même équipement deux fois !");
}
```

---

## 🎓 Concepts clés

### 1. Classe modèle (POCO)
**POCO** = Plain Old CLR Object (Objet Simple)

Une classe modèle est une classe simple qui contient juste des propriétés, sans logique complexe. Elle sert à **transporter des données**.

**Avantages :**
- Facile à comprendre
- Facile à tester
- Réutilisable partout

---

### 2. Propriétés auto-implémentées
```csharp
public string Id { get; set; }
```

C'est un raccourci pour :
```csharp
private string _id;
public string Id 
{ 
    get { return _id; } 
    set { _id = value; } 
}
```

---

### 3. Override ToString()
**Pourquoi overrider `ToString()` ?**

Par défaut, `ToString()` retourne le nom de la classe ("ProjetParc.Views.Loan.Models.EquipmentItem"). Pas très utile !

En overridant, on dit : "Quand tu affiches cet objet, montre la description complète de l'équipement."

---

### 4. Expression Lambda (=>)
```csharp
public override string ToString() => DisplayName;
```

C'est équivalent à :
```csharp
public override string ToString()
{
    return DisplayName;
}
```

La syntaxe `=>` est plus courte et élégante pour les méthodes simples.

---

## 💡 Points importants

### ✅ Pourquoi cette classe existe ?

**Problème :** Une ComboBox peut contenir n'importe quel type d'objet, mais par défaut elle affiche le nom de la classe.

**Solution :** `EquipmentItem` contient l'ID (pour les requêtes SQL) ET la description (pour l'utilisateur).

---

### ✅ Alternative sans EquipmentItem

Sans `EquipmentItem`, tu pourrais :
```csharp
// Mauvais : juste des strings
comboBox.Items.Add("PC Portable - Dell Latitude");

// Problème : comment récupérer l'ID (GUID) ensuite ?
string selected = (string)comboBox.SelectedItem;
// On a perdu l'ID ! On doit refaire une requête SQL...
```

Avec `EquipmentItem` :
```csharp
// Bon : on a l'ID et la description
comboBox.Items.Add(new EquipmentItem 
{ 
    Id = "a1b2c3d4-...", 
    DisplayName = "PC Portable - Dell Latitude" 
});

// Facile de récupérer l'ID
var selected = (EquipmentItem)comboBox.SelectedItem;
string id = selected.Id;  // ✅ On a le GUID directement !
```

---

### ✅ Pourquoi "Item" dans le nom ?

Convention de nommage :
- **`Equipment`** = Classe complète avec toutes les propriétés (type, modèle, état, etc.)
- **`EquipmentItem`** = Version simplifiée pour l'affichage dans les listes

---

### ✅ Format typique de DisplayName

Dans ce projet, `DisplayName` suit généralement ce format :
```
[Type] - [Modèle] - SN:[Numéro de série]
```

Exemples :
- "PC Portable - Dell Latitude - SN:ABC123"
- "Écran - Samsung 24 - SN:XYZ789"
- "Clavier - Logitech K120 - SN:LGT456"

---

### ⚠️ Attention : Null Reference

Si tu récupères `SelectedItem` sans vérifier :
```csharp
// ❌ Dangereux !
var equipment = (EquipmentItem)comboBox.SelectedItem;
string id = equipment.Id;  // Exception si rien n'est sélectionné !
```

Toujours vérifier :
```csharp
// ✅ Sûr
if (comboBox.SelectedItem is EquipmentItem equipment)
{
    string id = equipment.Id;
}
```

---

## 🔗 Fichiers liés

- **[LoanCreationView.cs](10_LoanCreationView.md)** → Utilise `EquipmentItem` indirectement via `EquipmentSelectionControl`
- **[EquipmentSelectionControl.cs](11_EquipmentSelectionControl.md)** → Utilise `EquipmentItem` pour la ComboBox
- **[AgentItem.cs](13_AgentItem.md)** → Classe similaire pour les agents
- **[EquipmentCreateView.cs](07_EquipmentCreateView.md)** → Crée les équipements dans la base de données
- **[EquipmentEditView.cs](08_EquipmentEditView.md)** → Modifie les équipements existants
- **[FreeEquipmentView.cs](09_FreeEquipmentView.md)** → Affiche les équipements disponibles

---

## 🤔 Questions fréquentes

**Q : Pourquoi ne pas mettre plus de propriétés (état, date d'achat, etc.) ?**
- R : `EquipmentItem` est conçu pour les **listes déroulantes**. On veut juste l'ID et la description. Pour plus d'infos, on ferait une autre requête SQL avec l'ID.

**Q : Peut-on modifier Id ou DisplayName après la création ?**
- R : Oui, avec `{ get; set; }`. Mais généralement on ne le fait pas, car ces valeurs viennent de la base de données et restent fixes.

**Q : Pourquoi pas un record au lieu d'une classe ?**
- R : Les `record` sont plus modernes (C# 9+) et conviendraient bien ici (données immuables). Mais une `class` fonctionne parfaitement aussi.

**Q : Cette classe est réutilisable ailleurs ?**
- R : Oui ! Partout où tu as une ComboBox ou une ListBox avec des équipements. C'est le but d'une classe modèle.

**Q : Que se passe-t-il si DisplayName est très long ?**
- R : La ComboBox affiche le texte complet, même s'il est coupé visuellement. Tu peux ajouter un tooltip pour voir le texte entier.

---

**💡 En résumé :**
`EquipmentItem` est une **étiquette d'inventaire simplifiée** pour afficher des équipements dans des listes déroulantes, en gardant l'ID (GUID) pour les requêtes SQL.

---

*Durée de lecture : ~15 minutes*  
*Niveau : Débutant-Intermédiaire*  
*Dernière mise à jour : 24 octobre 2025*
