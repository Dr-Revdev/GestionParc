# GestiParc (refactor) — Documentation unique

## 1) Vue d’ensemble

GestiParc est composé de 3 briques principales :

- **GestiParc.Ui** : client lourd (WinForms). Il parle à l’API via HTTP(S).
- **GestiParc.Api** : API ASP.NET Core (JWT + accès MySQL via repositories).
- **GestiParc.Core / GestiParc.Infrastructure** : bibliothèques partagées.
  - **Core** : DTOs, entités, interfaces.
  - **Infrastructure** : implémentations (repositories MySQL, BCrypt, etc.).

---

## 2) Prérequis

### Pour développer sur Windows
- .NET SDK 9 (pour build/run)
- MySQL accessible (local ou distant)

### Pour exécuter côté utilisateur (UI)
- .NET 9 Desktop Runtime (si déploiement framework-dependent)

---

## 3) Configuration — API

L’API **refuse de démarrer** si la connexion DB ou le secret JWT ne sont pas fournis.

### 3.1 Variables/Secrets attendus

#### Connexion MySQL
L’API lit **la chaîne de connexion** dans :

- `ConnectionStrings__GestiParcDb` 

Exemple de connection string MySqlConnector :

```text
Server=127.0.0.1;Port=3306;Database=gestiparc;User ID=app_user;Password=xxx;SslMode=Required;
```

#### JWT
L’API lit le secret JWT depuis :

- `Jwt__Secret` (préféré, correspond à `Jwt:Secret`)
- ou `JWT_SECRET`

Contraintes : **au moins 32 caractères**.

#### Options utiles
- `LOG_DIR` : dossier des logs (par défaut `logs`)
- `DOCKER_SECRETS_DIR` : dossier de secrets Docker (par défaut `/run/secrets`)
- `GESTIPARC_DOTENV_PATH` : chemin explicite vers un fichier `.env` (chargé uniquement en `Development`)

### 3.2 Lancement en local (PowerShell)

Dans un terminal à la racine du repo :

```powershell
$env:ConnectionStrings__GestiParcDb = "Server=127.0.0.1;Port=3306;Database=gestiparc;User ID=app_user;Password=xxx;SslMode=Preferred;"
$env:Jwt__Secret = "change-me-change-me-change-me-change-me"

dotnet run --project .\GestiParc.Api\GestiParc.Api.csproj
```

### 3.3 Ports en développement

Les profils de debug déclarent :
- HTTP : `http://localhost:5139`
- HTTPS : `https://localhost:7256`

Test rapide :

```powershell
curl http://localhost:5139/api/ping
```

### 3.4 Reconstruction rapide de la base

Si la base a été perdue, le dépôt contient un script de reconstruction minimale compatible avec l'application :

- [Docs/rebuild_database.sql](Docs/rebuild_database.sql)
- [Docs/seed_bulk_test_data.sql](Docs/seed_bulk_test_data.sql)

Exemple avec le client MySQL :

```powershell
mysql -u root -p < .\Docs\rebuild_database.sql
```

Le script recrée :

- les tables `sites`, `equipes`, `equipment_type`, `utilisateurs`, `agents`, `equipements`
- les clés étrangères utilisées par le code actuel
- un compte admin de secours

Compte initial créé par le script :

```text
username: admin
password: AdminTemp!2026
```

Après le premier login, il faut changer immédiatement ce mot de passe.

Pour charger des données de démonstration après reconstruction :

```powershell
mysql -u root -p gestiparc < .\Docs\seed_bulk_test_data.sql
```

---

## 4) Authentification (JWT)

### 4.1 Connexion
Endpoint :
- `POST /api/utilisateur/authentifier` (anonyme)

Payload :

```json
{
  "username": "...",
  "password": "..."
}
```

Réponse :

```json
{
  "user": { "id": 1, "username": "...", "role": "ADMIN" },
  "token": "<jwt>",
  "expiresIn": 28800
}
```

### 4.2 Utilisation du token
Les endpoints (hors ping + authentification) nécessitent :

```text
Authorization: Bearer <token>
```

Notes (implémentation actuelle) :
- `sub` = identifiant utilisateur (int) — utilisé pour l’audit/log.
- claim `role` = `ADMIN` ou `USER`.
- durée : 8h (configurée en dur dans `Program.cs`).

---

## 5) Configuration — UI (client lourd)

L’UI lit la base URL de l’API dans `App.config` :

```xml
<appSettings>
  <add key="ApiBaseUrl" value="https://localhost:7256" />
</appSettings>
```

Au démarrage, l’UI :
- valide l’URL
- appelle `GET api/ping`
- puis ouvre la page de login

### Token côté UI
Après login, l’UI :
- met le header `Authorization: Bearer ...` sur son `HttpClient`
- persiste le token via DPAPI Windows (chiffrement lié à l’utilisateur Windows)

---

## 6) Logs & audit

### API
- Serilog écrit en console + fichiers rolling journaliers.
- Emplacement : `LOG_DIR` (défaut `logs`).

### Audit
Un filtre d’audit log les opérations mutantes (POST/PUT/PATCH/DELETE) en incluant :
- opération (Create/Update/Delete)
- type d’entité (contrôleur)
- id détecté (route ou payload)
- statut HTTP

---

