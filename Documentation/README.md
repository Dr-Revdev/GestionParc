# 📚 Documentation Complète du Projet GestiParc

## 🎯 Bienvenue !

Cette documentation a été créée pour que **n'importe qui**, même sans expérience en programmation, puisse comprendre comment fonctionne ce projet.

Chaque fichier est expliqué **ligne par ligne**, avec des analogies de la vie quotidienne et des schémas.

---

## 📖 Table des matières

### 🏗️ Fichiers de base

1. **[Program.cs](01_Program.md)** ⭐ COMMENCER ICI
   - Le point de départ de l'application
   - Comment le programme démarre
   - La gestion des erreurs de démarrage
   - **Durée de lecture :** ~15 minutes
   - **Niveau :** Débutant

2. **[DataBase.cs](02_DataBase.md)**
   - Comment on se connecte à la base de données
   - Création automatique des dossiers et fichiers
   - Configuration du chemin de la base (pour SharePoint)
   - Configuration optimale de SQLite
   - **Durée de lecture :** ~20 minutes
   - **Niveau :** Débutant-Intermédiaire

2bis. **[CsvExporter.cs](02bis_CsvExporter.md)** 🆕
   - Export des données en fichiers CSV
   - Formats d'export (Agents, Équipements, Prêts actifs)
   - Export complet avec plusieurs fichiers
   - **Durée de lecture :** ~15 minutes
   - **Niveau :** Débutant

2ter. **[SharePointSyncManager.cs](17_SharePointSync.md)** 🆕 v1.1.0
   - Synchronisation multi-utilisateur SharePoint/OneDrive
   - Système de verrouillage (lock files)
   - Copie locale de travail pour performance
   - Checkpoint WAL pour sauvegarde fiable
   - **Durée de lecture :** ~25 minutes
   - **Niveau :** Intermédiaire-Avancé

---

### 🖼️ Fichiers des vues (Interface graphique)

#### Vue principale
3. **[WelcomePage.cs](03_WelcomePage.md)** ✅ Terminé
   - La page d'accueil de l'application
   - Les trois boutons principaux
   - Navigation entre les écrans
   - Barre d'outils avec export CSV 🆕
   - Bouton de sauvegarde SharePoint 🆕 v1.1.0
   - Confirmation de sauvegarde à la fermeture 🆕 v1.1.0
   - **Durée de lecture :** ~25 minutes
   - **Niveau :** Débutant

#### Vues d'administration
4. **[AdminMenuView.cs](04_AdminMenuView.md)** ✅ Terminé
   - Le menu pour les administrateurs
   - Accès aux fonctions de création/modification
   - **Durée de lecture :** ~20 minutes
   - **Niveau :** Débutant

#### Vues des agents
5. **[AgentCreateView.cs](05_AgentCreateView.md)** ✅ Terminé
   - Formulaire de création d'un nouvel agent
   - **Durée de lecture :** ~30 minutes
   - **Niveau :** Intermédiaire
   
6. **[AgentEditView.cs](06_AgentEditView.md)** ✅ Terminé
   - Modifier ou supprimer un agent existant
   - **Durée de lecture :** ~35 minutes
   - **Niveau :** Intermédiaire

#### Vues des équipements
7. **[EquipmentCreateView.cs](07_EquipmentCreateView.md)** ✅ Terminé
   - Formulaire de création d'un nouvel équipement
   - **Durée de lecture :** ~25 minutes
   - **Niveau :** Intermédiaire

8. **[EquipmentEditView.cs](08_EquipmentEditView.md)** ✅ Terminé
   - Modifier ou supprimer un équipement
   - **Durée de lecture :** ~35 minutes
   - **Niveau :** Intermédiaire

9. **[FreeEquipmentView.cs](09_FreeEquipmentView.md)** ✅ Terminé
   - Voir les équipements disponibles et rendus
   - Marquer comme "Rendu DSEM"
   - **Durée de lecture :** ~30 minutes
   - **Niveau :** Intermédiaire

#### Vues de prêt et inventaire
10. **[LoanCreationView.cs](10_LoanCreationView.md)** ✅ Terminé
    - Créer un nouveau prêt d'équipement
    - **Durée de lecture :** ~30 minutes
    - **Niveau :** Intermédiaire-Avancé

11. **[EquipmentSelectionControl.cs](11_EquipmentSelectionControl.md)** ✅ Terminé
    - Contrôle réutilisable pour sélectionner un équipement
    - **Durée de lecture :** ~15 minutes
    - **Niveau :** Intermédiaire

12. **[MainInventoryView.cs](12_MainInventoryView.md)** ✅ Terminé
    - Vue principale de l'inventaire complet
    - Liste des prêts en cours
    - **Durée de lecture :** ~30 minutes
    - **Niveau :** Intermédiaire-Avancé

#### Modèles (Classes de données)
13. **[AgentItem.cs](13_AgentItem.md)** ✅ Terminé
    - Classe modèle pour représenter un agent dans les listes
    - **Durée de lecture :** ~15 minutes
    - **Niveau :** Débutant-Intermédiaire

14. **[EquipmentItem.cs](14_EquipmentItem.md)** ✅ Terminé
    - Classe modèle pour représenter un équipement dans les listes
    - **Durée de lecture :** ~15 minutes
    - **Niveau :** Débutant-Intermédiaire

#### Vues de paramètres
15. **[SettingsView.cs](14_SettingsView.md)** ✅ Terminé
    - Configuration de l'application
    - Gestion des paramètres (équipes, sites, types d'équipements)
    - **Durée de lecture :** ~30 minutes
    - **Niveau :** Intermédiaire

#### Utilitaires
16. **[ListViewColumnSorter.cs](15_ListViewColumnSorter.md)** ✅ Terminé 🆕
    - Classe utilitaire pour trier les colonnes de ListView
    - Tri alphabétique et numérique intelligent
    - Utilisé dans toutes les vues avec tableaux
    - **Durée de lecture :** ~15 minutes
    - **Niveau :** Intermédiaire

---

## 🎓 Comment utiliser cette documentation

### Pour les débutants complets

Si tu n'as **jamais programmé** :

1. **Commence par** [Program.cs](01_Program.md)
   - C'est le plus court et le plus simple
   - Tu comprendras comment démarre un programme

2. **Ensuite** [DataBase.cs](02_DataBase.md)
   - Un peu plus technique, mais très bien expliqué
   - Tu apprendras comment on gère les données

3. **Puis** les vues dans l'ordre qui t'intéresse
   - Chaque vue est indépendante
   - Tu peux les lire dans n'importe quel ordre

### Pour ceux qui ont des bases

Si tu connais déjà un peu la programmation :

- Tu peux lire dans n'importe quel ordre
- Les concepts avancés sont marqués avec 🔥
- Les liens entre fichiers sont indiqués

### Légende des symboles

- ⭐ = À lire en premier
- 🚧 = En cours de rédaction
- 🔥 = Concept avancé
- 💡 = Astuce importante
- ⚠️ = Attention, piège courant
- 📌 = Point clé à retenir

---

## 🗂️ Structure du projet

```
GestiParc/
│
├── 📄 Program.cs ⭐
│   └─ Point d'entrée de l'application
│
├── 📁 Data/
│   ├── 📄 DataBase.cs
│   │   └─ Gestion de la base de données
│   ├── 📄 CsvExporter.cs
│   │   └─ Export des données en fichiers CSV
│   ├── 📄 SharePointSyncManager.cs 🆕 v1.1.0
│   │   └─ Synchronisation SharePoint/OneDrive
│   ├── 📄 LockFile.cs 🆕 v1.1.0
│   │   └─ Gestion des verrous multi-utilisateur
│   └── 📄 AppConfig.cs
│       └─ Configuration de l'application
│
├── 📁 Views/
│   ├── 📄 WelcomePage.cs
│   │   └─ Page d'accueil
│   │
│   ├── � ListViewColumnSorter.cs 🆕
│   │   └─ Utilitaire de tri pour ListView
│   │
│   ├── �📁 Admin/
│   │   └── 📄 AdminMenuView.cs
│   │
│   ├── 📁 Agent/
│   │   ├── 📄 AgentCreateView.cs
│   │   └── 📄 AgentEditView.cs
│   │
│   ├── 📁 Equipment/
│   │   ├── 📄 EquipmentCreateView.cs
│   │   ├── 📄 EquipmentEditView.cs
│   │   └── 📄 FreeEquipmentView.cs
│   │
│   ├── 📁 Inventory/
│   │   └── 📄 MainInventoryView.cs
│   │
│   ├── 📁 Settings/
│   │   └── 📄 SettingsView.cs
│   │
│   └── 📁 Loan/
│       ├── 📄 LoanCreationView.cs
│       ├── 📄 EquipmentSelectionControl.cs
│       └── 📁 Models/
│           ├── 📄 AgentItem.cs
│           └── 📄 EquipmentItem.cs
│
├── 📁 Docs/
│   └── Diagrammes et maquettes
│
└── 📁 database/
    └── 📄 bddGestiParc.db
        └─ Fichier de base de données SQLite
```

---

## 🔗 Flux de l'application

```
┌─────────────────┐
│   DÉMARRAGE     │
│   Program.cs    │
└────────┬────────┘
         │
         ├─► Initialise la base de données
         │   (DataBase.cs)
         │
         └─► Ouvre WelcomePage.cs
                    │
                    ├─► Barre d'outils : Bouton "Export CSV" 🆕
                    │   └─► Menu d'export (CsvExporter.cs)
                    │       ├─► Export Agents
                    │       ├─► Export Équipements
                    │       ├─► Export Prêts actifs
                    │       └─► Export complet (3 fichiers)
                    │
                    ├─► Bouton "Prêt Equipement"
                    │   └─► MainInventoryView.cs
                    │       └─► LoanCreationView.cs
                    │
                    ├─► Bouton "Equipement libre"
                    │   └─► FreeEquipmentView.cs
                    │
                    └─► Bouton "Création/Modification"
                        └─► AdminMenuView.cs
                            ├─► AgentCreateView.cs
                            ├─► AgentEditView.cs
                            ├─► EquipmentCreateView.cs
                            └─► EquipmentEditView.cs
```

---

## 💡 Concepts clés expliqués

### Qu'est-ce qu'une "classe" ?
Une classe est comme un **plan de construction**. Par exemple, la classe `Database` est le plan pour créer et gérer des connexions à la base de données.

### Qu'est-ce qu'une "méthode" ?
Une méthode est une **action** que peut faire une classe. Par exemple, `Open()` est une action qui ouvre une connexion.

### Qu'est-ce qu'un "namespace" ?
C'est comme une **adresse postale** pour organiser le code. `GestiParc.Data` signifie "dans le projet GestiParc, dans le dossier Data".

### Qu'est-ce qu'une "base de données" ?
C'est un endroit où on **stocke des informations** de manière organisée. Comme un classeur géant avec des dossiers et des fiches.

---

## 🛠️ Technologies utilisées

### C# (C-Sharp)
Le langage de programmation principal. Créé par Microsoft, très populaire pour les applications Windows.

### Windows Forms
Bibliothèque pour créer des **interfaces graphiques** (fenêtres, boutons, formulaires).

### SQLite
Système de base de données **léger** et **portable**. Tout est dans un seul fichier `.db`.

### CSV (Comma-Separated Values) 🆕
Format de fichier texte pour exporter des données. Utilisé pour :
- Partager les données avec Excel, Google Sheets
- Archiver des rapports
- Analyser les données dans d'autres outils
- **Format utilisé :** Point-virgule (`;`) comme séparateur, UTF-8 avec BOM

### .NET 9.0
La plateforme sur laquelle tourne l'application. Version la plus récente.

---

## 📚 Ressources pour aller plus loin

### Si tu veux apprendre C#
- [Microsoft Learn - C# pour débutants](https://docs.microsoft.com/fr-fr/learn/paths/csharp-first-steps/)
- [OpenClassrooms - Apprenez à programmer en C#](https://openclassrooms.com/)

### Si tu veux comprendre les bases de données
- [SQLite Tutorial](https://www.sqlitetutorial.net/)
- [W3Schools - SQL](https://www.w3schools.com/sql/)

### Si tu veux créer des interfaces graphiques
- [Microsoft Docs - Windows Forms](https://docs.microsoft.com/fr-fr/dotnet/desktop/winforms/)

### Si tu veux comprendre les ListView et le tri 🆕
- ListView : Affichage de données sous forme de tableau avec colonnes
- Tri : Organisation automatique par ordre alphabétique ou numérique
- Notre implémentation : `ListViewColumnSorter.cs` (voir documentation)

---

## 🤝 Comment contribuer

Si tu veux améliorer cette documentation :

1. Lis un fichier de documentation
2. Note ce qui n'est pas clair
3. Propose une amélioration
4. On ajoutera tes suggestions !

---

## ❓ Questions fréquentes générales

**Q : Dois-je tout lire d'un coup ?**
- R : Non ! Lis à ton rythme. Chaque fichier peut être lu indépendamment.

**Q : C'est normal si je ne comprends pas tout ?**
- R : Oui, complètement ! La programmation demande du temps. Relis plusieurs fois si nécessaire.

**Q : Où puis-je poser des questions ?**
- R : Note tes questions et on pourra créer une section FAQ spécifique.

**Q : Puis-je modifier le code en lisant ?**
- R : Oui ! La meilleure façon d'apprendre est d'expérimenter. Fais des copies de sauvegarde avant.

---

## 🎯 Objectif de cette documentation

L'objectif est simple : **rendre le code accessible à tous**.

Que tu sois :
- Un étudiant qui débute
- Un collègue qui reprend le projet
- Un curieux qui veut comprendre

Tu devrais pouvoir **comprendre** et **modifier** ce code en lisant cette documentation.

---

## 📞 Support

Pour toute question ou suggestion d'amélioration :
- Crée un fichier `QUESTIONS.md` avec tes interrogations
- On y répondra dans la documentation

---

**🚀 Bonne lecture et bon apprentissage !**

---

## 📝 Historique des versions

### **v1.1.0** - 29 octobre 2025 🆕
- ✨ Synchronisation SharePoint/OneDrive multi-utilisateur
- 🔒 Système de verrouillage avec fichiers .lock
- 💾 Bouton de sauvegarde manuelle dans la barre d'outils
- ✅ Confirmation de sauvegarde à la fermeture avec annulation
- 🔧 Checkpoint WAL SQLite pour sauvegarde fiable
- 📂 Copie locale de travail pour meilleures performances
- ⏱️ Récupération automatique des verrous expirés (4h)

### **v1.0.x** - Octobre 2025
- 📊 Export CSV (Agents, Équipements, Prêts)
- 🔄 Export complet avec dossier horodaté
- 📋 Tri intelligent des colonnes ListView
- 🎨 Interface graphique complète
- 💾 Gestion complète de l'inventaire
- 👥 Gestion des agents
- 🖥️ Gestion des équipements
- 📝 Système de prêts

---

*Dernière mise à jour : 29 octobre 2025*
