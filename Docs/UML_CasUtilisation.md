# Diagramme de cas d'utilisation - GestionParc

## Description
Ce diagramme montre les fonctionnalités principales du système et qui peut les utiliser.

## Acteurs
- **Administrateur** : Utilise toutes les fonctionnalités
- **Agent** : Utilise l'inventaire (en lecture seule dans cette version)

## Cas d'utilisation principaux

### Gestion des Agents
- Créer un agent
- Modifier un agent
- Supprimer un agent
- Rechercher un agent

### Gestion des Équipements
- Créer un équipement
- Modifier un équipement
- Supprimer un équipement
- Rechercher un équipement
- Gérer les équipements libres/rendus DSEM

### Gestion des Prêts et Échanges
- Créer un prêt
- Consulter les équipements en prêt
- Échanger des équipements entre agents

### Consultation
- Consulter l'inventaire complet
- Voir les détails d'un agent
- Voir les détails d'un équipement

## Code PlantUML (à copier dans Draw.io ou PlantUML)

```plantuml
@startuml
left to right direction
skinparam packageStyle rectangle

actor "Administrateur" as admin

rectangle "Gestion de Parc Informatique" {
  
  usecase "Créer un agent" as UC1
  usecase "Modifier un agent" as UC2
  usecase "Supprimer un agent" as UC3
  
  usecase "Créer un équipement" as UC4
  usecase "Modifier un équipement" as UC5
  usecase "Supprimer un équipement" as UC6
  
  usecase "Créer un prêt" as UC7
  usecase "Consulter inventaire" as UC8
  usecase "Échanger équipements" as UC9
  
  usecase "Rechercher" as UC10
  usecase "Gérer équipements libres" as UC11
}

' Relations avec l'administrateur
admin --> UC1
admin --> UC2
admin --> UC3
admin --> UC4
admin --> UC5
admin --> UC6
admin --> UC7
admin --> UC8
admin --> UC9
admin --> UC10
admin --> UC11

' Relations entre cas d'utilisation
UC7 ..> UC10 : <<include>>
UC9 ..> UC10 : <<include>>

' Positionnement pour clarté
UC1 -[hidden]- UC2
UC2 -[hidden]- UC3
UC4 -[hidden]- UC5
UC5 -[hidden]- UC6
UC7 -[hidden]- UC8
UC8 -[hidden]- UC9

@enduml
```

## Version simplifiée pour Draw.io

Si PlantUML ne fonctionne pas, voici comment le dessiner manuellement :

1. **Acteur** (bonhomme) : Administrateur
2. **Rectangle englobant** : "Gestion de Parc Informatique"
3. **Ellipses** (cas d'utilisation) :
   - Créer un agent
   - Modifier un agent
   - Supprimer un agent
   - Créer un équipement
   - Modifier un équipement
   - Supprimer un équipement
   - Créer un prêt
   - Échanger équipements
   - Consulter inventaire
   - Rechercher
4. **Flèches** : De l'Administrateur vers chaque ellipse
