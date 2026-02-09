# Couche Infrastructure - Persistance et Accès aux Données

## Rôle de cette couche

La couche Infrastructure est la **couche technique** qui implémente concrètement ce dont le métier a besoin. Elle répond à la question : « Comment stocker et retrouver les données ? »

> **Analogie** : Si le Domain dit « j'ai besoin de retrouver un produit par son identifiant », l'Infrastructure répond « voici comment je le fais avec Entity Framework et une base de données ».

## Structure

```
AdvancedDevSample.Infrastructure/
├── Persistence/
│   └── AppDbContext.cs          ← Contexte Entity Framework (schéma de la base)
├── Repositories/
│   ├── EfProductRepository.cs   ← Implémentation pour les produits
│   ├── EfCustomerRepository.cs  ← Implémentation pour les clients
│   ├── EfSupplierRepository.cs  ← Implémentation pour les fournisseurs
│   └── EfOrderRepository.cs    ← Implémentation pour les commandes
└── Exceptions/
    └── InfrastructureException.cs ← Erreurs techniques
```

## Le DbContext (AppDbContext)

Le `AppDbContext` est le **point central** d'Entity Framework Core. Il définit :

- **Quelles tables existent** : Products, Customers, Suppliers, Orders, OrderItems
- **Comment les entités sont mappées** : types de colonnes, tailles, contraintes
- **Les relations entre tables** : une commande contient plusieurs articles (1-N)

### Configuration des entités

Chaque entité est configurée via la Fluent API dans `OnModelCreating` :

| Entité | Configurations principales |
|--------|---------------------------|
| Product | Nom max 200 car., Description max 1000 car., Prix précision 18.2 |
| Customer | Prénom/Nom max 100 car., Email max 200 car. |
| Supplier | Société max 200 car., Email max 200 car., Téléphone max 50 car. |
| Order | Relation 1-N avec OrderItems, suppression en cascade |
| OrderItem | Quantité requise, Prix unitaire précision 18.2 |

### Cas particulier : la collection Items de Order

La collection `Items` dans `Order` est privée (`private readonly List<OrderItem> _items`), mais EF Core doit pouvoir y accéder. La configuration utilise `PropertyAccessMode.Field` pour dire à EF d'utiliser le champ privé `_items` directement au lieu de passer par la propriété publique `Items`.

Cela préserve l'**encapsulation** : seule l'entité `Order` peut ajouter/retirer des items via ses méthodes métier.

## Les Repositories

Chaque repository implémente une interface du Domain et utilise `AppDbContext` pour les opérations CRUD.

### Pattern commun à tous les repositories

```
public class EfXxxRepository : IXxxRepository
{
    private readonly AppDbContext _context;

    Add()     → _context.Xxx.Add(entity) + SaveChanges()
    GetById() → _context.Xxx.FirstOrDefault(x => x.Id == id)
    ListAll() → _context.Xxx.ToList()
    Save()    → _context.Xxx.Update(entity) + SaveChanges()
    Remove()  → Charger puis _context.Xxx.Remove(entity) + SaveChanges()
}
```

### Particularité du OrderRepository

Le repository des commandes utilise `.Include(o => o.Items)` pour charger systématiquement les articles avec la commande. C'est du **eager loading** : on récupère toujours la commande complète avec ses lignes.

Sans cela, la collection `Items` serait vide car EF Core utilise le **lazy loading** par défaut qui ne charge pas les relations automatiquement.

## Gestion des erreurs techniques

Toutes les opérations de base de données sont encapsulées dans des blocs `try/catch`. Les exceptions techniques (connexion perdue, contrainte violée...) sont transformées en `InfrastructureException` avec un message explicatif.

Le middleware intercepte ensuite ces exceptions et renvoie une erreur HTTP 500 générique, **sans exposer les détails techniques** au client. Les détails sont consignés dans les logs pour le débogage.

## Base de données utilisée

En développement, le projet utilise **InMemoryDatabase** d'Entity Framework :

- **Avantage** : aucune installation requise, données en mémoire vive
- **Inconvénient** : les données sont perdues à chaque redémarrage
- **Usage** : développement, prototypage, tests

Pour la production, il suffit de remplacer `UseInMemoryDatabase` par `UseSqlite` ou `UseSqlServer` dans `Program.cs` sans changer aucun autre code (grâce à l'abstraction des repositories).
