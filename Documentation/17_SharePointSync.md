# 📘 Documentation de SharePointSyncManager.cs

## 🎯 But de ce fichier
Ce fichier gère la **synchronisation avec SharePoint/OneDrive** pour permettre à plusieurs utilisateurs de travailler sur la même base de données. Il s'occupe de :
- Détecter si la base de données est sur SharePoint/OneDrive
- Créer une copie locale de travail
- Gérer les **fichiers de verrouillage** (lock files) pour éviter les conflits
- Sauvegarder les modifications vers SharePoint

💡 **Analogie :** C'est comme un système de réservation de livre à la bibliothèque. Quand quelqu'un emprunte le livre, les autres doivent attendre qu'il le rende.

---

## 🆕 Nouveauté v1.1.0

Cette fonctionnalité permet le **travail multi-utilisateur** :
- ✅ Plusieurs personnes peuvent utiliser l'application
- ✅ Une seule personne à la fois peut modifier la base
- ✅ Protection contre les modifications simultanées
- ✅ Récupération automatique des verrous expirés (après 4h)
- ✅ Sauvegarde manuelle vers SharePoint

---

## 📦 Les imports (using)

```csharp
using System;
using System.IO;
using System.Text.Json;
using System.Text;
using Microsoft.Data.Sqlite;
```

**Bibliothèques utilisées :**
- `System.IO` - Gestion des fichiers et dossiers
- `System.Text.Json` - Sérialisation/désérialisation JSON pour les lock files
- `Microsoft.Data.Sqlite` - Accès à SQLite pour le checkpoint WAL

---

## 🏛️ La classe SharePointSyncManager

```csharp
public class SharePointSyncManager
{
    private readonly string _sharePointPath;
    private readonly string _localWorkingPath;
    private readonly LockFile _currentLock;
    
    public bool IsActive { get; }
}
```

**Variables privées :**
- `_sharePointPath` - Chemin du dossier SharePoint/OneDrive
- `_localWorkingPath` - Chemin de la copie locale de travail
- `_currentLock` - Informations sur le verrouillage actuel
- `IsActive` - Indique si la synchronisation est active

---

## 🔍 Méthode Initialize() - Détection et initialisation

```csharp
public static SharePointSyncManager Initialize(string basePath)
{
    // Détecte si c'est un chemin SharePoint/OneDrive
    if (!basePath.Contains("OneDrive", StringComparison.OrdinalIgnoreCase) &&
        !basePath.Contains("SharePoint", StringComparison.OrdinalIgnoreCase))
    {
        return new SharePointSyncManager(basePath, basePath, null, false);
    }

    // Crée une copie locale
    string localPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GestiParc", 
        "database_local"
    );

    // Vérifie/crée le lock
    var lockFile = LockFile.Create();
    var lockPath = Path.Combine(basePath, ".lock");
    
    if (File.Exists(lockPath))
    {
        var existing = LockFile.Load(lockPath);
        if (!existing.IsExpired())
        {
            // Base verrouillée par quelqu'un d'autre
            throw new SharePointLockException(existing);
        }
    }

    // Copie depuis SharePoint vers local
    CopyDirectory(basePath, localPath);
    
    // Crée le fichier de verrouillage
    lockFile.Save(lockPath);
    
    return new SharePointSyncManager(basePath, localPath, lockFile, true);
}
```

**Étapes de détection :**

1. **Vérifier si SharePoint/OneDrive**
   - Regarde si le chemin contient "OneDrive" ou "SharePoint"
   - Si non → Mode normal sans synchronisation

2. **Créer un dossier local de travail**
   - Dans `%LocalAppData%\GestiParc\database_local`
   - Exemple : `C:\Users\Jean\AppData\Local\GestiParc\database_local`

3. **Vérifier le fichier de verrouillage (.lock)**
   - Si existe et non expiré → Erreur, quelqu'un d'autre utilise la base
   - Si expiré (>4h) → Peut être forcé

4. **Copier la base depuis SharePoint vers local**
   - Copie tous les fichiers du dossier SharePoint
   - Travail sur la copie locale = plus rapide

5. **Créer le nouveau fichier .lock**
   - Enregistre : utilisateur, machine, heure, PID
   - Indique "Je suis en train d'utiliser la base"

💡 **Pourquoi une copie locale ?**
- SharePoint/OneDrive peut être lent
- Évite les conflits de synchronisation automatique
- Meilleure performance

---

## 🔒 La classe LockFile - Fichier de verrouillage

```csharp
public class LockFile
{
    public string User { get; set; }
    public string MachineName { get; set; }
    public DateTime Timestamp { get; set; }
    public int ProcessId { get; set; }

    public bool IsExpired(TimeSpan? timeout = null)
    {
        var maxAge = timeout ?? TimeSpan.FromHours(4);
        return DateTime.Now - Timestamp > maxAge;
    }

    public static LockFile Create()
    {
        return new LockFile
        {
            User = Environment.UserName,
            MachineName = Environment.MachineName,
            Timestamp = DateTime.Now,
            ProcessId = Environment.ProcessId
        };
    }

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    public static LockFile Load(string path)
    {
        var json = File.ReadAllText(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<LockFile>(json);
    }
}
```

**Structure du fichier .lock (JSON) :**

```json
{
  "User": "jean.dupont",
  "MachineName": "PC-JEAN",
  "Timestamp": "2025-10-29T14:30:00",
  "ProcessId": 12345
}
```

**Informations stockées :**
- **User** - Nom de l'utilisateur Windows
- **MachineName** - Nom de l'ordinateur
- **Timestamp** - Date/heure de création du lock
- **ProcessId** - ID du processus de l'application

**Méthode IsExpired() :**
- Vérifie si le lock a plus de 4 heures
- Si oui → Considéré comme abandonné (crash probable)
- Permet de récupérer l'accès à la base

---

## 💾 Méthode CopyToSharePoint() - Sauvegarde vers SharePoint

```csharp
public void CopyToSharePoint()
{
    if (!IsActive) return;

    // IMPORTANT : Forcer SQLite à écrire toutes les modifications
    using (var connection = new SqliteConnection($"Data Source={_localWorkingPath}"))
    {
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
        command.ExecuteNonQuery();
    }

    // Copier la base locale vers SharePoint
    CopyDirectory(_localWorkingPath, _sharePointPath);
}
```

**⚠️ CRITIQUE - Le checkpoint WAL**

SQLite en mode WAL (Write-Ahead Logging) crée 3 fichiers :
- `bddProjetParc.db` - Base de données principale
- `bddProjetParc.db-wal` - Journal des modifications
- `bddProjetParc.db-shm` - Fichier de mémoire partagée

**Le problème :**
Sans le checkpoint, les modifications restent dans le fichier `.db-wal` et ne sont PAS dans le `.db` principal !

**La solution : `PRAGMA wal_checkpoint(TRUNCATE)`**
- Force SQLite à fusionner le `.db-wal` dans le `.db` principal
- Vide le fichier WAL après fusion
- **GARANTIT** que toutes les modifications sont dans le fichier principal

💡 **Découverte v1.1.0 :** C'était le bug principal ! Les copies vers SharePoint semblaient réussir mais les modifications n'étaient pas sauvegardées car elles restaient dans le WAL.

**Ensuite : Copie tous les fichiers vers SharePoint**
- Écrase les anciens fichiers
- Synchronisation complète

---

## 🧹 Méthode Cleanup() - Nettoyage à la fermeture

```csharp
public void Cleanup()
{
    if (!IsActive) return;

    try
    {
        // Sauvegarder vers SharePoint avant de nettoyer
        CopyToSharePoint();

        // Supprimer le fichier de verrouillage
        var lockPath = Path.Combine(_sharePointPath, ".lock");
        if (File.Exists(lockPath))
        {
            File.Delete(lockPath);
        }
    }
    catch (Exception ex)
    {
        // Log mais ne bloque pas la fermeture
    }
}
```

**Appelé par `Program.cs` à la fermeture de l'application**

**Actions :**
1. Sauvegarde finale vers SharePoint (avec checkpoint WAL)
2. Supprime le fichier `.lock`
3. Libère l'accès pour les autres utilisateurs

**Gestion des erreurs :**
- Si échec → Log l'erreur mais ferme quand même
- Évite de bloquer la fermeture de l'application

---

## 🔧 Méthode utilitaire CopyDirectory()

```csharp
private static void CopyDirectory(string sourceDir, string destDir)
{
    Directory.CreateDirectory(destDir);

    foreach (var file in Directory.GetFiles(sourceDir))
    {
        var dest = Path.Combine(destDir, Path.GetFileName(file));
        File.Copy(file, dest, overwrite: true);
    }

    foreach (var dir in Directory.GetDirectories(sourceDir))
    {
        var dest = Path.Combine(destDir, Path.GetDirectoryName(dir));
        CopyDirectory(dir, dest);
    }
}
```

**Copie récursive d'un dossier complet**

**Étapes :**
1. Crée le dossier destination
2. Copie tous les fichiers (écrase si existent déjà)
3. Pour chaque sous-dossier → Appel récursif

💡 **Écrase les fichiers** : `overwrite: true` garantit que la version la plus récente remplace l'ancienne.

---

## ⚠️ Exception SharePointLockException

```csharp
public class SharePointLockException : Exception
{
    public LockFile ExistingLock { get; }

    public SharePointLockException(LockFile existingLock)
        : base($"Base de données verrouillée par {existingLock.User} " +
               $"sur {existingLock.MachineName} depuis {existingLock.Timestamp}")
    {
        ExistingLock = existingLock;
    }
}
```

**Exception levée quand la base est déjà utilisée**

**Message d'erreur :**
```
Base de données verrouillée par jean.dupont sur PC-JEAN depuis 29/10/2025 14:30:00
```

**Contient l'objet `LockFile` pour afficher les détails à l'utilisateur**

---

## 🎬 Scénarios d'utilisation

### **Scénario 1 : Base de données locale normale**

```
1. Application démarre
   ↓
2. Database.cs initialise avec chemin : "C:\MonApp\database"
   ↓
3. SharePointSyncManager.Initialize(path)
   → Ne contient pas "OneDrive" ni "SharePoint"
   ↓
4. Retourne SyncManager avec IsActive = false
   ↓
5. Travail normal en local, pas de synchronisation
```

---

### **Scénario 2 : Premier utilisateur sur SharePoint**

```
1. Application démarre
   ↓
2. Database.cs initialise avec : "C:\Users\Jean\OneDrive\GestiParc\database"
   ↓
3. SharePointSyncManager.Initialize(path)
   → Détecte "OneDrive" dans le chemin
   ↓
4. Vérifie si fichier .lock existe
   → Non, c'est libre
   ↓
5. Crée dossier local : "%LocalAppData%\GestiParc\database_local"
   ↓
6. Copie depuis SharePoint vers local
   ↓
7. Crée fichier .lock sur SharePoint avec infos utilisateur
   ↓
8. Retourne SyncManager avec IsActive = true
   ↓
9. L'utilisateur travaille sur la copie locale (rapide)
   ↓
10. Bouton "Sauvegarder" → CopyToSharePoint()
    → Checkpoint WAL + Copie vers SharePoint
   ↓
11. Fermeture app → Cleanup()
    → Sauvegarde finale + Suppression du .lock
```

---

### **Scénario 3 : Deuxième utilisateur essaie d'accéder**

```
1. Application démarre (utilisateur 2)
   ↓
2. SharePointSyncManager.Initialize(path)
   → Détecte "OneDrive"
   ↓
3. Vérifie fichier .lock
   → Existe et créé il y a 10 minutes
   ↓
4. IsExpired() retourne false (< 4h)
   ↓
5. Lève SharePointLockException
   ↓
6. Program.cs affiche message :
   "Base verrouillée par jean.dupont sur PC-JEAN depuis 14:30"
   ↓
7. L'utilisateur 2 doit attendre
```

---

### **Scénario 4 : Récupération après crash**

```
1. Utilisateur 1 a crashé (pas de Cleanup)
   → Fichier .lock laissé sur SharePoint
   ↓
2. 5 heures plus tard, utilisateur 2 essaie d'accéder
   ↓
3. Vérifie .lock → Existe mais créé il y a 5h
   ↓
4. IsExpired() retourne true (> 4h)
   ↓
5. Program.cs affiche :
   "Verrouillage expiré (> 4h). Forcer l'accès ?"
   ↓
6. Si l'utilisateur confirme :
   → Supprime ancien .lock
   → Crée nouveau .lock
   → Accès accordé
```

---

### **Scénario 5 : Sauvegarde manuelle**

```
1. Utilisateur travaille depuis 30 minutes
   ↓
2. Clic sur bouton "💾 Sauvegarder" (barre d'outils)
   ↓
3. WelcomePage.SaveToSharePoint() appelle SyncManager.CopyToSharePoint()
   ↓
4. Checkpoint WAL :
   - Ouvre connexion à la base locale
   - Exécute "PRAGMA wal_checkpoint(TRUNCATE)"
   - Fusionne .db-wal dans .db principal
   ↓
5. Copie tous les fichiers vers SharePoint
   - bddProjetParc.db (avec toutes les modifs)
   - bddProjetParc.db-wal (vide)
   - bddProjetParc.db-shm
   ↓
6. Message de succès
   ↓
7. Les autres utilisateurs verront les modifications quand ils ouvriront l'app
```

---

## 🎓 Concepts clés

### **1. Mode WAL de SQLite**
- Les modifications sont écrites dans `.db-wal` d'abord
- Périodiquement fusionnées dans `.db` principal
- **CRUCIAL** : Faire un checkpoint avant de copier !

### **2. Système de verrouillage**
- Un seul utilisateur à la fois peut modifier
- Fichier `.lock` contient les infos de l'utilisateur actuel
- Timeout de 4h pour récupération automatique

### **3. Copie locale de travail**
- SharePoint/OneDrive synchronise automatiquement en arrière-plan
- Peut être lent ou créer des conflits
- Solution : Copie locale + Sauvegarde manuelle contrôlée

### **4. Gestion d'erreurs robuste**
- Détection des chemins SharePoint/OneDrive
- Vérification des locks avant accès
- Messages clairs à l'utilisateur

### **5. Sauvegarde explicite**
- Bouton visible dans la barre d'outils
- Confirmation à la fermeture
- Contrôle total pour l'utilisateur

---

## 💡 Questions fréquentes

**Q : Pourquoi ne pas utiliser la synchronisation automatique de OneDrive ?**
- R : OneDrive synchronise en arrière-plan. Si deux personnes modifient, risque de conflit de fichiers. Notre système de lock évite ça.

**Q : Que se passe-t-il si quelqu'un force le lock alors que l'autre travaille encore ?**
- R : Risque de perte de données ! C'est pourquoi il faut vraiment 4h d'inactivité. Ne forcer que si sûr que personne n'utilise.

**Q : Pourquoi 4 heures de timeout ?**
- R : Compromis. Assez long pour éviter les faux positifs (pause déjeuner), assez court pour ne pas bloquer trop longtemps après un crash.

**Q : Les fichiers .db-wal et .db-shm sont-ils importants ?**
- R : Le .db-wal OUI (contient les modifications). Le .db-shm non (mémoire temporaire). Mais on copie tout pour être sûr.

**Q : Peut-on travailler sans connexion SharePoint ?**
- R : Oui ! La copie locale fonctionne offline. Mais il faudra sauvegarder quand la connexion reviendra.

**Q : Que contient exactement le fichier .lock ?**
- R : Nom utilisateur, nom machine, date/heure, ID processus. Format JSON lisible.

---

## 🔗 Fichiers liés

- **`Database.cs`** - Utilise `SharePointSyncManager.Initialize()` au démarrage
- **`Program.cs`** - Gère les exceptions de lock et appelle `Cleanup()` à la fermeture
- **`WelcomePage.cs`** - Bouton "Sauvegarder" appelle `CopyToSharePoint()`
- **`LockFile.cs`** - Classe pour le fichier de verrouillage (même fichier)

---

## 📊 Schéma d'architecture

```
SharePoint/OneDrive
├─ database/
│  ├─ bddProjetParc.db
│  ├─ bddProjetParc.db-wal
│  ├─ bddProjetParc.db-shm
│  └─ .lock ← Fichier de verrouillage
│
│  [Copie au démarrage ↓]
│
└─ Local (%LocalAppData%\GestiParc\database_local)
   ├─ bddProjetParc.db ← TRAVAIL ICI (rapide)
   ├─ bddProjetParc.db-wal
   └─ bddProjetParc.db-shm
   
   [Checkpoint WAL ↓]
   
   Toutes les modifications fusionnées dans .db
   
   [Sauvegarde manuelle ↑]
   
   Copie vers SharePoint
```

---

**📌 Prochaine étape :** Consulter `Program.cs` pour voir la gestion des erreurs de lock, ou `WelcomePage.cs` pour le bouton de sauvegarde.

---

**🆕 Ajouté dans la version 1.1.0**

*Dernière mise à jour : 29 octobre 2025*
