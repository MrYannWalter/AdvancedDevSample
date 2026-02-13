# Architecture du Projet

## Vision d'ensemble

Le projet repose sur le principe de **Clean Architecture** : organiser le code en couches concentriques où les dépendances pointent toujours vers l'intérieur, c'est-à-dire vers le cœur métier.

### Pourquoi cette approche ?

Dans une application classique "tout-en-un", le code métier (les règles du commerce) est mélangé avec le code technique (base de données, HTTP). Cela crée plusieurs problèmes :

- **Difficulté à tester** : pour tester une règle métier, il faut monter une base de données
- **Couplage fort** : changer de base de données oblige à réécrire les règles métier
- **Lisibilité réduite** : on ne sait plus où se trouve la logique importante

La Clean Architecture résout ces problèmes en **isolant le métier** de la technique.

## Les 4 couches

```
┌─────────────────────────────────────────────────────┐
│                    API (Présentation)                │
│         Controllers, Middleware, Programme           │
│    → Reçoit les requêtes HTTP, renvoie les réponses │
├─────────────────────────────────────────────────────┤
│                Application (Orchestration)           │
│              Services, DTOs, Exceptions              │
│    → Coordonne les cas d'utilisation métier          │
├─────────────────────────────────────────────────────┤
│              Infrastructure (Technique)              │
│       InMemoryDataStore, Repositories, Configurations│
│    → Implémente l'accès aux données                  │
├─────────────────────────────────────────────────────┤
│                  Domain (Cœur métier)                │
│        Entités, Value Objects, Interfaces, Enums     │
│    → Définit les règles métier fondamentales         │
└─────────────────────────────────────────────────────┘
```

### Règle fondamentale : le sens des dépendances

```
API → Application → Domain ← Infrastructure
```

- Le **Domain** ne dépend de rien. Il définit des interfaces (contrats) que les autres couches implémentent.
- L'**Application** dépend uniquement du Domain. Elle utilise les interfaces sans connaître leur implémentation.
- L'**Infrastructure** dépend du Domain pour implémenter ses interfaces, mais ne connaît pas l'Application.
- L'**API** dépend de l'Application et de l'Infrastructure pour l'injection de dépendances.

### Pourquoi le Domain ne dépend de rien ?

C'est le point le plus important. Les règles métier (« un prix doit être positif », « on ne peut pas modifier une commande livrée ») sont des **vérités du commerce** qui ne changent pas selon la technologie utilisée. Elles doivent pouvoir vivre sans base de données, sans HTTP, sans framework.

## Flux d'une requête type

Exemple : **un utilisateur modifie le prix d'un produit**

```
1. Requête HTTP PUT /api/products/{id}/price
        │
        ▼
2. ProductsController reçoit la requête
   → Valide le format (ModelState)
   → Appelle ProductService.ChangeProductPrice()
        │
        ▼
3. ProductService (Application)
   → Récupère le produit via IProductRepository.GetById()
   → Appelle product.ChangePrice(newPrice) sur l'entité
   → Appelle IProductRepository.Save(product)
        │
        ▼
4. InMemoryProductRepository (Infrastructure)
   → Stocke/met à jour dans le Dictionary en mémoire
        │
        ▼
5. Product (Domain)
   → Vérifie la règle "le produit doit être actif"
   → Vérifie la règle "le prix doit être positif"
   → Met à jour le prix
        │
        ▼
6. Réponse HTTP 204 No Content
```

### Et en cas d'erreur ?

```
Erreur Domain (prix négatif)
   → DomainException("Le prix doit être positif")
   → ExceptionHandlingMiddleware intercepte
   → HTTP 400 Bad Request { "title": "Erreur Métier", "detail": "..." }

Erreur Application (produit inexistant)
   → ApplicationServiceException("Produit introuvable", 404)
   → ExceptionHandlingMiddleware intercepte
   → HTTP 404 Not Found { "title": "Ressource introuvable", "detail": "..." }

Erreur Infrastructure (base de données inaccessible)
   → InfrastructureException("Erreur de connexion")
   → ExceptionHandlingMiddleware intercepte
   → HTTP 500 Internal Server Error { "error": "Erreur Technique" }
```

## Injection de dépendances

Le câblage entre les couches se fait dans `Program.cs` via l'injection de dépendances .NET :

```
Services (Application)          →  Scoped (une instance par requête)
Repositories (Infrastructure)   →  Scoped (une instance par requête)
InMemoryDataStore (Infrastructure)      →  Singleton (instance unique partagée)
```

Le `InMemoryDataStore` est un **Singleton** pour garantir la persistance des données pendant toute la durée de vie de l'application. Les repositories sont `Scoped` pour s'aligner sur le cycle de vie des requêtes HTTP.

## Projet de tests

198 tests répartis en 3 niveaux, organisés en miroir de l'architecture :

- **Tests unitaires Domain** (5 fichiers) : règles métier pures, aucune dépendance externe
- **Tests de composants Application** (4 fichiers) : services testés avec les vrais repositories et `InMemoryDataStore` (pas de fakes)
- **Tests d'intégration API** (4 fichiers) : controllers testés via `WebApplicationFactory<Program>`

Convention de nommage : `Methode_DoitResultat_QuandCondition`.
