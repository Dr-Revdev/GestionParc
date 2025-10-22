# Diagramme de séquence - GestionParc

## Description
Ces diagrammes montrent les interactions entre les objets lors de l'exécution d'une fonctionnalité.

---

## Scénario 1 : Création d'un prêt d'équipement

### Description
L'administrateur crée un nouveau prêt en associant un agent à un équipement.

### Acteurs
- Administrateur
- LoanCreationView
- EquipmentSelectionControl
- Database

### Code PlantUML

```plantuml
@startuml
actor Administrateur as admin
participant "LoanCreationView" as view
participant "EquipmentSelectionControl" as ctrl
database "Database" as db

admin -> view : Ouvrir vue création prêt
activate view

view -> db : LoadAgents()
activate db
db --> view : Liste des agents
deactivate db

admin -> view : Sélectionner agent
view -> ctrl : LoadEquipmentForAgent(agentId)
activate ctrl

ctrl -> db : SELECT équipements disponibles
activate db
db --> ctrl : Liste équipements libres
deactivate db

ctrl --> view : Afficher équipements
deactivate ctrl

admin -> view : Cocher équipements à prêter
admin -> view : Clic "Valider"

view -> view : ValidateSelection()
alt Équipements sélectionnés
    view -> db : BEGIN TRANSACTION
    activate db
    
    loop Pour chaque équipement
        view -> db : UPDATE Equipements\nSET etat_pret = 1\nWHERE id = ?
        view -> db : INSERT INTO Travail\n(id_agent, id_equipement, date_debut)
    end
    
    view -> db : COMMIT
    db --> view : Succès
    deactivate db
    
    view -> admin : MessageBox("Prêt créé avec succès")
    view -> view : Fermer fenêtre
else Aucun équipement sélectionné
    view -> admin : MessageBox("Sélectionner au moins un équipement")
end

deactivate view
@enduml
```

---

## Scénario 2 : Échange d'équipements entre agents

### Description
L'administrateur échange des équipements entre deux agents.

### Code PlantUML

```plantuml
@startuml
actor Administrateur as admin
participant "EquipmentExchangeView" as view
database "Database" as db

admin -> view : Ouvrir vue échange
activate view

view -> db : LoadAllAgents()
activate db
db --> view : Liste agents
deactivate db

admin -> view : Sélectionner Agent 1
view -> db : LoadEquipmentForAgent(agent1)
activate db
db --> view : Équipements Agent 1
deactivate db

admin -> view : Sélectionner Agent 2
view -> db : LoadEquipmentForAgent(agent2)
activate db
db --> view : Équipements Agent 2
deactivate db

admin -> view : Cocher équipements à échanger
admin -> view : Clic "Échanger"

view -> view : ValidateExchange()
alt Validation OK
    view -> admin : Confirmation("Échanger X équipements ?")
    admin -> view : Oui
    
    view -> db : BEGIN TRANSACTION
    activate db
    
    loop Équipements Agent1 → Agent2
        view -> db : UPDATE Travail\nSET date_fin = NOW()\nWHERE id_agent = agent1
        view -> db : INSERT INTO Travail\n(id_agent=agent2, id_equipement, date_debut)
    end
    
    loop Équipements Agent2 → Agent1
        view -> db : UPDATE Travail\nSET date_fin = NOW()\nWHERE id_agent = agent2
        view -> db : INSERT INTO Travail\n(id_agent=agent1, id_equipement, date_debut)
    end
    
    view -> db : COMMIT
    db --> view : Succès
    deactivate db
    
    view -> admin : MessageBox("Échange effectué")
    view -> view : Rafraîchir les listes
else Validation échouée
    view -> admin : MessageBox("Erreur : agents identiques ou aucun équipement sélectionné")
end

deactivate view
@enduml
```

---

## Scénario 3 : Recherche d'un équipement

### Description
L'administrateur recherche un équipement par mot-clé.

### Code PlantUML

```plantuml
@startuml
actor Administrateur as admin
participant "EquipmentEditView" as view
database "Database" as db

admin -> view : Ouvrir modification équipement
activate view

view -> db : SELECT * FROM Equipements\nORDER BY nom
activate db
db --> view : Liste complète équipements
deactivate db

view -> view : Afficher liste

admin -> view : Saisir texte recherche
admin -> view : Clic bouton 🔍

view -> view : GetSearchQuery()
view -> db : SELECT * FROM Equipements\nWHERE nom LIKE '%query%'\nOR code_parc LIKE '%query%'\nOR numero_serie LIKE '%query%'
activate db
db --> view : Liste filtrée
deactivate db

view -> view : Afficher résultats filtrés

admin -> view : Sélectionner équipement
view -> db : SELECT * FROM Equipements\nWHERE id = ?
activate db
db --> view : Détails équipement
deactivate db

view -> view : Remplir formulaire édition

deactivate view
@enduml
```

---

## Version simplifiée pour dessin manuel

### Scénario : Création d'un prêt

```
Administrateur    LoanCreationView    Database
     |                  |                 |
     |---Ouvrir-------->|                 |
     |                  |--LoadAgents()-->|
     |                  |<--Liste---------|
     |                  |                 |
     |--Sélect. agent-->|                 |
     |                  |--GetEquipments->|
     |                  |<--Liste---------|
     |                  |                 |
     |--Cocher équip.-->|                 |
     |--Clic Valider--->|                 |
     |                  |--BEGIN TRANS--->|
     |                  |--UPDATE-------->|
     |                  |--INSERT-------->|
     |                  |--COMMIT-------->|
     |                  |<--Succès--------|
     |<--Message OK-----|                 |
     |                  |                 |
```

### Éléments à dessiner :

1. **Acteurs/Objets** (rectangles en haut) :
   - Administrateur (bonhomme)
   - LoanCreationView
   - Database

2. **Lignes de vie** (lignes verticales pointillées)

3. **Messages** (flèches horizontales avec texte) :
   - Flèche pleine = appel de méthode
   - Flèche pointillée = retour

4. **Barres d'activation** (rectangles fins sur les lignes de vie)
   - Montrent quand un objet est actif

---

## Conseils pour la présentation

### À expliquer au jury :

1. **Diagramme de séquence du prêt** :
   - "Voici comment se déroule la création d'un prêt étape par étape"
   - "On charge d'abord les agents disponibles"
   - "Puis les équipements libres de l'agent sélectionné"
   - "Enfin, on utilise une transaction pour garantir la cohérence"

2. **Points techniques à mettre en avant** :
   - Utilisation de **transactions SQL** (BEGIN/COMMIT)
   - Validation des données avant enregistrement
   - Gestion des erreurs (try/catch)
   - Rafraîchissement de l'interface après modification

3. **Questions probables du jury** :
   - "Que se passe-t-il si la base échoue ?"
     → "Le COMMIT échoue, les modifications sont annulées (ROLLBACK automatique)"
   
   - "Pourquoi une transaction ?"
     → "Pour garantir que tous les équipements soient prêtés ensemble ou aucun"
   
   - "Comment gérez-vous 2 utilisateurs simultanés ?"
     → "SQLite verrouille la base pendant l'écriture, le 2e attend"

---

## Fichiers à créer pour ta présentation

1. **Cas d'utilisation** : Montre QUOI (les fonctionnalités)
2. **Diagramme de classes** : Montre LES OBJETS (structure)
3. **Diagramme de séquence** : Montre COMMENT (le déroulement)

Ces 3 diagrammes couvrent les attentes du jury ! 🎯
