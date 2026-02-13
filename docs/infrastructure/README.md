# Couche Infrastructure - Persistance et Accès aux Données

## Rôle de cette couche

La couche Infrastructure est la **couche technique** qui implémente concrètement ce dont le métier a besoin. Elle répond à la question : « Comment stocker et retrouver les données ? »

> **Analogie** : Si le Domain dit « j'ai besoin de retrouver un produit par son identifiant », l'Infrastructure répond « voici comment je le fais avec un dictionnaire en mémoire ».

## Structure

```
AdvancedDevSample.Infrastructure/
├── Persistence/
│   └── InMemoryDataStore.cs        ← Stockage centralisé (dictionnaires)
├── Repositories/
│   ├── InMemoryProductRepository.cs   ← Implémentation pour les produits
│   ├── InMemoryCustomerRepository.cs  ← Implémentation pour les clients
│   ├── InMemorySupplierRepository.cs  ← Implémentation pour les fournisseurs
│   └── InMemoryOrderRepository.cs     ← Implémentation pour les commandes
└── Exceptions/
    └── InfrastructureException.cs ← Erreurs techniques
```

## Le Data Store (InMemoryDataStore)

Le `InMemoryDataStore` est le **point central** du stockage de données. C'est une classe simple qui expose 4 dictionnaires `Dictionary<Guid, TEntity>`, un par entité :

- **Products** : stocke les produits indexés par leur `Id`
- **Customers** : stocke les clients indexés par leur `Id`
- **Suppliers** : stocke les fournisseurs indexés par leur `Id`
- **Orders** : stocke les commandes (avec leurs articles) indexées par leur `Id`

### Pourquoi des dictionnaires ?

| Avantage | Explication |
|----------|-------------|
| Accès O(1) | Recherche par `Id` instantanée grâce au hachage |
| Simplicité | Pas de dépendance externe, pas de configuration |
| Lisibilité | Le code des repositories est immédiatement compréhensible |
| Légèreté | Zéro package NuGet supplémentaire |

### Enregistrement en tant que Singleton

Le `InMemoryDataStore` est enregistré comme **Singleton** dans le conteneur d'injection :

```csharp
builder.Services.AddSingleton<InMemoryDataStore>();
```

Cela garantit qu'une **seule instance** existe pendant toute la durée de vie de l'application. Tous les repositories partagent le même store, donc les données persistent entre les requêtes HTTP tant que l'application tourne.

## Les Repositories

Chaque repository implémente une interface du Domain et utilise `InMemoryDataStore` pour les opérations CRUD.

### Pattern commun à tous les repositories

```
public class InMemoryXxxRepository : IXxxRepository
{
    private readonly InMemoryDataStore _store;

    Add()     → _store.Xxxs[entity.Id] = entity
    GetById() → _store.Xxxs.TryGetValue(id, out var entity) ? entity : null
    ListAll() → _store.Xxxs.Values.ToList()
    Save()    → _store.Xxxs[entity.Id] = entity  (écrase l'entrée existante)
    Remove()  → _store.Xxxs.Remove(id)
}
```

### Particularité du OrderRepository

Le repository des commandes expose une méthode supplémentaire `GetByCustomerId(Guid customerId)` qui filtre les commandes par client :

```
_store.Orders.Values.Where(o => o.CustomerId == customerId).ToList()
```

Comme les articles (`OrderItem`) sont stockés directement dans l'objet `Order` (liste privée `_items`), il n'y a pas besoin d'eager loading comme c'était le cas avec Entity Framework. La commande est toujours complète avec ses articles.

## Gestion des erreurs techniques

Toutes les opérations de stockage sont encapsulées dans des blocs `try/catch`. Les exceptions techniques sont transformées en `InfrastructureException` avec un message explicatif.

Le middleware intercepte ensuite ces exceptions et renvoie une erreur HTTP 500 générique, **sans exposer les détails techniques** au client. Les détails sont consignés dans les logs pour le débogage.

## Stockage en mémoire

Le projet utilise un **stockage par dictionnaires en mémoire** :

- **Avantage** : aucune installation requise, aucune dépendance externe, accès rapide O(1)
- **Inconvénient** : les données sont perdues à chaque redémarrage de l'application
- **Usage** : développement, prototypage, tests, formation

### Évolution possible

Si le projet nécessite une persistance durable à l'avenir, il suffit de :
1. Ajouter les packages EF Core / Dapper / autre ORM
2. Créer de nouvelles implémentations de repositories
3. Changer l'enregistrement DI dans `Program.cs`

Grâce à l'abstraction des interfaces (`IProductRepository`, etc.), **aucun autre code ne change** : ni les services, ni les controllers, ni les tests unitaires.
