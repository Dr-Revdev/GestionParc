# Configuration du Premier Lancement - GestiParc

## 📋 Vue d'ensemble

Le système de configuration du premier lancement permet aux utilisateurs de choisir l'emplacement de leur base de données lors de la première utilisation de l'application.

---

## 🎯 Fonctionnalités

### 1. Détection du premier lancement
- L'application vérifie si un fichier de configuration existe
- Si aucune configuration n'existe, la fenêtre de premier lancement s'affiche

### 2. Deux options disponibles

#### Option A : Utiliser une base de données existante
- Ouvre un dialogue de sélection de fichier
- Permet de choisir un fichier `.db` existant
- Supporte :
  - Disque local (C:\, D:\, etc.)
  - Réseau partagé (\\serveur\partage\)
  - SharePoint (si monté comme lecteur réseau)
  - Tout emplacement accessible par le système de fichiers Windows

#### Option B : Créer une nouvelle base de données
- Ouvre un dialogue de sauvegarde de fichier
- Permet de choisir où créer le fichier `.db`
- Crée le fichier et le répertoire si nécessaire
- Supporte les mêmes emplacements que l'option A

### 3. Sauvegarde de la configuration
- Le chemin choisi est sauvegardé dans `%APPDATA%\GestiParc\config.json`
- La configuration est chargée automatiquement aux lancements suivants
- Pas besoin de reconfigurer à chaque fois

---

## 🏗️ Architecture

### Fichiers créés

#### 1. `Data/AppConfig.cs`
**Rôle** : Gère la configuration de l'application

**Propriétés** :
- `DatabasePath` : Chemin vers la base de données

**Méthodes principales** :
```csharp
// Vérifie si c'est le premier lancement
public static bool IsFirstRun()

// Charge la configuration depuis le fichier JSON
public static AppConfig Load()

// Sauvegarde la configuration
public void Save()

// Réinitialise la configuration (utile pour les tests)
public static void Reset()
```

**Emplacement du fichier de configuration** :
```
%APPDATA%\GestiParc\config.json
```

**Format du fichier** :
```json
{
  "DatabasePath": "C:\\chemin\\vers\\bddGestiParc.db"
}
```

---

#### 2. `Views/FirstRun/FirstRunView.cs`
**Rôle** : Interface graphique de configuration du premier lancement

**Composants** :
- Titre et description
- Bouton "Utiliser une base de données existante" (bleu)
- Bouton "Créer une nouvelle base de données" (vert)

**Comportement** :
1. L'utilisateur clique sur un des boutons
2. Un dialogue de fichier s'ouvre (Open ou Save)
3. L'utilisateur sélectionne/crée le fichier
4. Une confirmation est demandée
5. Le chemin est retourné via `SelectedDatabasePath`

---

### Modifications des fichiers existants

#### 1. `Program.cs`
**Changements** :
- Ajout de la logique de détection du premier lancement
- Affichage de `FirstRunView` si nécessaire
- Chargement de la configuration
- Initialisation de la base de données avec le chemin configuré

**Flux d'exécution** :
```
Démarrage
    ↓
Premier lancement ?
    ↓ Oui               ↓ Non
FirstRunView    Charger config.json
    ↓                   ↓
Sauvegarder config      ↓
    ↓                   ↓
    ↓←←←←←←←←←←←←←←←←←←↓
    ↓
Database.Initialize(path)
    ↓
Database.EnsureInitialized()
    ↓
WelcomePage
```

**Code complet** :
```csharp
[STAThread]
static void Main()
{
    try
    {
        ApplicationConfiguration.Initialize();

        // Vérifier si c'est le premier lancement
        if (AppConfig.IsFirstRun())
        {
            // Afficher la fenêtre de configuration du premier lancement
            using var firstRunView = new FirstRunView();
            var result = firstRunView.ShowDialog();

            if (result != DialogResult.OK || string.IsNullOrEmpty(firstRunView.SelectedDatabasePath))
            {
                MessageBox.Show("Configuration annulée...");
                return;
            }

            // Sauvegarder la configuration
            var config = new AppConfig
            {
                DatabasePath = firstRunView.SelectedDatabasePath
            };
            config.Save();

            // Initialiser la base de données
            Database.Initialize(config.DatabasePath);
        }
        else
        {
            // Charger la configuration existante
            var config = AppConfig.Load();

            if (string.IsNullOrEmpty(config.DatabasePath))
            {
                MessageBox.Show("Configuration invalide...");
                AppConfig.Reset();
                return;
            }

            // Initialiser la base de données
            Database.Initialize(config.DatabasePath);
        }

        // S'assurer que la base de données est initialisée
        Database.EnsureInitialized();

        // Lancer l'application principale
        Application.Run(new WelcomePage());
    }
    catch (Exception ex)
    {
        MessageBox.Show("Startup error:\n" + ex, "Crash au démarrage");
    }
}
```

---

#### 2. `Data/Database.cs`
**Changements** :
- Suppression du chemin codé en dur (`DataDir` et `DbPath` statiques)
- Ajout de `Initialize(string databasePath)` pour configurer le chemin
- Le chemin est maintenant stocké dans une variable statique `_dbPath`
- Toutes les méthodes vérifient que `Initialize()` a été appelé
- **NOUVEAU** : `EnsureInitialized()` crée automatiquement le schéma complet si les tables n'existent pas
- **NOUVEAU** : `CreateSchema()` crée toutes les tables, index et données initiales

**Méthodes** :

```csharp
// NOUVELLE : Initialise le chemin de la BDD
public static void Initialize(string databasePath)
{
    _dbPath = databasePath;
}

// NOUVELLE : Obtient le chemin actuel
public static string GetDatabasePath()
{
    if (string.IsNullOrEmpty(_dbPath))
        throw new InvalidOperationException("Database not initialized");
    return _dbPath;
}

// MODIFIÉE : Utilise _dbPath au lieu d'un chemin fixe
public static SqliteConnection Open()
{
    if (string.IsNullOrEmpty(_dbPath))
        throw new InvalidOperationException("Database not initialized");
    
    var directory = Path.GetDirectoryName(_dbPath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);
    
    var connexion = new SqliteConnection($"Data Source={_dbPath};Cache=Shared;Foreign Keys=True;");
    connexion.Open();
    // ... PRAGMA ...
    return connexion;
}

// MODIFIÉE : Vérifie si les tables existent, sinon crée le schéma
public static void EnsureInitialized()
{
    if (string.IsNullOrEmpty(_dbPath))
        throw new InvalidOperationException("Database not initialized");
    
    // Créer le répertoire si nécessaire
    var directory = Path.GetDirectoryName(_dbPath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);
    
    // Vérifier si les tables existent déjà
    bool needsInitialization = false;
    try
    {
        using var testConnection = Open();
        using var command = testConnection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Agents'";
        var result = command.ExecuteScalar();
        needsInitialization = result == null || (long)result == 0;
    }
    catch
    {
        needsInitialization = true;
    }
    
    // Créer le schéma si nécessaire
    if (needsInitialization)
    {
        using var connection = Open();
        CreateSchema(connection);
    }
}

// NOUVELLE : Crée le schéma complet de la base de données
private static void CreateSchema(SqliteConnection connection)
{
    using var command = connection.CreateCommand();
    
    // SQL complet : 6 tables + 5 index + types d'équipement initiaux
    command.CommandText = @"
        BEGIN TRANSACTION;
        
        CREATE TABLE IF NOT EXISTS Equipes (...);
        CREATE TABLE IF NOT EXISTS Sites (...);
        CREATE TABLE IF NOT EXISTS equipment_type (...);
        CREATE TABLE IF NOT EXISTS Agents (...);
        CREATE TABLE IF NOT EXISTS Equipements (...);
        CREATE TABLE IF NOT EXISTS Travail (...);
        
        CREATE INDEX IF NOT EXISTS idx_agents_equipe ON Agents(equipe_id);
        CREATE INDEX IF NOT EXISTS idx_agents_site ON Agents(site_id);
        CREATE INDEX IF NOT EXISTS idx_equipements_idrh ON Equipements(idrh);
        CREATE INDEX IF NOT EXISTS idx_equipements_type ON Equipements(type_id);
        CREATE INDEX IF NOT EXISTS idx_travail_site ON Travail(site_id);
        
        INSERT INTO equipment_type (id, name) VALUES 
            (1, 'PC'),
            (2, 'Ecran'),
            (3, 'Imprimante'),
            (4, 'Routeur'),
            (5, 'Switch'),
            (6, 'Inconnu');
        
        COMMIT;
    ";
    
    command.ExecuteNonQuery();
}
```

**Schéma créé automatiquement** :
- 6 tables : `Agents`, `Equipements`, `Equipes`, `Sites`, `Travail`, `equipment_type`
- 5 index pour optimiser les performances
- 6 types d'équipement par défaut
- Contraintes de clés étrangères avec CASCADE/SET NULL
- PRAGMA : `journal_mode=WAL`, `busy_timeout=3000`

---

## 🔄 Flux d'utilisation

### Premier lancement
1. L'utilisateur lance l'application
2. `AppConfig.IsFirstRun()` retourne `true`
3. `FirstRunView` s'affiche
4. L'utilisateur choisit "Utiliser existante" ou "Créer nouvelle"
5. Un dialogue de fichier s'ouvre
6. L'utilisateur sélectionne/crée le fichier `.db`
7. Le chemin est sauvegardé dans `config.json`
8. `Database.Initialize(path)` est appelé
9. L'application principale démarre normalement

### Lancements suivants
1. L'utilisateur lance l'application
2. `AppConfig.IsFirstRun()` retourne `false`
3. `AppConfig.Load()` charge `config.json`
4. `Database.Initialize(path)` est appelé avec le chemin sauvegardé
5. L'application principale démarre normalement

---

## 🌐 Cas d'usage

### Cas 1 : Utilisateur unique en local
```
Utilisateur → Crée nouvelle BDD → C:\Users\John\Documents\GestiParc\bdd.db
```

### Cas 2 : Équipe avec BDD partagée sur réseau
```
Utilisateur A → Utilise existante → \\serveur\partage\bdd.db
Utilisateur B → Utilise existante → \\serveur\partage\bdd.db
Utilisateur C → Utilise existante → \\serveur\partage\bdd.db
```

### Cas 3 : BDD sur SharePoint (monté comme lecteur)
```
Utilisateur → Utilise existante → Z:\SharePoint\Projets\GestiParc\bdd.db
```

### Cas 4 : BDD portable sur clé USB
```
Utilisateur → Crée nouvelle → E:\GestiParc\bdd.db
(où E:\ est une clé USB)
```

---

## 🧪 Tests

### Tester le premier lancement
1. Supprimer `%APPDATA%\GestiParc\config.json`
2. Lancer l'application
3. Vérifier que `FirstRunView` s'affiche
4. Tester les deux options (existante et nouvelle)

### Tester le lancement normal
1. S'assurer que `config.json` existe
2. Lancer l'application
3. Vérifier que l'application démarre directement sans `FirstRunView`

### Réinitialiser pour les tests
```csharp
// Dans le code
AppConfig.Reset();

// Ou manuellement
// Supprimer : %APPDATA%\GestiParc\config.json
```

---

## 🔧 Maintenance

### Changer l'emplacement de la BDD
Pour permettre à l'utilisateur de changer l'emplacement plus tard, ajouter une option dans `SettingsView` :

```csharp
private void ChangeDatabaseLocationButton_Click(object sender, EventArgs e)
{
    using var firstRunView = new FirstRunView();
    if (firstRunView.ShowDialog() == DialogResult.OK)
    {
        var config = AppConfig.Load();
        config.DatabasePath = firstRunView.SelectedDatabasePath;
        config.Save();
        
        MessageBox.Show(
            "L'emplacement de la base de données a été modifié.\n" +
            "Veuillez redémarrer l'application.",
            "Redémarrage requis",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
        
        Application.Exit();
    }
}
```

---

## 🚨 Gestion des erreurs

### Erreur : Fichier de configuration invalide
```csharp
if (string.IsNullOrEmpty(config.DatabasePath))
{
    MessageBox.Show("Configuration invalide...");
    AppConfig.Reset(); // Force reconfiguration
    return;
}
```

### Erreur : Base de données inaccessible
```csharp
try
{
    Database.Initialize(config.DatabasePath);
    Database.EnsureInitialized();
}
catch (Exception ex)
{
    MessageBox.Show($"Impossible d'accéder à la base de données : {ex.Message}");
    AppConfig.Reset(); // Permet de reconfigurer
}
```

### Erreur : Initialize() non appelé
```csharp
if (string.IsNullOrEmpty(_dbPath))
{
    throw new InvalidOperationException(
        "La base de données n'a pas été initialisée. Appelez Database.Initialize() d'abord."
    );
}
```

---

## 📝 Notes importantes

1. **Permissions** : L'utilisateur doit avoir les droits de lecture/écriture sur l'emplacement choisi
2. **Réseau** : Pour un partage réseau, vérifier que tous les utilisateurs ont accès
3. **SharePoint** : Doit être monté comme lecteur réseau (OneDrive Desktop, etc.)
4. **Sauvegarde** : Le fichier `.db` peut être sauvegardé/copié librement
5. **Migration** : Pour changer de BDD, il suffit de sélectionner un autre fichier au premier lancement

---

## 🎨 Interface utilisateur

### Design
- Fenêtre moderne avec fond blanc
- Deux gros boutons colorés (bleu et vert)
- Effet hover sur les boutons
- Icônes emoji pour une meilleure compréhension

### Textes
- Titre : "Bienvenue dans GestiParc !"
- Description claire des options
- Messages de confirmation
- Gestion des erreurs avec messages explicites

---

## 🔮 Améliorations futures possibles

1. **Validation avancée** : Vérifier que la BDD est bien au format SQLite
2. **Migration de données** : Importer des données depuis une ancienne BDD
3. **Profils multiples** : Permettre de basculer entre plusieurs BDD
4. **Cloud direct** : Support natif OneDrive/SharePoint sans montage de lecteur
5. **Cryptage** : Option pour crypter la base de données
6. **Historique** : Garder trace des BDD récemment utilisées
