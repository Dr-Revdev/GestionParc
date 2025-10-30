# Feuilles de Remise d'Équipement

## Vue d'ensemble

La fonctionnalité "Feuilles de remise" permet de générer un document officiel listant tous les équipements en prêt pour un agent donné. Cette fonctionnalité était requise dans le cahier des charges initial mais n'avait pas encore été implémentée.

## Accès à la fonctionnalité

### Option 1 : Menu contextuel (clic droit)

1. **Naviguer vers l'inventaire** depuis le menu principal
2. **Faire un clic droit sur une ligne de prêt** (agent ayant des équipements en prêt)
3. **Sélectionner "📄 Générer feuille de remise"** dans le menu contextuel

### Option 2 : Depuis la fenêtre d'édition de prêt

1. **Naviguer vers l'inventaire** depuis le menu principal
2. **Double-cliquer sur une ligne de prêt** pour ouvrir la fenêtre d'édition
3. **Cliquer sur "📄 Feuille de remise"** dans la barre de boutons
   - Ce bouton n'est disponible qu'en mode édition (pas en création de nouveau prêt)

## Fonctionnement

### Génération du document

La feuille de remise générée contient :

**En-tête :**
- Titre du document
- Date et heure de génération
- Ligne de séparation

**Informations Agent :**
- Nom et prénom
- IDRH (identifiant)
- Email
- Site d'affectation
- Équipe

**Liste des équipements :**
- Tableau avec colonnes : Type, Nom/Code, Série, Marque
- Total du nombre d'équipements
- Seuls les équipements en prêt (état = 1) sont listés

**Zones de signature :**
- Signature de l'agent (avec mention "Lu et approuvé")
- Signature du responsable (avec mention "Remise validée")
- Note légale en bas de page

### Options de sortie

Après génération, l'utilisateur peut :

1. **Prévisualiser** le document dans une fenêtre d'aperçu avant impression
2. **Sauvegarder en PDF** dans `Documents/GestiParc/FeuillesRemise/`
3. **Imprimer directement** sur une imprimante

### Nomenclature des fichiers

Les fichiers PDF sont nommés selon le format :
```
FeuilleRemise_{IDRH}_{YYYYMMDD_HHMMSS}.pdf
```

Exemple : `FeuilleRemise_12345_20251030_143022.pdf`

## Cas d'usage

### Utilisation en entreprise

- **Remise officielle d'équipement** lors de l'arrivée d'un nouvel agent
- **Contrôle périodique** des équipements en possession des agents
- **Audit de matériel** pour vérification des responsabilités
- **Archivage** des prêts pour la comptabilité/gestion

### Utilisation pour l'E6

Cette fonctionnalité démontre :
- **Génération de documents** (PDF, impression)
- **Intégration base de données** (requêtes multi-tables)
- **Interface utilisateur** (onglets, boutons)
- **Gestion des erreurs** et feedback utilisateur
- **Respecter les spécifications** du cahier des charges

## Architecture technique

### Classes impliquées

- **`FeuilleRemiseGenerator`** : Classe principale de génération
- **`MainInventoryView`** : Interface utilisateur (menu contextuel clic droit)
- **`LoanCreationView`** : Interface utilisateur (bouton dans fenêtre d'édition)
- **`Database`** : Accès aux données agents et équipements

### Dépendances

- **System.Drawing.Printing** : Gestion de l'impression
- **Windows Forms** : Dialogues de sauvegarde et aperçu
- **SQLite** : Accès aux données

### Requêtes SQL utilisées

```sql
-- Informations agent
SELECT a.nom, a.prenom, a.idrh, a.email, s.name as site_name, e.name as equipe_name
FROM Agents a
LEFT JOIN Sites s ON a.site_id = s.id
LEFT JOIN Equipes e ON a.equipe_id = e.id
WHERE a.idrh = ?

-- Équipements en prêt
SELECT t.name as type_equipement, COALESCE(e.nom, e.code_parc, 'Sans nom') as nom_equipement,
       COALESCE(e.numero_serie, 'N/A') as numero_serie, COALESCE(e.marque, 'N/A') as marque
FROM Equipements e
JOIN equipment_type t ON t.id = e.type_id
WHERE e.idrh = ? AND e.etat_pret = 1
ORDER BY t.name, e.nom
```

## Améliorations possibles (chausse-trappes E6)

1. **Signature électronique** : Intégration d'une signature numérique
2. **Email automatique** : Envoi de la feuille par email à l'agent
3. **Modèles personnalisables** : Templates différents selon l'entreprise
4. **Export multiple formats** : Word, Excel en plus du PDF
5. **Historique des feuilles** : Archivage des feuilles générées
6. **Code QR** : Pour validation mobile
7. **Photos équipements** : Intégration des photos dans le document

## Notes de développement

### Limitations actuelles

- **PDF "simulé"** : La sauvegarde PDF est simulée (nécessiterait iTextSharp pour une vraie implémentation)
- **Mise en page fixe** : Le template n'est pas personnalisable
- **Une page uniquement** : Pas de gestion du débordement sur plusieurs pages

### Extensions recommandées

Pour une version complète :
1. Intégrer **iTextSharp** ou **QuestPDF** pour la génération PDF native
2. Ajouter des **templates personnalisables**
3. Gérer le **multi-pages** pour les agents avec beaucoup d'équipements
4. Ajouter des **statistiques** (feuilles générées par période)

---

*Cette fonctionnalité complète l'application GestiParc en ajoutant la capacité de documenter officiellement les prêts d'équipement, répondant ainsi aux exigences du cahier des charges initial.*