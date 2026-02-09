# Couche API - Points d'Entrée REST

## Rôle de cette couche

La couche API est la **porte d'entrée** de l'application. Elle expose les fonctionnalités métier sous forme d'endpoints HTTP REST que les clients (navigateur, application mobile, Postman...) peuvent appeler.

> **Analogie** : Si l'application est un restaurant, la couche API est le serveur qui prend les commandes des clients et leur apporte les plats. Il ne cuisine pas lui-même (c'est le rôle du Domain/Application).

## Structure

```
AdvancedDevSample.Api/
├── Controllers/
│   ├── ProductsController.cs     ← /api/products
│   ├── CustomersController.cs    ← /api/customers
│   ├── SuppliersController.cs    ← /api/suppliers
│   └── OrdersController.cs      ← /api/orders
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
├── Program.cs                    ← Configuration et démarrage
├── appsettings.json
└── appsettings.Development.json
```

## Endpoints disponibles

### Produits (`/api/products`)

| Méthode | Route | Description | Réponse |
|---------|-------|-------------|---------|
| `GET` | `/api/products` | Lister tous les produits | 200 + liste |
| `GET` | `/api/products/{id}` | Détail d'un produit | 200 + produit |
| `POST` | `/api/products` | Créer un produit | 201 + produit créé |
| `PUT` | `/api/products/{id}` | Modifier nom/description/prix | 204 |
| `PUT` | `/api/products/{id}/price` | Modifier le prix uniquement | 204 |
| `PUT` | `/api/products/{id}/activate` | Activer un produit | 204 |
| `PUT` | `/api/products/{id}/deactivate` | Désactiver un produit | 204 |
| `DELETE` | `/api/products/{id}` | Supprimer un produit | 204 |

### Clients (`/api/customers`)

| Méthode | Route | Description | Réponse |
|---------|-------|-------------|---------|
| `GET` | `/api/customers` | Lister tous les clients | 200 + liste |
| `GET` | `/api/customers/{id}` | Détail d'un client | 200 + client |
| `POST` | `/api/customers` | Créer un client | 201 + client créé |
| `PUT` | `/api/customers/{id}` | Modifier les informations | 204 |
| `PUT` | `/api/customers/{id}/activate` | Activer un client | 204 |
| `PUT` | `/api/customers/{id}/deactivate` | Désactiver un client | 204 |
| `DELETE` | `/api/customers/{id}` | Supprimer un client | 204 |

### Fournisseurs (`/api/suppliers`)

| Méthode | Route | Description | Réponse |
|---------|-------|-------------|---------|
| `GET` | `/api/suppliers` | Lister tous les fournisseurs | 200 + liste |
| `GET` | `/api/suppliers/{id}` | Détail d'un fournisseur | 200 + fournisseur |
| `POST` | `/api/suppliers` | Créer un fournisseur | 201 + fournisseur créé |
| `PUT` | `/api/suppliers/{id}` | Modifier les informations | 204 |
| `PUT` | `/api/suppliers/{id}/activate` | Activer un fournisseur | 204 |
| `PUT` | `/api/suppliers/{id}/deactivate` | Désactiver un fournisseur | 204 |
| `DELETE` | `/api/suppliers/{id}` | Supprimer un fournisseur | 204 |

### Commandes (`/api/orders`)

| Méthode | Route | Description | Réponse |
|---------|-------|-------------|---------|
| `GET` | `/api/orders` | Lister toutes les commandes | 200 + liste |
| `GET` | `/api/orders/{id}` | Détail d'une commande | 200 + commande |
| `GET` | `/api/orders/customer/{customerId}` | Commandes d'un client | 200 + liste |
| `POST` | `/api/orders` | Créer une commande | 201 + commande créée |
| `POST` | `/api/orders/{id}/items` | Ajouter un article | 204 |
| `DELETE` | `/api/orders/{id}/items/{itemId}` | Retirer un article | 204 |
| `PUT` | `/api/orders/{id}/confirm` | Confirmer la commande | 204 |
| `PUT` | `/api/orders/{id}/ship` | Marquer comme expédiée | 204 |
| `PUT` | `/api/orders/{id}/deliver` | Marquer comme livrée | 204 |
| `PUT` | `/api/orders/{id}/cancel` | Annuler la commande | 204 |
| `DELETE` | `/api/orders/{id}` | Supprimer une commande | 204 |

## Codes de réponse HTTP

| Code | Signification | Quand ? |
|------|--------------|---------|
| 200 OK | Succès avec données | GET (consultation) |
| 201 Created | Ressource créée | POST (création) |
| 204 No Content | Succès sans données | PUT, DELETE (modification, suppression) |
| 400 Bad Request | Erreur de validation/métier | Règle métier violée (DomainException) |
| 404 Not Found | Ressource inexistante | ID invalide (ApplicationServiceException) |
| 500 Internal Server Error | Erreur technique | Problème base de données (InfrastructureException) |

## Le Middleware d'exceptions

Le `ExceptionHandlingMiddleware` est un **filet de sécurité global**. Il intercepte toutes les exceptions non gérées et les transforme en réponses HTTP standardisées.

### Pourquoi un middleware plutôt que des try/catch dans chaque controller ?

1. **Évite la duplication** : sans middleware, chaque action de chaque controller devrait avoir son try/catch
2. **Cohérence** : toutes les erreurs sont formatées de la même manière
3. **Séparation** : les controllers se concentrent sur le routage, pas sur la gestion d'erreurs

### Hiérarchie de gestion

```
DomainException         → 400 + message métier lisible
ApplicationServiceException → Code HTTP personnalisé + message
InfrastructureException → 500 + message générique (pas de détails techniques)
Exception (autre)       → 500 + message générique
```

## Swagger / OpenAPI

L'API est documentée automatiquement via Swagger. En mode développement :

- **Interface Swagger UI** : `https://localhost:7010/swagger`
- **Spécification OpenAPI** : `https://localhost:7010/swagger/v1/swagger.json`

Les commentaires XML (`///`) des controllers sont intégrés dans la documentation Swagger grâce à la configuration dans `Program.cs`.

## Program.cs - Le point de départ

`Program.cs` a trois responsabilités :

1. **Enregistrer les dépendances** : connecter les interfaces aux implémentations
2. **Configurer le pipeline** : définir l'ordre des middlewares
3. **Démarrer l'application** : lancer le serveur HTTP

### Pipeline des middlewares (ordre d'exécution)

```
Requête HTTP entrante
    │
    ▼ UseHttpsRedirection     → Force HTTPS
    ▼ UseAuthorization        → Vérifie les autorisations (futur)
    ▼ ExceptionHandlingMiddleware → Intercepte les erreurs
    ▼ MapControllers          → Route vers le bon controller
    │
    ▼
Réponse HTTP sortante
```
