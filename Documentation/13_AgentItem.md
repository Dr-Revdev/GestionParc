# 📋 AgentItem.cs - Classe modèle pour les agents

## 🎯 But du fichier

Ce fichier définit une **classe modèle** simple pour représenter un agent dans les listes déroulantes (ComboBox) de l'interface.

**Analogie :** C'est comme une **carte d'identité simplifiée** qui contient juste l'essentiel : un identifiant et un nom à afficher.

---

## 📋 Code complet avec numéros de ligne

```csharp
1   namespace ProjetParc.Views.Loan.Models;
2   
3   /// <summary>
4   /// Représente un agent sélectionnable dans l'interface
5   /// </summary>
6   public class AgentItem
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
/// Représente un agent sélectionnable dans l'interface
/// </summary>
```

**Explication :**
- **`/// <summary>`** = Commentaire spécial pour la documentation
- Apparaît quand tu survoles `AgentItem` dans le code
- Aide les autres développeurs à comprendre à quoi sert la classe

**Analogie :** C'est comme l'étiquette sur une boîte qui explique ce qu'il y a dedans.

---

### 🏗️ Ligne 6 : Déclaration de la classe
```csharp
public class AgentItem
```

**Explication :**
- **`public`** = Accessible de partout dans le code
- **`class`** = C'est un modèle/plan de construction
- **`AgentItem`** = Nom de la classe (convention : commence par une majuscule)

**Analogie :** C'est le plan de construction d'une carte d'identité pour un agent.

---

### 🔑 Ligne 8 : Propriété Id
```csharp
public string Id { get; set; }
```

**Explication :**
- **`public string Id`** = Propriété accessible partout, de type texte
- **`{ get; set; }`** = On peut lire (`get`) et modifier (`set`) cette propriété
- **`Id`** = Identifiant unique de l'agent (vient de la base de données)

**Analogie :** C'est le numéro d'identification sur la carte (comme un numéro de Sécurité Sociale).

---

### 📛 Ligne 9 : Propriété DisplayName
```csharp
public string DisplayName { get; set; }
```

**Explication :**
- **`DisplayName`** = Nom à afficher dans l'interface (ex: "DUPONT Jean (IT)")
- Généralement construit à partir du nom, prénom et service de l'agent
- C'est ce que l'utilisateur verra dans la liste déroulante

**Analogie :** C'est le nom complet écrit sur la carte, facile à lire.

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
Quand tu ajoutes un `AgentItem` à une ComboBox, Windows Forms appelle automatiquement `ToString()` pour savoir quoi afficher. Grâce à cette ligne, c'est le nom complet qui s'affiche !

**Analogie :** Quand quelqu'un te demande "qui es-tu ?", tu réponds avec ton nom complet, pas juste ton numéro.

---

## 🎬 Scénarios d'utilisation

### Scénario 1 : Création d'un AgentItem depuis la base de données
```csharp
// Dans LoanCreationView.cs
using (var cmd = new SqliteCommand(@"
    SELECT id_agent, nom, prenom, nom_service 
    FROM Agent 
    INNER JOIN Travail ON Agent.id_agent = Travail.agent_id
    INNER JOIN Service ON Travail.service_id = Service.id_service", 
    DataBase.Connection))
{
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            // Création d'un AgentItem
            var agent = new AgentItem
            {
                Id = reader.GetString(0),  // id_agent
                DisplayName = $"{reader.GetString(1)} {reader.GetString(2)} ({reader.GetString(3)})"
                              // ex: "DUPONT Jean (IT)"
            };
            
            agentComboBox.Items.Add(agent);
        }
    }
}
```

**Résultat :** La ComboBox affiche "DUPONT Jean (IT)" grâce à `ToString()`.

---

### Scénario 2 : Récupération de l'agent sélectionné
```csharp
// Quand l'utilisateur sélectionne un agent
private void AgentComboBox_SelectedIndexChanged(object sender, EventArgs e)
{
    var selectedAgent = (AgentItem)agentComboBox.SelectedItem;
    
    // On peut maintenant utiliser l'ID pour faire des requêtes
    string agentId = selectedAgent.Id;
    
    // Par exemple, charger les prêts de cet agent
    LoadAgentLoans(agentId);
}
```

---

### Scénario 3 : Comparaison d'agents
```csharp
// Vérifier si deux ComboBox ont le même agent
AgentItem agent1 = (AgentItem)comboBox1.SelectedItem;
AgentItem agent2 = (AgentItem)comboBox2.SelectedItem;

if (agent1.Id == agent2.Id)
{
    MessageBox.Show("C'est le même agent !");
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

Par défaut, `ToString()` retourne le nom de la classe ("ProjetParc.Views.Loan.Models.AgentItem"). Pas très utile !

En overridant, on dit : "Quand tu affiches cet objet, montre le nom complet de l'agent."

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

**Solution :** `AgentItem` contient l'ID (pour les requêtes SQL) ET le nom à afficher (pour l'utilisateur).

---

### ✅ Alternative sans AgentItem

Sans `AgentItem`, tu pourrais :
```csharp
// Mauvais : juste des strings
comboBox.Items.Add("DUPONT Jean");

// Problème : comment récupérer l'ID ensuite ?
string selected = (string)comboBox.SelectedItem;
// On a perdu l'ID ! On doit refaire une requête SQL...
```

Avec `AgentItem` :
```csharp
// Bon : on a l'ID et le nom
comboBox.Items.Add(new AgentItem { Id = "123", DisplayName = "DUPONT Jean" });

// Facile de récupérer l'ID
var selected = (AgentItem)comboBox.SelectedItem;
string id = selected.Id;  // ✅ On a l'ID directement !
```

---

### ✅ Pourquoi "Item" dans le nom ?

Convention de nommage :
- **`Agent`** = Classe complète avec toutes les propriétés
- **`AgentItem`** = Version simplifiée pour l'affichage dans les listes

---

### ⚠️ Attention : Null Reference

Si tu récupères `SelectedItem` sans vérifier :
```csharp
// ❌ Dangereux !
var agent = (AgentItem)comboBox.SelectedItem;
string id = agent.Id;  // Exception si rien n'est sélectionné !
```

Toujours vérifier :
```csharp
// ✅ Sûr
if (comboBox.SelectedItem is AgentItem agent)
{
    string id = agent.Id;
}
```

---

## 🔗 Fichiers liés

- **[LoanCreationView.cs](10_LoanCreationView.md)** → Utilise `AgentItem` pour la ComboBox des agents
- **[EquipmentItem.cs](14_EquipmentItem.md)** → Classe similaire pour les équipements
- **[AgentCreateView.cs](05_AgentCreateView.md)** → Crée les agents dans la base de données
- **[AgentEditView.cs](06_AgentEditView.md)** → Modifie les agents existants

---

## 🤔 Questions fréquentes

**Q : Pourquoi ne pas mettre plus de propriétés (email, téléphone, etc.) ?**
- R : `AgentItem` est conçu pour les **listes déroulantes**. On veut juste l'ID et le nom. Pour plus d'infos, on ferait une autre requête SQL avec l'ID.

**Q : Peut-on modifier Id ou DisplayName après la création ?**
- R : Oui, avec `{ get; set; }`. Mais généralement on ne le fait pas, car ces valeurs viennent de la base de données.

**Q : Pourquoi pas un record au lieu d'une classe ?**
- R : Les `record` sont plus modernes (C# 9+) et conviendraient bien ici. Mais une `class` fonctionne parfaitement aussi.

**Q : Cette classe est réutilisable ailleurs ?**
- R : Oui ! Partout où tu as une ComboBox avec des agents. C'est le but d'une classe modèle.

---

**💡 En résumé :**
`AgentItem` est une **carte d'identité simplifiée** pour afficher des agents dans des listes déroulantes, en gardant l'ID pour les requêtes SQL.

---

*Durée de lecture : ~15 minutes*  
*Niveau : Débutant-Intermédiaire*  
*Dernière mise à jour : 22 octobre 2025*
