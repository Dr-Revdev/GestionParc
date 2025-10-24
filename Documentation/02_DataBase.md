# 📘 Documentation de DataBase.cs# 📘 Documentation de DataBase.cs



## 🎯 But de ce fichier## 🎯 But de ce fichier



Ce fichier est le **gardien de la base de données**. Il gère tout ce qui concerne l'accès aux données : créer le dossier, ouvrir les connexions, et configurer la base de données.Ce fichier est le **gardien de la base de données**. Il gère tout ce qui concerne l'accès aux données : créer le dossier, ouvrir les connexions, et configurer la base de données.



💡 **Analogie :** C'est comme le bibliothécaire qui gère l'accès aux livres, s'assure que la bibliothèque existe, et configure les règles d'emprunt.

💡 **Analogie :** C'est comme le bibliothécaire qui gère l'accès aux livres, s'assure que la bibliothèque existe, et configure les règles d'emprunt.

---

---

## 📦 Les "using" - Importer des outils

##  Les "using" - Importer des outils

```csharp

using System.IO;```csharp

using Microsoft.Data.Sqlite;using System.IO;

```using Microsoft.Data.Sqlite;

```### 🔍 Explication :

### 🔍 Explication :

6   /// <summary>

**`using System.IO;`**

- `IO` = Input/Output (Entrée/Sortie)7   /// Classe statique gérant les connexions à la base de données SQLite**`using System.IO;`**

- C'est la boîte à outils pour gérer les **fichiers et dossiers**

- Permet de :8   /// </summary>- `IO` = Input/Output (Entrée/Sortie)

  - Créer des dossiers (`Directory.CreateDirectory`)

  - Vérifier si un fichier existe (`File.Exists`)9   public static class Database- C'est la boîte à outils pour gérer les **fichiers et dossiers**

  - Lire/écrire des fichiers

10  {- Permet de :

**`using Microsoft.Data.Sqlite;`**

- C'est la bibliothèque pour travailler avec **SQLite**11      private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");  - Créer des dossiers (`Directory.CreateDirectory`)

- **SQLite** = Un type de base de données très simple et légère

- Stocke toutes les données dans un seul fichier (`.db`)12      private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");  - Vérifier si un fichier existe (`File.Exists`)

- Parfait pour les petites applications

13  - Lire/écrire des fichiers

---

14      /// <summary>

## 🏷️ Le namespace

15      /// Ouvre une nouvelle connexion à la base de données SQLite**`using Microsoft.Data.Sqlite;`**

```csharp

namespace ProjetParc.Data;16      /// </summary>- C'est la bibliothèque pour travailler avec **SQLite**

```

17      /// <returns>Une connexion SQLite ouverte et configurée</returns>- **SQLite** = Un type de base de données très simple et légère

**Explication :**

- `ProjetParc` = Notre projet18      public static SqliteConnection Open()- Stocke toutes les données dans un seul fichier (`.db`)

- `Data` = Le sous-dossier/catégorie pour tout ce qui concerne les données

- Adresse complète : `ProjetParc.Data.Database`19      {- Parfait pour les petites applications



---20          Directory.CreateDirectory(DataDir);



## 💬 Commentaire XML de documentation21          var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");---



```csharp22          connexion.Open();

/// <summary>

/// Classe statique gérant les connexions à la base de données SQLite23          using var pragma = connexion.CreateCommand();## 🏷️ Le namespace

/// </summary>

```24          pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";



### 🔍 Qu'est-ce que c'est ?25          pragma.ExecuteNonQuery();```csharp



**Les trois slashes `///`**26          return connexion;namespace ProjetParc.Data;

- Différent des commentaires normaux `//`

- Crée une **documentation automatique**27      }```

- Quand tu survoles la classe dans Visual Studio, tu vois ce texte

28

**La balise `<summary>`**

- Décrit brièvement ce que fait la classe29      /// <summary>**Explication :**

- Apparaît dans l'aide automatique de Visual Studio

30      /// S'assure que la base de données est initialisée et que le répertoire existe- `ProjetParc` = Notre projet

---

31      /// </summary>- `Data` = Le sous-dossier/catégorie pour tout ce qui concerne les données

## 🏛️ La classe Database

32      public static void EnsureInitialized()- Adresse complète : `ProjetParc.Data.Database`

```csharp

public static class Database33      {

{

```34          Directory.CreateDirectory(DataDir);---



### 🔍 Décomposition :35          if (!File.Exists(DbPath))



**`public`**36          {## 💬 Commentaire XML de documentation

- = Accessible depuis n'importe où dans le projet

- **Analogie :** Une porte ouverte à tous, pas une porte privée37              using var _ = Open();



**`static`**38          }```csharp

- = Pas besoin de créer une instance (une copie)

- On utilise directement `Database.Open()` au lieu de `new Database().Open()`39      }/// <summary>

- **Pourquoi ?** Il n'y a qu'une seule base de données, pas besoin d'en créer plusieurs copies

40  }/// Classe statique gérant les connexions à la base de données SQLite

**`class Database`**

- Le nom de notre classe```/// </summary>

- Convention : Les noms de classes commencent par une majuscule

```

---

---

## 📁 Variables privées - Les chemins

### 🔍 Qu'est-ce que c'est ?

```csharp

private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");## 📦 Lignes 1-2 : Les imports

private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

```**Les trois slashes `///`**



### 🔍 Première ligne - Le dossier de données```csharp- Différent des commentaires normaux `//`



```csharp1   using System.IO;- Crée une **documentation automatique**

private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");

```2   using Microsoft.Data.Sqlite;- Quand tu survoles la classe dans Visual Studio, tu vois ce texte



**Décomposition mot par mot :**```



**`private`****La balise `<summary>`**

- = Visible uniquement à l'intérieur de cette classe

- Personne d'autre ne peut accéder à cette variable### **Ligne 1 : `using System.IO;`**- Décrit brièvement ce que fait la classe

- **Analogie :** Un carnet de notes personnel que tu ne montres à personne

- IO = Input/Output (Entrée/Sortie)- Apparaît dans l'aide automatique de Visual Studio

**`static`**

- = Partagé par toute la classe, pas par instance- Outils pour gérer les **fichiers et dossiers**

- Il existe une seule version de cette variable

- Permet d'utiliser `Directory.CreateDirectory()` et `File.Exists()`---

**`readonly`**

- = "Lecture seule" - ne peut pas être modifié après initialisation

- On définit sa valeur une fois, puis elle ne change jamais

- **Sécurité :** Empêche de modifier accidentellement le chemin### **Ligne 2 : `using Microsoft.Data.Sqlite;`**## 🏛️ La classe Database



**`string`**- Bibliothèque pour travailler avec **SQLite**

- = Type de donnée : du texte

- Contient des caractères (lettres, chiffres, symboles)- SQLite = Base de données légère stockée dans un seul fichier `.db````csharp



**`DataDir`**- Permet d'utiliser `SqliteConnection` pour se connecterpublic static class Database

- = Le nom de la variable

- Convention : Commence par une majuscule car c'est une propriété{



**`=`**---```

- Opérateur d'affectation : "donne la valeur..."



**`Path.Combine(...)`**

- Méthode qui **combine des morceaux de chemin** intelligemment## 🏷️ Ligne 4 : Le namespace### 🔍 Décomposition :

- Gère automatiquement les `/` ou `\` selon le système (Windows, Mac, Linux)

- **Pourquoi ?** Windows utilise `\`, Linux/Mac utilisent `/`



**`AppDomain.CurrentDomain.BaseDirectory`**```csharp**`public`**

- **Décomposition :**

  - `AppDomain` = Le domaine d'application (zone où tourne le programme)4   namespace ProjetParc.Data;- = Accessible depuis n'importe où dans le projet

  - `.CurrentDomain` = Le domaine actuel (notre application)

  - `.BaseDirectory` = Le dossier de base où se trouve l'exécutable```- **Analogie :** Une porte ouverte à tous, pas une porte privée

- **Exemple :** Si ton `.exe` est dans `C:\Program Files\ProjetParc\`, alors `BaseDirectory` = `C:\Program Files\ProjetParc\`



**`"database"`**

- Le nom du sous-dossier qu'on veut créer- **ProjetParc.Data** = Adresse complète de ce fichier**`static`**



**Résultat final :**- Tous les fichiers liés aux données sont dans `ProjetParc.Data`- = Pas besoin de créer une instance (une copie)

Si l'application est dans `C:\MonApp\`, alors :

```- On utilise directement `Database.Open()` au lieu de `new Database().Open()`

DataDir = "C:\MonApp\database"

```---- **Pourquoi ?** Il n'y a qu'une seule base de données, pas besoin d'en créer plusieurs copies



---



### 🔍 Deuxième ligne - Le fichier de base de données## 🏛️ Ligne 9 : La classe Database**`class Database`**



```csharp- Le nom de notre classe

private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

``````csharp- Convention : Les noms de classes commencent par une majuscule



**Explication :**9   public static class Database

- Prend le `DataDir` qu'on vient de définir

- Ajoute le nom du fichier : `bddProjetParc.db````---

- **Résultat :** `C:\MonApp\database\bddProjetParc.db`



**L'extension `.db`**

- Indique que c'est un fichier de base de données**`public`** - Accessible depuis n'importe où dans le projet## 📁 Variables privées - Les chemins

- Peut être ouvert avec des outils comme DB Browser for SQLite



---

**`static`** - Pas besoin de créer une instance```csharp

## 🔓 Méthode Open() - Ouvrir une connexion

- On utilise directement `Database.Open()` private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");

```csharp

/// <summary>- Il n'y a qu'une seule base de données, pas besoin de copiesprivate static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

/// Ouvre une nouvelle connexion à la base de données SQLite

/// </summary>```

/// <returns>Une connexion SQLite ouverte et configurée</returns>

public static SqliteConnection Open()**`class Database`** - Le nom de notre classe

{

    Directory.CreateDirectory(DataDir);### 🔍 Première ligne - Le dossier de données

    var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

    connexion.Open();💡 **Important :** Tous les autres fichiers utilisent cette classe pour accéder aux données.

    using var pragma = connexion.CreateCommand();

    pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";```csharp

    pragma.ExecuteNonQuery();

    return connexion;---private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");

}

``````



### 🔍 Signature de la méthode## 📁 Lignes 11-12 : Les chemins de la base de données



```csharp**Décomposition mot par mot :**

public static SqliteConnection Open()

``````csharp



**Décomposition :**11      private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");**`private`**



**`public`**12      private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");- = Visible uniquement à l'intérieur de cette classe

- Accessible depuis n'importe où

- Permet à `Program.cs` et autres fichiers d'appeler cette méthode```- Personne d'autre ne peut accéder à cette variable



**`static`**- **Analogie :** Un carnet de notes personnel que tu ne montres à personne

- Peut être appelé directement : `Database.Open()`

- Pas besoin de faire `new Database()`### **Ligne 11 : Chemin du dossier**



**`SqliteConnection`****`static`**

- Le **type de retour** (ce que la méthode renvoie)

- C'est une connexion à la base de données SQLite**`private`** - Visible uniquement dans cette classe- = Partagé par toute la classe, pas par instance

- **Analogie :** C'est comme recevoir une clé pour ouvrir un coffre

- Il existe une seule version de cette variable

**`Open()`**

- Le nom de la méthode**`static readonly`** - Valeur partagée et non modifiable

- Les parenthèses vides signifient qu'elle ne prend aucun paramètre

**`readonly`**

---

**`Path.Combine(...)`** - Combine des morceaux de chemin intelligemment- = "Lecture seule" - ne peut pas être modifié après initialisation

### 🔍 Ligne 1 : Créer le dossier

- Gère automatiquement les `/` ou `\` selon le système (Windows/Mac/Linux)- On définit sa valeur une fois, puis elle ne change jamais

```csharp

Directory.CreateDirectory(DataDir);- **Sécurité :** Empêche de modifier accidentellement le chemin

```

**`AppDomain.CurrentDomain.BaseDirectory`**

**Explication :**

- `Directory` = Classe pour gérer les dossiers- Le dossier où se trouve l'exécutable de l'application**`string`**

- `CreateDirectory()` = Crée un dossier

- `DataDir` = Le chemin qu'on a défini plus haut- Exemple : `C:\MonApp\`- = Type de donnée : du texte



**Intelligent :**- Contient des caractères (lettres, chiffres, symboles)

- Si le dossier existe déjà → Ne fait rien (pas d'erreur)

- Si le dossier n'existe pas → Le crée**`"database"`** - Nom du sous-dossier



**Pourquoi ?****`DataDir`**

La première fois qu'on lance l'application, le dossier `database` n'existe pas. Cette ligne le crée automatiquement.

**Résultat :** Si l'application est dans `C:\MonApp\`, alors `DataDir = "C:\MonApp\database"`- = Le nom de la variable

---

- Convention : Commence par une majuscule car c'est une propriété

### 🔍 Ligne 2 : Créer la connexion

---

```csharp

var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");**`=`**

```

### **Ligne 12 : Chemin du fichier de base de données**- Opérateur d'affectation : "donne la valeur..."

**Décomposition ultra-détaillée :**



**`var`**

- Mot-clé pour "Variable avec type automatique"**`Path.Combine(DataDir, "bddProjetParc.db")`****`Path.Combine(...)`**

- Le compilateur devine que c'est une `SqliteConnection`

- Équivalent à écrire : `SqliteConnection connexion = ...`- Ajoute le nom du fichier au chemin du dossier- Méthode qui **combine des morceaux de chemin** intelligemment



**`connexion`**- **Résultat :** `C:\MonApp\database\bddProjetParc.db`- Gère automatiquement les `/` ou `\` selon le système (Windows, Mac, Linux)

- Le nom qu'on donne à notre variable

- On aurait pu l'appeler `maConnexion` ou `db`- **Pourquoi ?** Windows utilise `\`, Linux/Mac utilisent `/`



**`=`**💡 **L'extension `.db`** indique que c'est un fichier de base de données SQLite.

- Opérateur d'affectation

**`AppDomain.CurrentDomain.BaseDirectory`**

**`new SqliteConnection(...)`**

- `new` = Crée une nouvelle instance---- **Décomposition :**

- `SqliteConnection` = Le type d'objet créé

- C'est comme construire un pont entre notre code et la base de données  - `AppDomain` = Le domaine d'application (zone où tourne le programme)



**La chaîne de connexion :**## 🔓 Lignes 18-27 : La méthode Open() - Ouvrir une connexion  - `.CurrentDomain` = Le domaine actuel (notre application)



```csharp  - `.BaseDirectory` = Le dossier de base où se trouve l'exécutable

$"Data Source={DbPath};Cache=Shared;Foreign Keys=True;"

``````csharp- **Exemple :** Si ton `.exe` est dans `C:\Program Files\ProjetParc\`, alors `BaseDirectory` = `C:\Program Files\ProjetParc\`



**Le `$` au début**18      public static SqliteConnection Open()

- Crée une "string interpolée"

- Permet d'insérer des variables avec `{}`19      {**`"database"`**

- `{DbPath}` sera remplacé par le chemin réel

20          Directory.CreateDirectory(DataDir);- Le nom du sous-dossier qu'on veut créer

**Décomposition de la chaîne :**

21          var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

1. **`Data Source={DbPath}`**

   - Indique où se trouve le fichier de base de données22          connexion.Open();**Résultat final :**

   - Exemple : `Data Source=C:\MonApp\database\bddProjetParc.db`

23          using var pragma = connexion.CreateCommand();Si l'application est dans `C:\MonApp\`, alors :

2. **`;`**

   - Sépare les différentes options24          pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";```



3. **`Cache=Shared`**25          pragma.ExecuteNonQuery();DataDir = "C:\MonApp\database"

   - **Cache** = Mémoire temporaire pour aller plus vite

   - **Shared** = Partagé entre plusieurs connexions26          return connexion;```

   - **Effet :** Plusieurs parties du programme peuvent accéder à la DB en même temps

   - **Performance :** Les données fréquentes restent en mémoire27      }



4. **`;`**```---

   - Encore un séparateur



5. **`Foreign Keys=True`**

   - **Foreign Keys** = Clés étrangères### **Ligne 18 : Signature de la méthode**### 🔍 Deuxième ligne - Le fichier de base de données

   - Ce sont des liens entre tables (ex: un équipement appartient à un type)

   - **True** = Active la vérification de ces liens

   - **Sécurité :** Empêche de supprimer un type d'équipement si des équipements l'utilisent encore

**`public static`** - Accessible partout, pas besoin d'instance```csharp

---

private static readonly string DbPath = Path.Combine(DataDir, "bddProjetParc.db");

### 🔍 Ligne 3 : Ouvrir la connexion

**`SqliteConnection`** - Type de retour : une connexion à la base de données```

```csharp

connexion.Open();

```

**`Open()`** - Nom de la méthode**Explication :**

**Explication :**

- Jusqu'ici, on a juste **préparé** la connexion- Prend le `DataDir` qu'on vient de définir

- Cette ligne **ouvre vraiment** le canal de communication avec la base de données

- **Analogie :** On a construit le pont, maintenant on l'ouvre à la circulation💡 **Utilisation :** `using var db = Database.Open();`- Ajoute le nom du fichier : `bddProjetParc.db`



**Important :**- **Résultat :** `C:\MonApp\database\bddProjetParc.db`

Sans cette ligne, toute tentative d'utiliser la connexion échouerait.

---

---

**L'extension `.db`**

### 🔍 Lignes 4-6 : Configuration avancée (PRAGMA)

### **Ligne 20 : Créer le dossier si nécessaire**- Indique que c'est un fichier de base de données

```csharp

using var pragma = connexion.CreateCommand();- Peut être ouvert avec des outils comme DB Browser for SQLite

pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";

pragma.ExecuteNonQuery();```csharp

```

20          Directory.CreateDirectory(DataDir);---

#### **Ligne 4 : Créer une commande**

```

```csharp

using var pragma = connexion.CreateCommand();## 🔓 Méthode Open() - Ouvrir une connexion

```

- Crée le dossier `database` s'il n'existe pas

**`using`**

- Mot-clé spécial pour la **gestion automatique de la mémoire**- Si le dossier existe déjà → ne fait rien (pas d'erreur)```csharp

- Quand le code sort du bloc, l'objet est automatiquement libéré

- **Analogie :** Tu empruntes un livre, quand tu as fini, il est automatiquement retourné- **Sécurité :** Garantit que le dossier existe avant d'essayer de créer le fichier/// <summary>



**`var pragma`**/// Ouvre une nouvelle connexion à la base de données SQLite

- Variable qui contiendra une commande SQL

- Nom `pragma` car on va exécuter des commandes PRAGMA---/// </summary>



**`connexion.CreateCommand()`**/// <returns>Une connexion SQLite ouverte et configurée</returns>

- Crée un objet qui peut exécuter des commandes SQL

- C'est comme préparer une feuille de papier pour écrire une instruction### **Ligne 21 : Créer la connexion**public static SqliteConnection Open()



---{



#### **Ligne 5 : Définir les commandes SQL**```csharp    Directory.CreateDirectory(DataDir);



```csharp21          var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");    var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";

``````    connexion.Open();



**PRAGMA ?**    using var pragma = connexion.CreateCommand();

- Ce sont des **commandes de configuration** pour SQLite

- Modifient le comportement de la base de données**`var connexion`** - Variable qui contient la connexion    pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";



**Première commande : `PRAGMA journal_mode=WALL;`**    pragma.ExecuteNonQuery();



**`journal_mode`****`new SqliteConnection(...)`** - Crée une nouvelle connexion    return connexion;

- Mode de journal = Comment SQLite gère les modifications

}

**`WALL`** (Write-Ahead Logging)

- **Fonctionnement :** Les modifications sont écrites dans un fichier temporaire d'abord**Chaîne de connexion :**```

- **Avantages :**

  - 📈 **Performance** : Écritures plus rapides

  - 🔒 **Sécurité** : Si le programme plante, les données sont protégées

  - 👥 **Concurrence** : Plusieurs utilisateurs peuvent lire pendant qu'un autre écrit1. **`Data Source={DbPath}`**### 🔍 Signature de la méthode



**Deuxième commande : `PRAGMA busy_timeout=3000;`**   - Indique où se trouve le fichier `.db`



**`busy_timeout`**   - Exemple : `Data Source=C:\MonApp\database\bddProjetParc.db````csharp

- Délai d'attente quand la base est occupée

public static SqliteConnection Open()

**`3000`**

- 3000 millisecondes = 3 secondes2. **`Cache=Shared`**```

- Si la base est verrouillée, attendre 3 secondes avant d'abandonner

   - Le cache est partagé entre plusieurs connexions

**Scénario :**

1. L'utilisateur A modifie un équipement   - **Effet :** Plusieurs parties du programme peuvent accéder à la DB en même temps**Décomposition :**

2. L'utilisateur B essaie de lire en même temps

3. Au lieu d'échouer immédiatement, B attend jusqu'à 3 secondes   - Améliore les performances

4. Dès que A finit, B peut accéder aux données

**`public`**

---

3. **`Foreign Keys=True`**- Accessible depuis n'importe où

#### **Ligne 6 : Exécuter les commandes**

   - Active la vérification des clés étrangères (liens entre tables)- Permet à `Program.cs` et autres fichiers d'appeler cette méthode

```csharp

pragma.ExecuteNonQuery();   - **Sécurité :** Empêche de supprimer des données liées

```

   - Exemple : Impossible de supprimer un type d'équipement si des équipements l'utilisent**`static`**

**`ExecuteNonQuery()`**

- Exécute une commande SQL qui ne retourne pas de données- Peut être appelé directement : `Database.Open()`

- **Non-Query** = Pas de requête de lecture (SELECT)

- Ici, on configure juste, on ne lit rien---- Pas besoin de faire `new Database()`



---



### 🔍 Ligne 7 : Retourner la connexion### **Ligne 22 : Ouvrir la connexion****`SqliteConnection`**



```csharp- Le **type de retour** (ce que la méthode renvoie)

return connexion;

``````csharp- C'est une connexion à la base de données SQLite



**Explication :**22          connexion.Open();- **Analogie :** C'est comme recevoir une clé pour ouvrir un coffre

- Renvoie la connexion configurée et ouverte

- Le code qui appelle `Database.Open()` reçoit cette connexion```

- Il peut maintenant l'utiliser pour lire/écrire des données

**`Open()`**

**Exemple d'utilisation :**

- Ouvre vraiment le canal de communication avec la base de données- Le nom de la méthode

```csharp

using var db = Database.Open();- Jusqu'ici, on avait juste préparé la connexion- Les parenthèses vides signifient qu'elle ne prend aucun paramètre

// Maintenant on peut utiliser 'db' pour accéder à la base de données

```- **Important :** Sans cette ligne, aucune requête ne fonctionnerait



------



## ✅ Méthode EnsureInitialized() - S'assurer que tout est prêt💡 **Analogie :** On a construit le pont, maintenant on l'ouvre à la circulation.



```csharp### 🔍 Ligne 1 : Créer le dossier

/// <summary>

/// S'assure que la base de données est initialisée et que le répertoire existe---

/// </summary>

public static void EnsureInitialized()```csharp

{

    Directory.CreateDirectory(DataDir);### **Lignes 23-25 : Configuration avancée (PRAGMA)**Directory.CreateDirectory(DataDir);

    if (!File.Exists(DbPath))

    {```

        using var _ = Open();

    }```csharp



}23          using var pragma = connexion.CreateCommand();**Explication :**

```

24          pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";- `Directory` = Classe pour gérer les dossiers

### 🔍 Signature de la méthode

25          pragma.ExecuteNonQuery();- `CreateDirectory()` = Crée un dossier

```csharp

public static void EnsureInitialized()```- `DataDir` = Le chemin qu'on a défini plus haut

```



**`void`**

- Cette méthode ne retourne rien**Ligne 23 :** Crée un objet pour exécuter des commandes SQL**Intelligent :**

- Elle fait juste une action de vérification/préparation

- `using` = gestion automatique de la mémoire (nettoyage auto)- Si le dossier existe déjà → Ne fait rien (pas d'erreur)

**`EnsureInitialized`**

- "Ensure" = S'assurer- Si le dossier n'existe pas → Le crée

- "Initialized" = Initialisé

- → "Assure-toi que tout est initialisé"**Ligne 24 :** Définit deux configurations SQLite



---**Pourquoi ?**



### 🔍 Ligne 1 : Créer le dossier1. **`PRAGMA journal_mode=WALL`**La première fois qu'on lance l'application, le dossier `database` n'existe pas. Cette ligne le crée automatiquement.



```csharp   - WALL = Write-Ahead Logging (Journalisation en écriture anticipée)

Directory.CreateDirectory(DataDir);

```   - **Avantages :**---



**Même chose que dans `Open()`**     - 📈 Écritures plus rapides

- Double sécurité : on s'assure vraiment que le dossier existe

- Pas de problème si on le crée deux fois     - 🔒 Protection des données en cas de crash### 🔍 Ligne 2 : Créer la connexion



---     - 👥 Plusieurs utilisateurs peuvent lire pendant qu'un autre écrit



### 🔍 Lignes 2-5 : Créer le fichier si nécessaire```csharp



```csharp2. **`PRAGMA busy_timeout=3000`**var connexion = new SqliteConnection($"Data Source={DbPath};Cache=Shared;Foreign Keys=True;");

if (!File.Exists(DbPath))

{   - Temps d'attente maximum = 3000 millisecondes (3 secondes)```

    using var _ = Open();

}   - Si la base est occupée, attendre jusqu'à 3s avant d'abandonner

```

   - **Évite les erreurs** quand plusieurs personnes accèdent en même temps**Décomposition ultra-détaillée :**

**Décomposition ligne par ligne :**



**`if (!File.Exists(DbPath))`**

**Ligne 25 :** Exécute les commandes de configuration**`var`**

- `File.Exists(DbPath)` = Vérifie si le fichier existe

  - Retourne `true` si le fichier existe- Mot-clé pour "Variable avec type automatique"

  - Retourne `false` si le fichier n'existe pas

  ---- Le compilateur devine que c'est une `SqliteConnection`

- `!` = Opérateur de négation (NOT)

  - Inverse le résultat- Équivalent à écrire : `SqliteConnection connexion = ...`

  - `!true` devient `false`

  - `!false` devient `true`### **Ligne 26 : Retourner la connexion**



- **Traduction :** "Si le fichier N'EXISTE PAS"**`connexion`**



**`{`**```csharp- Le nom qu'on donne à notre variable

- Début du bloc de code à exécuter si la condition est vraie

26          return connexion;- On aurait pu l'appeler `maConnexion` ou `db`

**`using var _ = Open();`**

```

**Explication détaillée :**

**`=`**

- `using` = Gestion automatique de la mémoire

- `var _` = Variable avec un nom spécial `_`- Renvoie la connexion configurée et ouverte- Opérateur d'affectation

  - Le `_` signifie "Je m'en fiche de cette variable"

  - On l'utilise quand on ne va pas réutiliser la valeur- Le code appelant peut maintenant l'utiliser pour lire/écrire des données

  

- `= Open()` = Appelle notre méthode `Open()`**`new SqliteConnection(...)`**



**Mais pourquoi ?****Exemple d'utilisation :**- `new` = Crée une nouvelle instance



Quand on appelle `Open()` :```csharp- `SqliteConnection` = Le type d'objet créé

1. Le dossier est créé (si nécessaire)

2. Une connexion à la base de données est crééeusing var db = Database.Open();- C'est comme construire un pont entre notre code et la base de données

3. **SQLite crée automatiquement le fichier `.db` s'il n'existe pas**

4. La connexion est configurée et ouverte// Maintenant on peut faire des requêtes SQL avec 'db'



Ensuite, grâce au `using`, la connexion est automatiquement fermée et nettoyée.```**La chaîne de connexion :**



**Résultat :**

Le fichier de base de données est créé, même vide au départ.

---```csharp

---

$"Data Source={DbPath};Cache=Shared;Foreign Keys=True;"

## 🎬 Flux complet d'utilisation

## ✅ Lignes 32-39 : La méthode EnsureInitialized() - Vérifier l'initialisation```

### Scénario 1 : Premier lancement de l'application



```

1. Program.cs appelle Database.EnsureInitialized()```csharp**Le `$` au début**

   ↓

2. Le dossier "database" est créé32      public static void EnsureInitialized()- Crée une "string interpolée"

   ↓

3. File.Exists() retourne false (le fichier n'existe pas)33      {- Permet d'insérer des variables avec `{}`

   ↓

4. Open() est appelé :34          Directory.CreateDirectory(DataDir);- `{DbPath}` sera remplacé par le chemin réel

   a. Dossier créé (déjà fait, mais sécurité)

   b. Connexion créée avec le chemin vers le fichier35          if (!File.Exists(DbPath))

   c. Connexion ouverte → SQLite crée le fichier .db

   d. Configuration PRAGMA appliquée36          {**Décomposition de la chaîne :**

   ↓

5. La connexion est fermée automatiquement (using)37              using var _ = Open();

   ↓

6. Le fichier bddProjetParc.db existe maintenant !38          }1. **`Data Source={DbPath}`**

```

39      }   - Indique où se trouve le fichier de base de données

### Scénario 2 : Lancements suivants

```   - Exemple : `Data Source=C:\MonApp\database\bddProjetParc.db`

```

1. Database.EnsureInitialized() est appelé

   ↓

2. Le dossier existe déjà → rien ne se passe### **Ligne 32 : Signature**2. **`;`**

   ↓

3. File.Exists() retourne true   - Sépare les différentes options

   ↓

4. Le bloc if est ignoré**`public static void`** - Méthode publique qui ne retourne rien

   ↓

5. Tout est déjà prêt !3. **`Cache=Shared`**

```

**`EnsureInitialized()`** - "Assure-toi que tout est initialisé"   - **Cache** = Mémoire temporaire pour aller plus vite

### Scénario 3 : Accès aux données

   - **Shared** = Partagé entre plusieurs connexions

```

1. Un autre fichier fait : using var db = Database.Open()💡 **Appelée par :** `Program.cs` au démarrage de l'application   - **Effet :** Plusieurs parties du programme peuvent accéder à la DB en même temps

   ↓

2. Le dossier est vérifié/créé   - **Performance :** Les données fréquentes restent en mémoire

   ↓

3. Une connexion est créée et ouverte---

   ↓

4. Configuration PRAGMA appliquée4. **`;`**

   ↓

5. La variable 'db' peut maintenant exécuter des requêtes SQL### **Ligne 34 : Créer le dossier**   - Encore un séparateur

   ↓

6. À la fin du bloc 'using', la connexion se ferme automatiquement

```

```csharp5. **`Foreign Keys=True`**

---

34          Directory.CreateDirectory(DataDir);   - **Foreign Keys** = Clés étrangères

## 🎓 Concepts clés à retenir

```   - Ce sont des liens entre tables (ex: un équipement appartient à un type)

### 1. **Séparation des responsabilités**

- Cette classe ne fait qu'une chose : gérer la connexion   - **True** = Active la vérification de ces liens

- Elle ne contient pas de requêtes SQL spécifiques

- C'est une bonne pratique de conception- Double sécurité : on s'assure vraiment que le dossier existe   - **Sécurité :** Empêche de supprimer un type d'équipement si des équipements l'utilisent encore



### 2. **Chemins dynamiques**- Déjà fait dans `Open()`, mais on le refait par précaution

- On n'écrit jamais un chemin en dur comme `C:\database\`

- On utilise `AppDomain.CurrentDomain.BaseDirectory`---

- L'application fonctionnera sur n'importe quel ordinateur

---

### 3. **Gestion de la mémoire**

- Le mot-clé `using` nettoie automatiquement### 🔍 Ligne 3 : Ouvrir la connexion

- Les connexions sont fermées proprement

- Évite les fuites de mémoire### **Lignes 35-38 : Créer le fichier si nécessaire**



### 4. **Configuration optimale**```csharp

- WALL améliore les performances

- busy_timeout évite les erreurs de concurrence```csharpconnexion.Open();

- Foreign Keys garantit l'intégrité des données

35          if (!File.Exists(DbPath))```

### 5. **Défense en profondeur**

- On crée le dossier dans deux endroits différents36          {

- Double vérification pour être sûr

- Mieux vaut prévenir que guérir !37              using var _ = Open();**Explication :**



---38          }- Jusqu'ici, on a juste **préparé** la connexion



## 💡 Questions fréquentes```- Cette ligne **ouvre vraiment** le canal de communication avec la base de données



**Q : Pourquoi SQLite et pas MySQL ou SQL Server ?**- **Analogie :** On a construit le pont, maintenant on l'ouvre à la circulation

- R : SQLite est parfait pour les petites applications. Un seul fichier, pas de serveur à installer, très simple.

**Ligne 35 :** `if (!File.Exists(DbPath))`

**Q : Que se passe-t-il si deux utilisateurs modifient en même temps ?**

- R : Le `busy_timeout` fait attendre le second utilisateur. SQLite gère les verrous automatiquement.- `File.Exists()` vérifie si le fichier existe**Important :**



**Q : Peut-on changer le nom du fichier bddProjetParc.db ?**- `!` inverse le résultatSans cette ligne, toute tentative d'utiliser la connexion échouerait.

- R : Oui, il suffit de modifier la variable `DbPath`. Mais attention, si on le change, l'ancienne base ne sera plus utilisée.

- **Traduction :** "Si le fichier N'EXISTE PAS..."

**Q : Où se trouve physiquement le fichier .db ?**

- R : Dans le même dossier que l'exécutable, dans un sous-dossier `database\`.---



**Q : Le `using var _` est-il obligatoire ?****Ligne 37 :** `using var _ = Open();`

- R : On pourrait écrire `var connexion = Open(); connexion.Close();` mais `using` est plus sûr car il ferme même en cas d'erreur.

- Appelle `Open()` qui crée la connexion### 🔍 Lignes 4-6 : Configuration avancée (PRAGMA)

---

- **Important :** SQLite crée automatiquement le fichier `.db` à la première connexion

## 🔗 Fichiers liés

- `_` = variable qu'on n'utilise pas (juste pour l'effet de bord)```csharp

- `Program.cs` - Appelle `EnsureInitialized()` au démarrage

- Tous les fichiers de vues - Utilisent `Open()` pour accéder aux données- `using` = ferme et nettoie automatiquement aprèsusing var pragma = connexion.CreateCommand();



---pragma.CommandText = "PRAGMA journal_mode=WALL; PRAGMA busy_timeout=3000;";



## 📊 Schéma de la structure💡 **Résultat :** Le fichier de base de données est créé (même vide au départ).pragma.ExecuteNonQuery();



``````

Application

    ├─ Program.cs---

    │   └─ EnsureInitialized() ───┐

    │                              │#### **Ligne 4 : Créer une commande**

    ├─ Database.cs ◄───────────────┘

    │   ├─ DataDir (variable)## 🎬 Scénarios d'utilisation

    │   ├─ DbPath (variable)

    │   ├─ Open() méthode```csharp

    │   └─ EnsureInitialized() méthode

    │### **Scénario 1 : Premier lancement de l'application**using var pragma = connexion.CreateCommand();

    └─ Système de fichiers

        └─ database/```

            └─ bddProjetParc.db

``````



---1. Program.cs appelle Database.EnsureInitialized()**`using`**



**📌 Prochaine étape :** Consulter la documentation des vues pour comprendre comment elles utilisent cette base de données.   ↓- Mot-clé spécial pour la **gestion automatique de la mémoire**


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
