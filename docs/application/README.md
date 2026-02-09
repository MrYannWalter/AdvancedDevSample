# Couche Application - Orchestration des Cas d'Utilisation

## Rôle de cette couche

La couche Application est le **chef d'orchestre** de l'application. Elle ne contient pas de règles métier (c'est le rôle du Domain) ni de code technique (c'est le rôle de l'Infrastructure). Son travail est de :

1. **Recevoir une demande** (via un DTO)
2. **Coordonner les opérations** nécessaires (charger des entités, appeler des méthodes métier)
3. **Persister les résultats** (via les interfaces repositories)

> **Analogie** : Si le Domain est un ensemble d'employés experts dans leur métier, la couche Application est le manager qui leur dit dans quel ordre travailler et s'assure que tout se passe bien.

## Structure

```
AdvancedDevSample.Application/
├── Services/              ← Les cas d'utilisation regroupés par entité
│   ├── ProductService.cs
│   ├── CustomerService.cs
│   ├── SupplierService.cs
│   └── OrderService.cs
├── DTOs/                  ← Les objets de transfert de données
│   ├── CreateProductRequest.cs
│   ├── UpdateProductRequest.cs
│   ├── ChangePriceRequest.cs
│   ├── ProductResponse.cs
│   ├── CreateCustomerRequest.cs
│   ├── UpdateCustomerRequest.cs
│   ├── CustomerResponse.cs
│   ├── CreateSupplierRequest.cs
│   ├── UpdateSupplierRequest.cs
│   ├── SupplierResponse.cs
│   ├── CreateOrderRequest.cs
│   ├── AddOrderItemRequest.cs
│   ├── OrderResponse.cs
│   └── OrderItemResponse.cs
└── Exceptions/
    └── ApplicationServiceException.cs
```

## Les Services

Chaque service regroupe tous les **cas d'utilisation** liés à une entité.

### ProductService - Gestion du catalogue

| Cas d'utilisation | Description métier |
|-------------------|-------------------|
| `CreateProduct` | Ajouter un nouveau produit au catalogue |
| `GetProduct` | Consulter les détails d'un produit |
| `GetAllProducts` | Afficher tout le catalogue |
| `ChangeProductPrice` | Modifier le prix d'un produit actif |
| `UpdateProduct` | Mettre à jour le nom et la description |
| `DeleteProduct` | Retirer un produit du catalogue |
| `ActivateProduct` | Remettre un produit en vente |
| `DeactivateProduct` | Retirer temporairement un produit de la vente |

### CustomerService - Gestion des clients

| Cas d'utilisation | Description métier |
|-------------------|-------------------|
| `CreateCustomer` | Enregistrer un nouveau client |
| `GetCustomer` | Consulter la fiche d'un client |
| `GetAllCustomers` | Lister tous les clients |
| `UpdateCustomer` | Modifier les coordonnées d'un client |
| `DeleteCustomer` | Supprimer un client |
| `ActivateCustomer` | Réactiver un client archivé |
| `DeactivateCustomer` | Archiver un client |

### SupplierService - Gestion des fournisseurs

| Cas d'utilisation | Description métier |
|-------------------|-------------------|
| `CreateSupplier` | Référencer un nouveau fournisseur |
| `GetSupplier` | Consulter la fiche d'un fournisseur |
| `GetAllSuppliers` | Lister tous les fournisseurs |
| `UpdateSupplier` | Modifier les coordonnées d'un fournisseur |
| `DeleteSupplier` | Supprimer un fournisseur |
| `ActivateSupplier` | Réactiver un fournisseur |
| `DeactivateSupplier` | Désactiver un fournisseur |

### OrderService - Gestion des commandes

| Cas d'utilisation | Description métier |
|-------------------|-------------------|
| `CreateOrder` | Ouvrir une nouvelle commande pour un client |
| `GetOrder` | Consulter le détail d'une commande |
| `GetAllOrders` | Lister toutes les commandes |
| `GetOrdersByCustomer` | Voir l'historique d'un client |
| `AddItemToOrder` | Ajouter un produit à une commande |
| `RemoveItemFromOrder` | Retirer un produit d'une commande |
| `ConfirmOrder` | Valider la commande |
| `ShipOrder` | Marquer comme expédiée |
| `DeliverOrder` | Marquer comme livrée |
| `CancelOrder` | Annuler la commande |
| `DeleteOrder` | Supprimer une commande |

**Le OrderService est le plus complexe** car il interagit avec trois repositories :
- `IOrderRepository` pour les commandes
- `ICustomerRepository` pour vérifier que le client existe à la création
- `IProductRepository` pour récupérer le prix du produit lors de l'ajout d'un article

## Les DTOs (Data Transfer Objects)

### Pourquoi des DTOs ?

Les entités du Domain contiennent de la **logique métier** avec des setters privés. On ne peut pas (et on ne doit pas) les utiliser directement pour recevoir les données HTTP. Les DTOs servent de **traducteurs** :

- **Request DTOs** (entrants) : reçoivent les données de l'utilisateur avec des validations de format
- **Response DTOs** (sortants) : formatent les données de l'entité pour l'utilisateur

### Validation à deux niveaux

```
Niveau 1 : DTO (format)                     Niveau 2 : Domain (métier)
─────────────────────────                    ──────────────────────────
"Le prix est un nombre"                      "Le prix doit être > 0"
"L'email a un format valide"                 "L'email est obligatoire"
"La quantité est un entier > 0"              "Le produit est déjà dans la commande"
```

Les validations DTO sont vérifiées automatiquement par ASP.NET Core (attributs `[Required]`, `[Range]`, `[EmailAddress]`). Les validations métier sont vérifiées par les entités du Domain.

## Les exceptions applicatives

`ApplicationServiceException` est levée quand un **cas d'utilisation échoue** pour une raison non-métier, typiquement une ressource introuvable :

- `"Produit introuvable."` → HTTP 404
- `"Client introuvable."` → HTTP 404
- `"Commande introuvable."` → HTTP 404

Elle embarque un `HttpStatusCode` pour que le middleware sache quel code HTTP renvoyer.

## Pattern : Comment un service fonctionne

Chaque méthode de service suit toujours le même schéma :

```
1. Charger    → Récupérer l'entité via le repository (lever une exception si elle n'existe pas)
2. Exécuter   → Appeler la méthode métier sur l'entité (le Domain valide les règles)
3. Persister  → Sauvegarder le résultat via le repository
```

Ce pattern en 3 étapes est systématique et prévisible, ce qui rend le code facile à comprendre et à maintenir.
