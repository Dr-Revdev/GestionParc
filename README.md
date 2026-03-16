# GestiParc (refactor) — Documentation unique

## 1) Vue d’ensemble

GestiParc est composé de 3 briques principales :

- **GestiParc.Ui** : client lourd (WinForms/WPF). Il parle à l’API via HTTP(S).
- **GestiParc.Api** : API ASP.NET Core (JWT + accès MySQL via repositories).
- **GestiParc.Core / GestiParc.Infrastructure** : bibliothèques partagées.
  - **Core** : DTOs, entités, interfaces.
  - **Infrastructure** : implémentations (repositories MySQL, BCrypt, etc.).

### Important : CORS
Ce projet n’implémente pas CORS et **n’en a pas besoin** tant que l’unique client est **un client lourd** (.NET). CORS concerne uniquement les navigateurs.

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

#### Connexion MySQL (obligatoire)
L’API lit **la chaîne de connexion** dans :

- `ConnectionStrings__GestiParcDb` (recommandé en prod)
- ou `ConnectionStrings:GestiParcDb` via `appsettings.json` (possible, mais le fichier est volontairement vide par défaut)

Exemple de connection string MySqlConnector :

```text
Server=127.0.0.1;Port=3306;Database=gestiparc;User ID=app_user;Password=xxx;SslMode=Required;
```

#### JWT (obligatoire)
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

