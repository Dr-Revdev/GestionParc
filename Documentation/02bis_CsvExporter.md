# 📘 Documentation de CsvExporter.cs

## 🎯 But de ce fichier
Cette classe gère **l'export des données** de l'application en fichiers CSV (Comma-Separated Values). Elle permet de générer des rapports au format tableur pour Excel, LibreOffice, etc.

💡 **Analogie :** C'est comme un photocopieur qui transforme les données de la base en fichiers Excel.

---

## 🎨 Formats d'export disponibles

### 1. **Export Agents** (`ExportAgents`)
Liste complète de tous les agents avec leurs informations :
- IDRH, Nom, Prénom, Email
- Équipe, Site, Hébergé
- Commentaire

### 2. **Export Équipements** (`ExportEquipements`)
Liste de tous les équipements avec :
- Type, Nom, Code Parc
- Numéro de série, Marque
- État (Disponible/En prêt/Rendu DSEM)
- Agent assigné

### 3. **Export Prêts actifs** (`ExportPrets`)
**Format spécial :** Une ligne par agent, avec ses équipements en colonnes séparées
- IDRH Agent, Nom Agent, Prénom Agent, Email
- Équipe, Site, Hébergé
- Pour chaque équipement :
  - Type Équipement N
  - Nom Équipement N
  - Code Parc N
  - Numéro de série N
  - Marque N
  - Commentaire Équipement N

### 4. **Export Complet** (`ExportComplet`)
Génère un dossier avec :
- `Agents.csv`
- `Equipements.csv`
- `Prets_Actifs.csv`
- `README.txt` (explications)

---

## 📋 Fonctionnalités clés

### Gestion des caractères spéciaux
Le code échappe automatiquement :
- Les points-virgules (`;`)
- Les guillemets (`"`)
- Les retours à la ligne

### Format CSV
- **Séparateur :** Point-virgule (`;`) - standard européen
- **Encodage :** UTF-8 avec BOM - compatible avec Excel français
- **Extension :** `.csv`

### Interface utilisateur
- Dialogue de sélection de fichier
- Messages de confirmation après export
- Ouverture automatique du dossier (pour export complet)

---

## 🎬 Utilisation depuis l'interface

### Via la barre d'outils
1. Cliquer sur **"Export CSV"** en haut de l'écran
2. Choisir le type d'export souhaité
3. Sélectionner l'emplacement de sauvegarde
4. Confirmation du succès

---

## 💡 Méthodes principales

### `SelectExportFile(string defaultName)`
Affiche un dialogue pour choisir où enregistrer le fichier CSV.

### `SelectExportFolder()`
Affiche un dialogue pour choisir un dossier (pour l'export complet).

### `WriteCsv(string filePath, SqliteDataReader reader)`
Méthode utilitaire qui écrit les données du reader dans le fichier CSV.

### `WriteDynamicCsvForLoans(...)`
Méthode spécialisée pour le format "une ligne par agent" avec colonnes dynamiques pour les équipements.

---

## 🔗 Fichiers liés

- **WelcomePage.cs** - Contient le menu d'export et la barre d'outils
- **DataBase.cs** - Utilisé pour les requêtes SQL

---

## 🎓 Concepts clés

### Format CSV
- CSV = Comma-Separated Values (valeurs séparées par des virgules/points-virgules)
- Peut être ouvert dans Excel, LibreOffice Calc, Google Sheets
- Format texte simple et universel

### Pivot dynamique
Pour les prêts, on transforme :
```
Ligne 1: Agent A, Équipement 1
Ligne 2: Agent A, Équipement 2
```

En :
```
Ligne 1: Agent A, Équipement 1 (colonnes), Équipement 2 (colonnes)
```

### Échappement
Si une donnée contient `;` ou `"`, on l'entoure de guillemets :
```
Normal: Jean;Dupont;Paris
Avec ;: Jean;Dupont;"Paris;France"
```

---

## 💡 Questions fréquentes

**Q : Pourquoi utiliser `;` et pas `,` ?**
- R : En Europe, Excel utilise `;` par défaut. Le `,` est réservé aux décimales (ex: 1,5).

**Q : Comment ouvrir le fichier dans Excel ?**
- R : Double-clic sur le fichier `.csv`, ou Fichier > Ouvrir dans Excel.

**Q : Le fichier s'ouvre en "bizarre" dans Excel ?**
- R : Excel peut avoir du mal avec l'UTF-8. Essayer : Données > Importer depuis CSV > choisir UTF-8.

**Q : Peut-on modifier le format d'export ?**
- R : Oui, en modifiant les requêtes SQL dans chaque méthode `Export*`.

---

**📌 Prochaine étape :** Retour à [WelcomePage.cs](03_WelcomePage.md) pour voir l'intégration du menu d'export.
