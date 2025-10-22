# 📘 Documentation de Program.cs

## 🎯 But de ce fichier
Ce fichier est le **point d'entrée** de l'application. C'est comme la porte principale d'une maison : c'est par là que tout commence quand vous lancez le programme.

---

## � Le code complet

```csharp
1   using System;
2   using System.Windows.Forms;
3   using ProjetParc.Data;
4   using ProjetParc.Views;
5
6   namespace ProjetParc;
7
8   static class Program
9   {
10      [STAThread]
11      static void Main()
12      {
13          try
14          {
15              Database.EnsureInitialized();
16              ApplicationConfiguration.Initialize();
17              Application.Run(new WelcomePage());
18          }
19          catch (Exception ex)
20          {
21              MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
22          }
23      }
24  }
```

---

## 📦 Lignes 1-4 : Les imports (using)

```csharp
1   using System;
2   using System.Windows.Forms;
3   using ProjetParc.Data;
4   using ProjetParc.Views;
```

**`using`** = Importer des outils dont on a besoin

### **Ligne 1 : `using System;`**
- Les outils de base de C# (gestion d'erreurs, types de base, etc.)

### **Ligne 2 : `using System.Windows.Forms;`**
- Les outils pour créer des fenêtres graphiques (boutons, formulaires, etc.)
- Sans ça, impossible de créer l'interface utilisateur

### **Ligne 3 : `using ProjetParc.Data;`**
- Notre propre code pour gérer la base de données
- Permet d'utiliser la classe `Database`

### **Ligne 4 : `using ProjetParc.Views;`**
- Notre propre code pour les écrans de l'application
- Permet d'utiliser `WelcomePage`

💡 **Analogie :** C'est comme ouvrir les boîtes à outils dont tu auras besoin avant de commencer un travail.

---

## 🏷️ Ligne 6 : Le namespace

```csharp
6   namespace ProjetParc;
```

**namespace** = L'adresse de ce fichier dans le projet

- C'est comme l'adresse postale du code
- **ProjetParc** = le nom de notre projet
- Évite les confusions si deux fichiers ont le même nom de classe

---

## �️ Lignes 8-9 : La classe Program

```csharp
8   static class Program
9   {
```

### **`static class Program`**

**`static`**
- Pas besoin de créer une copie de cette classe
- On l'utilise directement

**`class Program`**
- Le conteneur principal de notre application
- Tous les programmes C# ont une classe avec une méthode `Main`

💡 **Analogie :** C'est le chef d'orchestre qui coordonne tout.

---

## 🚀 Lignes 10-12 : La méthode Main (point de départ)

```csharp
10      [STAThread]
11      static void Main()
12      {
```

### **Ligne 10 : `[STAThread]`**
- Instruction spéciale pour Windows
- Configure l'application pour qu'elle tourne correctement
- Obligatoire pour les applications Windows graphiques

### **Ligne 11 : `static void Main()`**

**`static`** - Méthode accessible directement (pas besoin de créer un objet)

**`void`** - Cette méthode ne retourne rien (elle fait juste des actions)

**`Main()`** - **LE** point d'entrée obligatoire
- Tous les programmes C# cherchent une méthode appelée `Main` au démarrage
- C'est le premier code qui s'exécute

💡 **Important :** Sans `Main()`, le programme ne peut pas démarrer !

---

## 🛡️ Lignes 13-22 : Protection contre les erreurs (try-catch)

```csharp
13          try
14          {
15              Database.EnsureInitialized();
16              ApplicationConfiguration.Initialize();
17              Application.Run(new WelcomePage());
18          }
19          catch (Exception ex)
20          {
21              MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
22          }
```

### **Structure try-catch**

**`try { ... }`** - "Essaie de faire ça..."
**`catch { ... }`** - "...mais si ça échoue, fais ça à la place"

💡 **Analogie :** Comme avoir un parachute de secours en cas de problème.

---

### **Ligne 15 : Préparer la base de données**

```csharp
15              Database.EnsureInitialized();
```

**Ce que ça fait :**
1. Vérifie que le dossier `database` existe (le crée si besoin)
2. Vérifie que le fichier de base de données existe (le crée si besoin)
3. Prépare tout pour que l'application puisse lire/écrire des données

💡 **Classe définie dans :** `Data/DataBase.cs`

---

### **Ligne 16 : Configurer l'interface Windows**

```csharp
16              ApplicationConfiguration.Initialize();
```

**Ce que ça fait :**
- Active les styles modernes de Windows
- Configure le rendu du texte pour qu'il soit net
- Prépare l'application pour afficher des fenêtres

---

### **Ligne 17 : Lancer l'application**

```csharp
17              Application.Run(new WelcomePage());
```

**Décomposition :**

**`new WelcomePage()`**
- Crée la fenêtre principale de l'application
- `WelcomePage` est définie dans `Views/WelcomePage.cs`

**`Application.Run(...)`**
- Démarre la boucle principale de l'application
- Le programme reste ouvert et attend que l'utilisateur interagisse
- Ne se termine que quand l'utilisateur ferme la fenêtre

💡 **Résultat :** Une fenêtre s'ouvre et l'utilisateur peut utiliser l'application !

---

### **Lignes 19-22 : Si une erreur arrive**

```csharp
19          catch (Exception ex)
20          {
21              MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
22          }
```

**`catch (Exception ex)`**
- Attrape n'importe quelle erreur qui se produit dans le bloc `try`
- `ex` contient les détails de l'erreur

**`MessageBox.Show(...)`**
- Affiche une fenêtre popup avec le message d'erreur
- L'utilisateur voit ce qui s'est mal passé au lieu d'un crash silencieux

💡 **Sans ce catch :** Si une erreur arrive, l'application planterait sans explication.

---

## 🎬 Résumé du flux d'exécution

Voici ce qui se passe quand tu lances l'application :

```
1. 🚀 Le programme démarre
   ↓
2. 🔍 Windows cherche la méthode Main()
   ↓
3. 🛡️ Le bloc "try" commence
   ↓
4. 💾 Vérification/Création de la base de données
   ↓
5. ⚙️ Configuration de l'interface Windows
   ↓
6. 🏠 Ouverture de la page d'accueil (WelcomePage)
   ↓
7. ⏳ Le programme tourne en boucle
   ↓
8. 👤 L'utilisateur interagit avec l'application
   ↓
9. ❌ Si une erreur arrive → affichage d'un message
   ↓
10. 🔚 L'utilisateur ferme la fenêtre → le programme se termine
```

---

## 🎓 Concepts clés à retenir

### 1. **Point d'entrée**
- Tout programme C# commence par la méthode `Main()`
- C'est obligatoire, sans elle le programme ne peut pas démarrer

### 2. **Gestion des erreurs**
- `try-catch` protège contre les plantages
- Si quelque chose échoue, on peut réagir proprement

### 3. **Ordre d'exécution**
- L'ordre des instructions est crucial
- On doit initialiser la base de données AVANT d'ouvrir la fenêtre

### 4. **Séparation des responsabilités**
- `Program.cs` se contente de démarrer l'application
- La logique métier est ailleurs (dans les autres fichiers)

---

## 💡 Questions fréquentes

**Q : Pourquoi `static` partout ?**
- R : La méthode `Main` doit être `static` car elle est appelée avant qu'aucun objet ne soit créé. C'est une règle de C#.

**Q : Que se passe-t-il si on retire le try-catch ?**
- R : Si une erreur se produit au démarrage, l'application va planter sans explication. L'utilisateur verra juste que le programme s'est fermé.

**Q : Peut-on avoir plusieurs méthodes Main() ?**
- R : Non, il ne peut y avoir qu'une seule méthode `Main` par programme. C'est le point d'entrée unique.

**Q : Pourquoi new WelcomePage() ?**
- R : `new` crée une instance (une copie) de la classe. C'est comme construire une maison à partir du plan architectural.

---

## 🔗 Fichiers liés

- `Data/DataBase.cs` - Gère la connexion à la base de données
- `Views/WelcomePage.cs` - La page d'accueil de l'application

---

**📌 Prochaine étape :** Consulter la documentation de `DataBase.cs` pour comprendre comment fonctionne la base de données.
