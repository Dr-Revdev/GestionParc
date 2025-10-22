# 📘 Documentation de DataBase.cs# 📘 Documentation de DataBase.cs



## 🎯 But de ce fichier## 🎯 But de ce fichier

Ce fichier gère **toutes les connexions à la base de données**. C'est le gardien qui ouvre la porte de la base de données et s'assure qu'elle est correctement configurée.Ce fichier est le **gardien de la base de données**. Il gère tout ce qui concerne l'accès aux données : créer le dossier, ouvrir les connexions, et configurer la base de données.



💡 **Analogie :** C'est comme le bibliothécaire qui gère l'accès aux livres et s'assure que la bibliothèque existe.**Analogie :** C'est comme le bibliothécaire qui gère l'accès aux livres, s'assure que la bibliothèque existe, et configure les règles d'emprunt.



------



## 📋 Le code complet## 📦 Les "using" - Importer des outils



```csharp```csharp

1   using System.IO;using System.IO;

2   using Microsoft.Data.Sqlite;using Microsoft.Data.Sqlite;

3```

4   namespace ProjetParc.Data;

5### 🔍 Explication :

6   /// <summary>

7   /// Classe statique gérant les connexions à la base de données SQLite**`using System.IO;`**

8   /// </summary>- `IO` = Input/Output (Entrée/Sortie)

9   public static class Database- C'est la boîte à outils pour gérer les **fichiers et dossiers**

10  {- Permet de :

11      private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");  - Créer des dossiers (`Directory.CreateDirectory`)

12      private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");  - Vérifier si un fichier existe (`File.Exists`)

13  - Lire/écrire des fichiers

14      /// <summary>

15      /// Ouvre une nouvelle connexion à la base de données SQLite**`using Microsoft.Data.Sqlite;`**

16      /// </summary>- C'est la bibliothèque pour travailler avec **SQLite**

17      /// <returns>Une connexion SQLite ouverte et configurée</returns>- **SQLite** = Un type de base de données très simple et légère

18      public static SqliteConnection Open()- Stocke toutes les données dans un seul fichier (`.db`)

19      {- Parfait pour les petites applications

20          Directory.CreateDirectory(DataDir);

21          var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");---

22          connexion.Open();

23          using var pragma = connexion.CreateCommand();## 🏷️ Le namespace

24          pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";

25          pragma.ExecuteNonQuery();```csharp

26          return connexion;namespace ProjetParc.Data;

27      }```

28

29      /// <summary>**Explication :**

30      /// S'assure que la base de données est initialisée et que le répertoire existe- `ProjetParc` = Notre projet

31      /// </summary>- `Data` = Le sous-dossier/catégorie pour tout ce qui concerne les données

32      public static void EnsureInitialized()- Adresse complète : `ProjetParc.Data.Database`

33      {

34          Directory.CreateDirectory(DataDir);---

35          if (!File.Exists(DbPath))

36          {## 💬 Commentaire XML de documentation

37              using var _ = Open();

38          }```csharp

39      }/// <summary>

40  }/// Classe statique gérant les connexions à la base de données SQLite

```/// </summary>

```

---

### 🔍 Qu'est-ce que c'est ?

## 📦 Lignes 1-2 : Les imports

**Les trois slashes `///`**

```csharp- Différent des commentaires normaux `//`

1   using System.IO;- Crée une **documentation automatique**

2   using Microsoft.Data.Sqlite;- Quand tu survoles la classe dans Visual Studio, tu vois ce texte

```

**La balise `<summary>`**

### **Ligne 1 : `using System.IO;`**- Décrit brièvement ce que fait la classe

- IO = Input/Output (Entrée/Sortie)- Apparaît dans l'aide automatique de Visual Studio

- Outils pour gérer les **fichiers et dossiers**

- Permet d'utiliser `Directory.CreateDirectory()` et `File.Exists()`---



### **Ligne 2 : `using Microsoft.Data.Sqlite;`**## 🏛️ La classe Database

- Bibliothèque pour travailler avec **SQLite**

- SQLite = Base de données légère stockée dans un seul fichier `.db````csharp

- Permet d'utiliser `SqliteConnection` pour se connecterpublic static class Database

{

---```



## 🏷️ Ligne 4 : Le namespace### 🔍 Décomposition :



```csharp**`public`**

4   namespace ProjetParc.Data;- = Accessible depuis n'importe où dans le projet

```- **Analogie :** Une porte ouverte à tous, pas une porte privée



- **ProjetParc.Data** = Adresse complète de ce fichier**`static`**

- Tous les fichiers liés aux données sont dans `ProjetParc.Data`- = Pas besoin de créer une instance (une copie)

- On utilise directement `Database.Open()` au lieu de `new Database().Open()`

---- **Pourquoi ?** Il n'y a qu'une seule base de données, pas besoin d'en créer plusieurs copies



## 🏛️ Ligne 9 : La classe Database**`class Database`**

- Le nom de notre classe

```csharp- Convention : Les noms de classes commencent par une majuscule

9   public static class Database

```---



**`public`** - Accessible depuis n'importe où dans le projet## 📁 Variables privées - Les chemins



**`static`** - Pas besoin de créer une instance```csharp

- On utilise directement `Database.Open()` private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");

- Il n'y a qu'une seule base de données, pas besoin de copiesprivate static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

```

**`class Database`** - Le nom de notre classe

### 🔍 Première ligne - Le dossier de données

💡 **Important :** Tous les autres fichiers utilisent cette classe pour accéder aux données.

```csharp

---private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");

```

## 📁 Lignes 11-12 : Les chemins de la base de données

**Décomposition mot par mot :**

```csharp

11      private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");**`private`**

12      private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");- = Visible uniquement à l'intérieur de cette classe

```- Personne d'autre ne peut accéder à cette variable

- **Analogie :** Un carnet de notes personnel que tu ne montres à personne

### **Ligne 11 : Chemin du dossier**

**`static`**

**`private`** - Visible uniquement dans cette classe- = Partagé par toute la classe, pas par instance

- Il existe une seule version de cette variable

**`static readonly`** - Valeur partagée et non modifiable

**`readonly`**

**`Path.Combine(...)`** - Combine des morceaux de chemin intelligemment- = "Lecture seule" - ne peut pas être modifié après initialisation

- Gère automatiquement les `/` ou `\` selon le système (Windows/Mac/Linux)- On définit sa valeur une fois, puis elle ne change jamais

- **Sécurité :** Empêche de modifier accidentellement le chemin

**`AppDomain.CurrentDomain.BaseDirectory`**

- Le dossier où se trouve l'exécutable de l'application**`string`**

- Exemple : `C:\MonApp\`- = Type de donnée : du texte

- Contient des caractères (lettres, chiffres, symboles)

**`"database"`** - Nom du sous-dossier

**`DataDir`**

**Résultat :** Si l'application est dans `C:\MonApp\`, alors `DataDir = "C:\MonApp\database"`- = Le nom de la variable

- Convention : Commence par une majuscule car c'est une propriété

---

**`=`**

### **Ligne 12 : Chemin du fichier de base de données**- Opérateur d'affectation : "donne la valeur..."



**`Path.Combine(DataDir, "bddProjetParc.db")`****`Path.Combine(...)`**

- Ajoute le nom du fichier au chemin du dossier- Méthode qui **combine des morceaux de chemin** intelligemment

- **Résultat :** `C:\MonApp\database\bddProjetParc.db`- Gère automatiquement les `/` ou `\` selon le système (Windows, Mac, Linux)

- **Pourquoi ?** Windows utilise `\`, Linux/Mac utilisent `/`

💡 **L'extension `.db`** indique que c'est un fichier de base de données SQLite.

**`AppDomain.CurrentDomain.BaseDirectory`**

---- **Décomposition :**

  - `AppDomain` = Le domaine d'application (zone où tourne le programme)

## 🔓 Lignes 18-27 : La méthode Open() - Ouvrir une connexion  - `.CurrentDomain` = Le domaine actuel (notre application)

  - `.BaseDirectory` = Le dossier de base où se trouve l'exécutable

```csharp- **Exemple :** Si ton `.exe` est dans `C:\Program Files\ProjetParc\`, alors `BaseDirectory` = `C:\Program Files\ProjetParc\`

18      public static SqliteConnection Open()

19      {**`"database"`**

20          Directory.CreateDirectory(DataDir);- Le nom du sous-dossier qu'on veut créer

21          var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

22          connexion.Open();**Résultat final :**

23          using var pragma = connexion.CreateCommand();Si l'application est dans `C:\MonApp\`, alors :

24          pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";```

25          pragma.ExecuteNonQuery();DataDir = "C:\MonApp\database"

26          return connexion;```

27      }

```---



### **Ligne 18 : Signature de la méthode**### 🔍 Deuxième ligne - Le fichier de base de données



**`public static`** - Accessible partout, pas besoin d'instance```csharp

private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

**`SqliteConnection`** - Type de retour : une connexion à la base de données```



**`Open()`** - Nom de la méthode**Explication :**

- Prend le `DataDir` qu'on vient de définir

💡 **Utilisation :** `using var db = Database.Open();`- Ajoute le nom du fichier : `bddProjetParc.db`

- **Résultat :** `C:\MonApp\database\bddProjetParc.db`

---

**L'extension `.db`**

### **Ligne 20 : Créer le dossier si nécessaire**- Indique que c'est un fichier de base de données

- Peut être ouvert avec des outils comme DB Browser for SQLite

```csharp

20          Directory.CreateDirectory(DataDir);---

```

## 🔓 Méthode Open() - Ouvrir une connexion

- Crée le dossier `database` s'il n'existe pas

- Si le dossier existe déjà → ne fait rien (pas d'erreur)```csharp

- **Sécurité :** Garantit que le dossier existe avant d'essayer de créer le fichier/// <summary>

/// Ouvre une nouvelle connexion à la base de données SQLite

---/// </summary>

/// <returns>Une connexion SQLite ouverte et configurée</returns>

### **Ligne 21 : Créer la connexion**public static SqliteConnection Open()

{

```csharp    Directory.CreateDirectory(DataDir);

21          var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");    var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

```    connexion.Open();

    using var pragma = connexion.CreateCommand();

**`var connexion`** - Variable qui contient la connexion    pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";

    pragma.ExecuteNonQuery();

**`new SqliteConnection(...)`** - Crée une nouvelle connexion    return connexion;

}

**Chaîne de connexion :**```



1. **`Data Source={DbPath}`**### 🔍 Signature de la méthode

   - Indique où se trouve le fichier `.db`

   - Exemple : `Data Source=C:\MonApp\database\bddProjetParc.db````csharp

public static SqliteConnection Open()

2. **`Cache=Shared`**```

   - Le cache est partagé entre plusieurs connexions

   - **Effet :** Plusieurs parties du programme peuvent accéder à la DB en même temps**Décomposition :**

   - Améliore les performances

**`public`**

3. **`Foreign Keys=True`**- Accessible depuis n'importe où

   - Active la vérification des clés étrangères (liens entre tables)- Permet à `Program.cs` et autres fichiers d'appeler cette méthode

   - **Sécurité :** Empêche de supprimer des données liées

   - Exemple : Impossible de supprimer un type d'équipement si des équipements l'utilisent**`static`**

- Peut être appelé directement : `Database.Open()`

---- Pas besoin de faire `new Database()`



### **Ligne 22 : Ouvrir la connexion****`SqliteConnection`**

- Le **type de retour** (ce que la méthode renvoie)

```csharp- C'est une connexion à la base de données SQLite

22          connexion.Open();- **Analogie :** C'est comme recevoir une clé pour ouvrir un coffre

```

**`Open()`**

- Ouvre vraiment le canal de communication avec la base de données- Le nom de la méthode

- Jusqu'ici, on avait juste préparé la connexion- Les parenthèses vides signifient qu'elle ne prend aucun paramètre

- **Important :** Sans cette ligne, aucune requête ne fonctionnerait

---

💡 **Analogie :** On a construit le pont, maintenant on l'ouvre à la circulation.

### 🔍 Ligne 1 : Créer le dossier

---

```csharp

### **Lignes 23-25 : Configuration avancée (PRAGMA)**Directory.CreateDirectory(DataDir);

```

```csharp

23          using var pragma = connexion.CreateCommand();**Explication :**

24          pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";- `Directory` = Classe pour gérer les dossiers

25          pragma.ExecuteNonQuery();- `CreateDirectory()` = Crée un dossier

```- `DataDir` = Le chemin qu'on a défini plus haut



**Ligne 23 :** Crée un objet pour exécuter des commandes SQL**Intelligent :**

- `using` = gestion automatique de la mémoire (nettoyage auto)- Si le dossier existe déjà → Ne fait rien (pas d'erreur)

- Si le dossier n'existe pas → Le crée

**Ligne 24 :** Définit deux configurations SQLite

**Pourquoi ?**

1. **`PRAGMA journal_mode=WALL`**La première fois qu'on lance l'application, le dossier `database` n'existe pas. Cette ligne le crée automatiquement.

   - WALL = Write-Ahead Logging (Journalisation en écriture anticipée)

   - **Avantages :**---

     - 📈 Écritures plus rapides

     - 🔒 Protection des données en cas de crash### 🔍 Ligne 2 : Créer la connexion

     - 👥 Plusieurs utilisateurs peuvent lire pendant qu'un autre écrit

```csharp

2. **`PRAGMA busy_timeout=3000`**var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

   - Temps d'attente maximum = 3000 millisecondes (3 secondes)```

   - Si la base est occupée, attendre jusqu'à 3s avant d'abandonner

   - **Évite les erreurs** quand plusieurs personnes accèdent en même temps**Décomposition ultra-détaillée :**



**Ligne 25 :** Exécute les commandes de configuration**`var`**

- Mot-clé pour "Variable avec type automatique"

---- Le compilateur devine que c'est une `SqliteConnection`

- Équivalent à écrire : `SqliteConnection connexion = ...`

### **Ligne 26 : Retourner la connexion**

**`connexion`**

```csharp- Le nom qu'on donne à notre variable

26          return connexion;- On aurait pu l'appeler `maConnexion` ou `db`

```

**`=`**

- Renvoie la connexion configurée et ouverte- Opérateur d'affectation

- Le code appelant peut maintenant l'utiliser pour lire/écrire des données

**`new SqliteConnection(...)`**

**Exemple d'utilisation :**- `new` = Crée une nouvelle instance

```csharp- `SqliteConnection` = Le type d'objet créé

using var db = Database.Open();- C'est comme construire un pont entre notre code et la base de données

// Maintenant on peut faire des requêtes SQL avec 'db'

```**La chaîne de connexion :**



---```csharp

$"Data Source={DbPath};Cache=Shared;Foreign Keys=True;"

## ✅ Lignes 32-39 : La méthode EnsureInitialized() - Vérifier l'initialisation```



```csharp**Le `$` au début**

32      public static void EnsureInitialized()- Crée une "string interpolée"

33      {- Permet d'insérer des variables avec `{}`

34          Directory.CreateDirectory(DataDir);- `{DbPath}` sera remplacé par le chemin réel

35          if (!File.Exists(DbPath))

36          {**Décomposition de la chaîne :**

37              using var _ = Open();

38          }1. **`Data Source={DbPath}`**

39      }   - Indique où se trouve le fichier de base de données

```   - Exemple : `Data Source=C:\MonApp\database\bddProjetParc.db`



### **Ligne 32 : Signature**2. **`;`**

   - Sépare les différentes options

**`public static void`** - Méthode publique qui ne retourne rien

3. **`Cache=Shared`**

**`EnsureInitialized()`** - "Assure-toi que tout est initialisé"   - **Cache** = Mémoire temporaire pour aller plus vite

   - **Shared** = Partagé entre plusieurs connexions

💡 **Appelée par :** `Program.cs` au démarrage de l'application   - **Effet :** Plusieurs parties du programme peuvent accéder à la DB en même temps

   - **Performance :** Les données fréquentes restent en mémoire

---

4. **`;`**

### **Ligne 34 : Créer le dossier**   - Encore un séparateur



```csharp5. **`Foreign Keys=True`**

34          Directory.CreateDirectory(DataDir);   - **Foreign Keys** = Clés étrangères

```   - Ce sont des liens entre tables (ex: un équipement appartient à un type)

   - **True** = Active la vérification de ces liens

- Double sécurité : on s'assure vraiment que le dossier existe   - **Sécurité :** Empêche de supprimer un type d'équipement si des équipements l'utilisent encore

- Déjà fait dans `Open()`, mais on le refait par précaution

---

---

### 🔍 Ligne 3 : Ouvrir la connexion

### **Lignes 35-38 : Créer le fichier si nécessaire**

```csharp

```csharpconnexion.Open();

35          if (!File.Exists(DbPath))```

36          {

37              using var _ = Open();**Explication :**

38          }- Jusqu'ici, on a juste **préparé** la connexion

```- Cette ligne **ouvre vraiment** le canal de communication avec la base de données

- **Analogie :** On a construit le pont, maintenant on l'ouvre à la circulation

**Ligne 35 :** `if (!File.Exists(DbPath))`

- `File.Exists()` vérifie si le fichier existe**Important :**

- `!` inverse le résultatSans cette ligne, toute tentative d'utiliser la connexion échouerait.

- **Traduction :** "Si le fichier N'EXISTE PAS..."

---

**Ligne 37 :** `using var _ = Open();`

- Appelle `Open()` qui crée la connexion### 🔍 Lignes 4-6 : Configuration avancée (PRAGMA)

- **Important :** SQLite crée automatiquement le fichier `.db` à la première connexion

- `_` = variable qu'on n'utilise pas (juste pour l'effet de bord)```csharp

- `using` = ferme et nettoie automatiquement aprèsusing var pragma = connexion.CreateCommand();

pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";

💡 **Résultat :** Le fichier de base de données est créé (même vide au départ).pragma.ExecuteNonQuery();

```

---

#### **Ligne 4 : Créer une commande**

## 🎬 Scénarios d'utilisation

```csharp

### **Scénario 1 : Premier lancement de l'application**using var pragma = connexion.CreateCommand();

```

```

1. Program.cs appelle Database.EnsureInitialized()**`using`**

   ↓- Mot-clé spécial pour la **gestion automatique de la mémoire**

2. Le dossier "database" est créé- Quand le code sort du bloc, l'objet est automatiquement libéré

   ↓- **Analogie :** Tu empruntes un livre, quand tu as fini, il est automatiquement retourné

3. File.Exists() retourne false (fichier n'existe pas)

   ↓**`var pragma`**

4. Open() est appelé :- Variable qui contiendra une commande SQL

   - Dossier créé (déjà fait)- Nom `pragma` car on va exécuter des commandes PRAGMA

   - Connexion créée → SQLite crée le fichier .db

   - Configuration PRAGMA appliquée**`connexion.CreateCommand()`**

   ↓- Crée un objet qui peut exécuter des commandes SQL

5. Connexion fermée automatiquement (using)- C'est comme préparer une feuille de papier pour écrire une instruction

   ↓

6. Le fichier bddProjetParc.db existe maintenant !---

```

#### **Ligne 5 : Définir les commandes SQL**

### **Scénario 2 : Lancements suivants**

```csharp

```pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";

1. EnsureInitialized() est appelé```

   ↓

2. Le dossier existe déjà → rien ne se passe**PRAGMA ?**

   ↓- Ce sont des **commandes de configuration** pour SQLite

3. File.Exists() retourne true- Modifient le comportement de la base de données

   ↓

4. Le bloc if est ignoré**Première commande : `PRAGMA journal_mode=WALL;`**

   ↓

5. Tout est déjà prêt !**`journal_mode`**

```- Mode de journal = Comment SQLite gère les modifications



### **Scénario 3 : Accès aux données depuis une vue****`WALL`** (Write-Ahead Logging)

- **Fonctionnement :** Les modifications sont écrites dans un fichier temporaire d'abord

```- **Avantages :**

1. Une vue fait : using var db = Database.Open()  - 📈 **Performance** : Écritures plus rapides

   ↓  - 🔒 **Sécurité** : Si le programme plante, les données sont protégées

2. Le dossier est vérifié/créé  - 👥 **Concurrence** : Plusieurs utilisateurs peuvent lire pendant qu'un autre écrit

   ↓

3. Connexion créée et ouverte**Deuxième commande : `PRAGMA busy_timeout=3000;`**

   ↓

4. Configuration PRAGMA appliquée**`busy_timeout`**

   ↓- Délai d'attente quand la base est occupée

5. La variable 'db' peut exécuter des requêtes SQL

   ↓**`3000`**

6. À la fin du bloc 'using', connexion fermée automatiquement- 3000 millisecondes = 3 secondes

```- Si la base est verrouillée, attendre 3 secondes avant d'abandonner



---**Scénario :**

1. L'utilisateur A modifie un équipement

## 🎓 Concepts clés2. L'utilisateur B essaie de lire en même temps

3. Au lieu d'échouer immédiatement, B attend jusqu'à 3 secondes

### **1. Chemins dynamiques**4. Dès que A finit, B peut accéder aux données

- On n'écrit jamais un chemin en dur comme `C:\database\`

- On utilise `AppDomain.CurrentDomain.BaseDirectory`---

- **Avantage :** L'application fonctionne sur n'importe quel ordinateur

#### **Ligne 6 : Exécuter les commandes**

### **2. Gestion automatique de la mémoire**

- Le mot-clé `using` nettoie automatiquement```csharp

- Les connexions sont fermées proprementpragma.ExecuteNonQuery();

- Évite les fuites de mémoire```



### **3. Configuration optimale****`ExecuteNonQuery()`**

- WALL améliore les performances- Exécute une commande SQL qui ne retourne pas de données

- busy_timeout évite les erreurs de concurrence- **Non-Query** = Pas de requête de lecture (SELECT)

- Foreign Keys garantit l'intégrité des données- Ici, on configure juste, on ne lit rien



### **4. Défense en profondeur**---

- Le dossier est créé dans deux endroits différents

- Double vérification pour être sûr### 🔍 Ligne 7 : Retourner la connexion

- Mieux vaut prévenir que guérir !

```csharp

---return connexion;

```

## 💡 Questions fréquentes

**Explication :**

**Q : Pourquoi SQLite et pas MySQL ou SQL Server ?**- Renvoie la connexion configurée et ouverte

- R : SQLite est parfait pour les petites applications. Un seul fichier, pas de serveur à installer, très simple.- Le code qui appelle `Database.Open()` reçoit cette connexion

- Il peut maintenant l'utiliser pour lire/écrire des données

**Q : Que se passe-t-il si deux utilisateurs modifient en même temps ?**

- R : Le `busy_timeout` fait attendre le second utilisateur. SQLite gère les verrous automatiquement.**Exemple d'utilisation :**

```csharp

**Q : Peut-on changer le nom du fichier bddProjetParc.db ?**using var db = Database.Open();

- R : Oui, modifier la ligne 12. Mais attention : l'ancienne base ne sera plus utilisée.// Maintenant on peut utiliser 'db' pour accéder à la base de données

```

**Q : Où se trouve physiquement le fichier .db ?**

- R : Dans le même dossier que l'exécutable, dans un sous-dossier `database\`.---



---## ✅ Méthode EnsureInitialized() - S'assurer que tout est prêt



## 🔗 Fichiers liés```csharp

/// <summary>

- **`Program.cs`** - Appelle `EnsureInitialized()` au démarrage/// S'assure que la base de données est initialisée et que le répertoire existe

- **Toutes les vues** - Utilisent `Open()` pour accéder aux données/// </summary>

public static void EnsureInitialized()

---{

    Directory.CreateDirectory(DataDir);

## 📊 Structure des fichiers    if (!File.Exists(DbPath))

    {

```        using var _ = Open();

Application (C:\MonApp\)    }

├─ ProjetParc.exe}

├─ Data/```

│  └─ DataBase.cs  ← CE FICHIER

└─ database/### 🔍 Signature de la méthode

   └─ bddProjetParc.db  ← Créé automatiquement

``````csharp

public static void EnsureInitialized()

---```



**📌 Prochaine étape :** Consulter la documentation des vues pour voir comment elles utilisent cette base de données.**`void`**

- Cette méthode ne retourne rien
- Elle fait juste une action de vérification/préparation

**`EnsureInitialized`**
- "Ensure" = S'assurer
- "Initialized" = Initialisé
- → "Assure-toi que tout est initialisé"

---

### 🔍 Ligne 1 : Créer le dossier

```csharp
Directory.CreateDirectory(DataDir);
```

**Même chose que dans `Open()`**
- Double sécurité : on s'assure vraiment que le dossier existe
- Pas de problème si on le crée deux fois

---

### 🔍 Lignes 2-5 : Créer le fichier si nécessaire

```csharp
if (!File.Exists(DbPath))
{
    using var _ = Open();
}
```

**Décomposition ligne par ligne :**

**`if (!File.Exists(DbPath))`**

- `File.Exists(DbPath)` = Vérifie si le fichier existe
  - Retourne `true` si le fichier existe
  - Retourne `false` si le fichier n'existe pas
  
- `!` = Opérateur de négation (NOT)
  - Inverse le résultat
  - `!true` devient `false`
  - `!false` devient `true`

- **Traduction :** "Si le fichier N'EXISTE PAS"

**`{`**
- Début du bloc de code à exécuter si la condition est vraie

**`using var _ = Open();`**

**Explication détaillée :**

- `using` = Gestion automatique de la mémoire
- `var _` = Variable avec un nom spécial `_`
  - Le `_` signifie "Je m'en fiche de cette variable"
  - On l'utilise quand on ne va pas réutiliser la valeur
  
- `= Open()` = Appelle notre méthode `Open()`

**Mais pourquoi ?**

Quand on appelle `Open()` :
1. Le dossier est créé (si nécessaire)
2. Une connexion à la base de données est créée
3. **SQLite crée automatiquement le fichier `.db` s'il n'existe pas**
4. La connexion est configurée et ouverte

Ensuite, grâce au `using`, la connexion est automatiquement fermée et nettoyée.

**Résultat :**
Le fichier de base de données est créé, même vide au départ.

---

## 🎬 Flux complet d'utilisation

### Scénario 1 : Premier lancement de l'application

```
1. Program.cs appelle Database.EnsureInitialized()
   ↓
2. Le dossier "database" est créé
   ↓
3. File.Exists() retourne false (le fichier n'existe pas)
   ↓
4. Open() est appelé :
   a. Dossier créé (déjà fait, mais sécurité)
   b. Connexion créée avec le chemin vers le fichier
   c. Connexion ouverte → SQLite crée le fichier .db
   d. Configuration PRAGMA appliquée
   ↓
5. La connexion est fermée automatiquement (using)
   ↓
6. Le fichier bddProjetParc.db existe maintenant !
```

### Scénario 2 : Lancements suivants

```
1. Database.EnsureInitialized() est appelé
   ↓
2. Le dossier existe déjà → rien ne se passe
   ↓
3. File.Exists() retourne true
   ↓
4. Le bloc if est ignoré
   ↓
5. Tout est déjà prêt !
```

### Scénario 3 : Accès aux données

```
1. Un autre fichier fait : using var db = Database.Open()
   ↓
2. Le dossier est vérifié/créé
   ↓
3. Une connexion est créée et ouverte
   ↓
4. Configuration PRAGMA appliquée
   ↓
5. La variable 'db' peut maintenant exécuter des requêtes SQL
   ↓
6. À la fin du bloc 'using', la connexion se ferme automatiquement
```

---

## 🎓 Concepts clés à retenir

### 1. **Séparation des responsabilités**
- Cette classe ne fait qu'une chose : gérer la connexion
- Elle ne contient pas de requêtes SQL spécifiques
- C'est une bonne pratique de conception

### 2. **Chemins dynamiques**
- On n'écrit jamais un chemin en dur comme `C:\database\`
- On utilise `AppDomain.CurrentDomain.BaseDirectory`
- L'application fonctionnera sur n'importe quel ordinateur

### 3. **Gestion de la mémoire**
- Le mot-clé `using` nettoie automatiquement
- Les connexions sont fermées proprement
- Évite les fuites de mémoire

### 4. **Configuration optimale**
- WALL améliore les performances
- busy_timeout évite les erreurs de concurrence
- Foreign Keys garantit l'intégrité des données

### 5. **Défense en profondeur**
- On crée le dossier dans deux endroits différents
- Double vérification pour être sûr
- Mieux vaut prévenir que guérir !

---

## 💡 Questions fréquentes

**Q : Pourquoi SQLite et pas MySQL ou SQL Server ?**
- R : SQLite est parfait pour les petites applications. Un seul fichier, pas de serveur à installer, très simple.

**Q : Que se passe-t-il si deux utilisateurs modifient en même temps ?**
- R : Le `busy_timeout` fait attendre le second utilisateur. SQLite gère les verrous automatiquement.

**Q : Peut-on changer le nom du fichier bddProjetParc.db ?**
- R : Oui, il suffit de modifier la variable `DbPath`. Mais attention, si on le change, l'ancienne base ne sera plus utilisée.

**Q : Où se trouve physiquement le fichier .db ?**
- R : Dans le même dossier que l'exécutable, dans un sous-dossier `database\`.

**Q : Le `using var _` est-il obligatoire ?**
- R : On pourrait écrire `var connexion = Open(); connexion.Close();` mais `using` est plus sûr car il ferme même en cas d'erreur.

---

## 🔗 Fichiers liés

- `Program.cs` - Appelle `EnsureInitialized()` au démarrage
- Tous les fichiers de vues - Utilisent `Open()` pour accéder aux données

---

## 📊 Schéma de la structure

```
Application
    ├─ Program.cs
    │   └─ EnsureInitialized() ───┐
    │                              │
    ├─ Database.cs ◄───────────────┘
    │   ├─ DataDir (variable)
    │   ├─ DbPath (variable)
    │   ├─ Open() méthode
    │   └─ EnsureInitialized() méthode
    │
    └─ Système de fichiers
        └─ database/
            └─ bddProjetParc.db
```

---

**📌 Prochaine étape :** Consulter la documentation des vues pour comprendre comment elles utilisent cette base de données.
