# Couche Domain - Le Cœur Métier

## Rôle de cette couche

La couche Domain contient **toute la logique métier** de l'application. C'est ici que se trouvent les règles fondamentales du commerce qui ne changent jamais, quelle que soit la technologie utilisée.

> **Principe clé** : Si demain on remplace la base de données par des fichiers CSV, ou si on remplace l'API REST par une application desktop, les règles métier du Domain restent exactement les mêmes.

## Structure

```
AdvancedDevSample.Domain/
├── Entities/          ← Les objets métier principaux
│   ├── Product.cs     ← Un produit vendable
│   ├── Customer.cs    ← Un client
│   ├── Supplier.cs    ← Un fournisseur
│   ├── Order.cs       ← Une commande
│   └── OrderItem.cs   ← Une ligne de commande
├── Enums/
│   └── OrderStatus.cs ← Les états d'une commande
├── ValueObjects/
│   └── Price.cs       ← Objet-valeur pour les prix
├── Interfaces/
│   ├── IProductRepository.cs
│   ├── ICustomerRepository.cs
│   ├── ISupplierRepository.cs
│   └── IOrderRepository.cs
└── Exceptions/
    └── DomainException.cs
```

## Les entités métier

### Product (Produit)

Un produit représente un **article vendable** dans le catalogue.

**Données** :
- **Nom** : obligatoire, identifie le produit dans le catalogue
- **Description** : texte libre décrivant le produit
- **Prix** : obligatoirement positif, modifiable sous conditions
- **Actif/Inactif** : un produit inactif ne peut plus être modifié ou commandé

**Règles métier** :
| Règle | Explication |
|-------|------------|
| Le prix doit être strictement positif | On ne vend pas un produit à 0 € ou à prix négatif |
| Un produit inactif ne peut pas changer de prix | Avant de modifier un prix, il faut réactiver le produit |
| Le nom est obligatoire | Un produit sans nom n'a pas de sens dans un catalogue |
| Une réduction doit être entre 0% et 100% | On ne peut pas appliquer une réduction de 150% |
| Le prix après réduction doit rester positif | Une réduction ne doit pas donner un prix nul |

### Customer (Client)

Un client représente une **personne physique** pouvant passer des commandes.

**Données** :
- **Prénom** : obligatoire
- **Nom** : obligatoire
- **Email** : obligatoire, sert d'identifiant de contact
- **Actif/Inactif** : un client inactif est archivé

**Règles métier** :
| Règle | Explication |
|-------|------------|
| Prénom, nom et email obligatoires | Ce sont les informations minimales pour identifier un client |
| Un client peut être désactivé | Au lieu de supprimer un client ayant un historique, on le désactive |

### Supplier (Fournisseur)

Un fournisseur représente une **société** qui approvisionne le catalogue en produits.

**Données** :
- **Nom de société** : obligatoire
- **Email de contact** : obligatoire
- **Téléphone** : optionnel
- **Actif/Inactif** : un fournisseur inactif n'est plus sollicité

**Règles métier** :
| Règle | Explication |
|-------|------------|
| Le nom de société et l'email sont obligatoires | Informations minimales pour référencer un fournisseur |

### Order (Commande)

Une commande représente un **achat passé par un client**. C'est l'entité la plus riche en logique métier.

**Données** :
- **Client** : la commande est toujours liée à un client identifié
- **Date de commande** : automatiquement définie à la création
- **Statut** : le cycle de vie de la commande
- **Articles** : la liste des produits commandés avec quantités et prix

**Cycle de vie d'une commande** :
```
    ┌──────────┐
    │ Pending  │ ← État initial (en attente)
    └────┬─────┘
         │ Confirm()
         ▼
    ┌──────────┐
    │Confirmed │ ← Commande validée
    └────┬─────┘
         │ Ship()
         ▼
    ┌──────────┐
    │ Shipped  │ ← En cours de livraison
    └────┬─────┘
         │ Deliver()
         ▼
    ┌──────────┐
    │Delivered │ ← Livrée au client
    └──────────┘

    ⚡ Cancel() est possible à tout moment SAUF si Delivered
```

**Règles métier** :
| Règle | Explication |
|-------|------------|
| Une commande doit avoir un client | Pas de commande anonyme |
| On ne peut ajouter des articles qu'à une commande en attente | Une fois confirmée, la commande est figée |
| On ne peut pas confirmer une commande vide | Il faut au moins un article |
| Les transitions de statut suivent un ordre strict | Pending → Confirmed → Shipped → Delivered |
| Une commande livrée ne peut pas être annulée | Le client a déjà reçu sa commande |
| Un même produit ne peut pas être ajouté deux fois | Il faut modifier la quantité de la ligne existante |

### OrderItem (Ligne de commande)

Un article de commande représente **une ligne dans une commande** : quel produit, en quelle quantité, à quel prix.

**Règles métier** :
| Règle | Explication |
|-------|------------|
| La quantité doit être positive | On commande au minimum 1 unité |
| Le prix unitaire doit être positif | Le prix est capturé au moment de l'ajout |

## Les interfaces (contrats)

Les interfaces du Domain définissent **ce dont le métier a besoin** sans se soucier du comment :

- `IProductRepository` : savoir sauvegarder, retrouver et lister des produits
- `ICustomerRepository` : idem pour les clients
- `ISupplierRepository` : idem pour les fournisseurs
- `IOrderRepository` : idem pour les commandes, plus la recherche par client

> **Important** : ces interfaces sont définies dans le Domain mais **implémentées dans l'Infrastructure**. C'est le principe d'inversion de dépendances.

## Les exceptions métier

`DomainException` est levée chaque fois qu'une **règle métier est violée**. Elle est toujours interceptée par le middleware et traduite en erreur HTTP 400 (Bad Request) avec un message explicatif pour l'utilisateur.

Exemples :
- `"Le prix doit être strictement positif."` → le prix saisi est négatif
- `"Le produit est inactif."` → tentative de modification sur un produit désactivé
- `"Impossible d'annuler une commande déjà livrée."` → règle du cycle de vie
