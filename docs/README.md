# AdvancedDevSample - Documentation du Projet

## Qu'est-ce que ce projet ?

AdvancedDevSample est une **application web de gestion commerciale** permettant de :

- **Gérer un catalogue de produits** : créer, consulter, modifier, activer/désactiver et supprimer des produits vendables
- **Gérer les clients** : enregistrer et maintenir à jour les informations des clients qui passent des commandes
- **Gérer les fournisseurs** : référencer les sociétés qui fournissent les produits du catalogue
- **Gérer les commandes** : créer des commandes pour les clients, y ajouter des produits, et suivre leur cycle de vie (en attente → confirmée → expédiée → livrée ou annulée)

## À qui s'adresse cette application ?

Cette application est conçue pour un **petit commerce ou une PME** ayant besoin d'un outil simple pour :
- Maintenir un catalogue produit à jour avec des prix
- Suivre les clients et leurs commandes
- Connaître les fournisseurs référencés
- Suivre l'état d'avancement des commandes

## Comment le projet est-il organisé ?

Le projet suit une **architecture en couches** (Clean Architecture) qui sépare clairement les responsabilités :

```
AdvancedDevSample/
│
├── docs/                          ← 📖 Vous êtes ici
│   ├── architecture/              ← Vues d'ensemble et choix techniques
│   ├── domain/                    ← Règles métier (produits, clients, commandes...)
│   ├── application/               ← Cas d'utilisation et flux de données
│   ├── infrastructure/            ← Persistance et accès aux données
│   ├── api/                       ← Points d'entrée HTTP (REST API)
│   ├── adr/                       ← Décisions architecturales (ADR)
│   ├── runbooks/                  ← Guides de résolution d'erreurs
│   └── diagrams/                  ← Diagrammes visuels (Mermaid)
│
├── AdvancedDevSample.Domain/      ← Cœur métier (entités, règles, interfaces)
├── AdvancedDevSample.Application/ ← Orchestration (services, DTOs, cas d'usage)
├── AdvancedDevSample.Infrastructure/ ← Technique (base de données, repositories)
├── AdvancedDevSample.Api/         ← Exposition (controllers REST, middleware)
└── AdvancedDevSample.Test/        ← Tests (unitaires, intégration)
```

## Par où commencer ?

| Vous souhaitez...                              | Lisez...                                      |
|------------------------------------------------|-----------------------------------------------|
| Comprendre l'architecture globale              | [architecture/overview.md](architecture/overview.md) |
| Comprendre les règles métier                   | [domain/README.md](domain/README.md)          |
| Savoir comment fonctionnent les cas d'usage    | [application/README.md](application/README.md)|
| Comprendre la persistance des données          | [infrastructure/README.md](infrastructure/README.md) |
| Connaître les endpoints API disponibles        | [api/README.md](api/README.md)                |
| Comprendre pourquoi un choix technique a été fait | [adr/](adr/)                               |
| Résoudre une erreur en production              | [runbooks/](runbooks/)                        |
| Voir les diagrammes du projet                  | [diagrams/](diagrams/)                        |

## Stack technique

| Composant          | Technologie                     |
|--------------------|---------------------------------|
| Framework          | .NET 8.0 / ASP.NET Core Web API|
| Langage            | C# 12                          |
| Persistance        | Dictionary<Guid, T> en mémoire (InMemoryDataStore) |
| Documentation API  | Swagger / OpenAPI (Swashbuckle) |
| Tests              | xUnit — 198 tests (unitaires, composants, intégration) |
| Architecture       | Clean Architecture (4 couches)  |
