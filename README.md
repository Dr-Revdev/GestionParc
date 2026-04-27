# GestiParc

Application de gestion de parc informatique permettant d'inventorier les équipements, de suivre les prêts et de gérer les agents et les équipes.  
Projet réalisé dans le cadre du BTS SIO.

## Problématique

Une entreprise souhaite centraliser la gestion de son parc informatique au sein d'une application unique.

Les besoins identifiés sont les suivants :
- inventorier les équipements informatiques et suivre leur état ;
- associer les équipements aux agents et gérer les prêts ;
- organiser les agents par équipes et par sites ;
- permettre à un administrateur de gérer les utilisateurs et les accès.

## Fonctionnalités

- Authentification des utilisateurs
- Gestion des équipements (création, consultation, modification, suppression)
- Suivi des prêts et de l'état des équipements
- Gestion des agents
- Gestion des équipes et des sites
- Gestion des types d'équipements
- Administration des comptes utilisateurs
- Export CSV
- Journalisation des actions

## Rôles utilisateurs

- **Administrateur** : gère les utilisateurs, les rôles et l'ensemble des données de l'application
- **Utilisateur** : consulte et modifie les équipements et les agents

## Technologies utilisées

- **Client lourd** : Windows Forms, .NET 9
- **Backend** : ASP.NET Core Web API, .NET 9
- **Base de données** : MySQL
- **Authentification** : JWT

## Choix techniques

- **.NET 9** a été utilisé pour bénéficier des dernières fonctionnalités du framework et des performances améliorées.
- **ASP.NET Core** permet de structurer l'API REST de manière claire, avec une séparation propre entre les contrôleurs, les services et les repositories.
- **Windows Forms** a été choisi pour le client lourd afin de proposer une interface native adaptée à l'environnement Windows de l'entreprise.
- **MySqlConnector** assure l'accès direct à la base de données MySQL sans couche ORM, pour un contrôle total des requêtes.
- **JWT** permet de sécuriser l'authentification et l'accès aux routes protégées.
- **BCrypt** garantit le stockage sécurisé des mots de passe par hachage.
- **Serilog** assure une journalisation structurée avec rotation quotidienne des fichiers de logs.
- **xUnit** est utilisé pour les tests unitaires et d'intégration.

## Installation

### Prérequis

- .NET 9 SDK
- MySQL

### Étapes

1. Cloner le dépôt :

```bash
git clone <url-du-depot>
cd GestiParc_refactor
```

2. Initialiser la base de données MySQL à l'aide du script fourni :

```bash
mysql -u root -p < Docs/rebuild_database.sql
```

Compte administrateur créé par défaut :

```
username : admin
password : AdminTemp!2026
```

Le mot de passe doit être changé dès le premier login.

3. Configurer les variables d'environnement de l'API :

```powershell
$env:ConnectionStrings__GestiParcDb = "Server=127.0.0.1;Port=3306;Database=gestiparc;User ID=xxx;Password=xxx;SslMode=Preferred;"
$env:Jwt__Secret = "un_secret_minimum_32_caracteres_ici"
```

Ou créer un fichier `.env` à la racine de `GestiParc.Api/` en développement.

4. Lancer l'API :

```bash
dotnet run --project GestiParc.Api/GestiParc.Api.csproj
```

5. Configurer l'URL de l'API dans le fichier `GestiParc.Ui/App.config` :

```xml
<add key="ApiBaseUrl" value="http://localhost:5139" />
```

6. Lancer ou compiler le client lourd :

```bash
dotnet run --project GestiParc.Ui/GestiParc.Ui.csproj
```

## Configuration

### API

L'API nécessite deux variables obligatoires au démarrage :

- `ConnectionStrings__GestiParcDb` : chaîne de connexion MySQL
- `Jwt__Secret` : secret JWT (minimum 32 caractères)

En développement, ces variables peuvent être définies dans un fichier `.env` placé dans `GestiParc.Api/`.  
En production, elles doivent être fournies via les variables d'environnement ou les secrets Docker.

### Client lourd

L'URL de l'API est définie dans `GestiParc.Ui/App.config` sous la clé `ApiBaseUrl`.  
Au démarrage, le client vérifie la connexion à l'API avant d'afficher l'écran de login.
Le client est téléchargeable dans le dossier installer

## Utilisation

Une fois l'application démarrée, il est possible de :
- se connecter avec un compte utilisateur ;
- consulter et gérer l'inventaire des équipements ;
- suivre les prêts et les retours d'équipements ;
- gérer les agents, les équipes et les sites ;
- administrer les comptes utilisateurs (rôles, mots de passe, activation).

## Structure du projet

```text
GestiParc_refactor/
├── GestiParc.Api/
├── GestiParc.Core/
├── GestiParc.Infrastructure/
├── GestiParc.Ui/
├── GestiParc.Tests/
├── Docs/
│   ├── rebuild_database.sql
│   └── seed_bulk_test_data.sql
└── GestiParc.sln
```

## Organisation technique

- `GestiParc.Api/` : API REST ASP.NET Core — contrôleurs, configuration, point d'entrée
- `GestiParc.Core/` : couche domaine partagée — entités, DTOs, interfaces
- `GestiParc.Infrastructure/` : couche d'accès aux données — repositories MySQL, hachage BCrypt
- `GestiParc.Ui/` : client lourd Windows Forms — interface utilisateur
- `GestiParc.Tests/` : tests unitaires et d'intégration xUnit
- `Docs/` : scripts SQL de reconstruction et de données de test

## Tests

Des tests unitaires et d'intégration ont été mis en place à l'aide de **xUnit**.  
Ils couvrent notamment le service JWT, l'export CSV et les politiques d'autorisation de l'API.

## Limites actuelles

- Absence de pagination
- Client lourd fonctionnel uniquement sous Windows
- Schéma de base de données géré manuellement (pas de système de migrations)
- Gestion des erreurs encore perfectible

## Pistes d'amélioration

- Mise en place d'un système de migrations pour la base de données
- Ajout d'une interface web en complément du client lourd
- Extension de la couverture des tests
- Mise en place d'une chaîne CI/CD
- Amélioration de la gestion des erreurs

## Auteur

Projet réalisé dans le cadre du BTS SIO.