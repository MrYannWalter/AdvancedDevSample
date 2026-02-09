# ADR-001 : Adoption de la Clean Architecture

## Statut
**Accepté** — Décision fondatrice du projet.

## Contexte

Nous devons concevoir une application web de gestion commerciale (catalogue, clients, fournisseurs, commandes). L'application doit être :
- Facile à comprendre pour de nouveaux développeurs
- Testable unitairement sans infrastructure lourde
- Évolutive (ajout de nouvelles fonctionnalités sans casser l'existant)
- Indépendante d'un framework ou d'une base de données spécifique

## Décision

Nous adoptons une **Clean Architecture en 4 couches** :

1. **Domain** : Entités métier, règles de gestion, interfaces
2. **Application** : Services (cas d'utilisation), DTOs, orchestration
3. **Infrastructure** : Entity Framework, repositories concrets
4. **API** : Controllers REST, middleware, configuration

### Règle de dépendances
Les dépendances pointent toujours **vers l'intérieur** :
- API → Application → Domain
- Infrastructure → Domain (implémente les interfaces)

Le Domain ne dépend de rien.

## Alternatives considérées

### Architecture en 3 couches classique (Controller → Service → Repository)
- **Avantage** : Plus simple, moins de projets
- **Inconvénient** : Le métier est mélangé avec la technique. Impossible de tester les règles métier sans base de données. Le service fait tout (orchestration + métier + persistance).
- **Rejeté** car on perdrait la testabilité et la séparation des responsabilités.

### Architecture hexagonale (Ports & Adapters)
- **Avantage** : Encore plus découplée, supporte plusieurs points d'entrée
- **Inconvénient** : Plus complexe à mettre en place, surdimensionné pour un projet de cette taille
- **Rejeté** car la Clean Architecture offre un bon équilibre pour ce projet.

## Conséquences

### Positives
- Le Domain est testable sans aucune dépendance technique
- On peut changer de base de données sans toucher au métier
- La structure en projets séparés force la discipline
- Chaque couche a un rôle clair et défini

### Négatives
- Plus de fichiers et de projets qu'une application simple
- Nécessite de comprendre l'injection de dépendances
- Courbe d'apprentissage pour les développeurs juniors
