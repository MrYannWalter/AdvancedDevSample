# ADR-002 : Stockage par Dictionnaires en Mémoire

## Statut
**Accepté (révisé)** — Remplace l'utilisation d'Entity Framework Core InMemoryDatabase.

## Contexte

L'application a besoin de stocker et retrouver des données (produits, clients, commandes, fournisseurs). En phase de développement et de prototypage, nous voulons :
- Minimiser les dépendances externes
- Garder le code de persistance simple et lisible
- Permettre un démarrage immédiat sans aucune configuration

Initialement, le projet utilisait Entity Framework Core avec InMemoryDatabase. Cette approche a été **remplacée** par un stockage direct via des dictionnaires `Dictionary<Guid, TEntity>` pour plus de simplicité et d'indépendance.

## Décision

Nous utilisons une classe `InMemoryDataStore` contenant **4 dictionnaires** comme mécanisme de persistance :

```csharp
public class InMemoryDataStore
{
    public Dictionary<Guid, Product> Products { get; } = new();
    public Dictionary<Guid, Customer> Customers { get; } = new();
    public Dictionary<Guid, Supplier> Suppliers { get; } = new();
    public Dictionary<Guid, Order> Orders { get; } = new();
}
```

### Configuration

```csharp
// Singleton pour persister les données tant que l'app tourne
builder.Services.AddSingleton<InMemoryDataStore>();

// Repositories en Scoped (une instance par requête HTTP)
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<ICustomerRepository, InMemoryCustomerRepository>();
// etc.
```

### Aucun package NuGet requis

La couche Infrastructure n'a **aucune dépendance externe** — uniquement une référence projet vers le Domain.

## Alternatives considérées

### Entity Framework Core InMemoryDatabase (ancien choix)
- **Avantage** : ORM complet, migrations, Fluent API, transition facile vers SQL
- **Inconvénient** : 3 packages NuGet, complexité accrue (InMemoryDataStore, OnModelCreating, tracking), overhead pour un projet de formation
- **Remplacé** car la simplicité des dictionnaires est plus adaptée au contexte du projet.

### Fichiers JSON
- **Avantage** : Persistance entre redémarrages, lisible
- **Inconvénient** : Sérialisation/désérialisation à chaque opération, problèmes de concurrence, pas de requêtes efficaces
- **Rejeté** car trop lent pour des opérations fréquentes et complexité inutile.

### SQLite via Dapper
- **Avantage** : Persistance durable, performances, fichier unique
- **Inconvénient** : SQL à écrire manuellement, installation requise
- **Rejeté pour le développement**, mais envisageable pour la production.

## Conséquences

### Positives
- **Zéro dépendance** : aucun package NuGet dans la couche Infrastructure
- **Simplicité maximale** : le code des repositories est immédiatement compréhensible
- **Performances** : accès O(1) par clé (Guid), pas d'overhead ORM
- **Démarrage instantané** : aucune configuration, aucune connection string
- **Tests facilités** : même approche pour les tests unitaires et d'intégration
- **Indépendance technique** : le projet ne dépend d'aucun framework de persistance

### Négatives
- Les données sont **perdues à chaque redémarrage** de l'application
- Pas de **contraintes d'intégrité référentielle** automatiques (FK, unicité)
- Pas de **transactions** au sens ACID
- La recherche par critère autre que l'Id nécessite un parcours linéaire O(n)

## Migration future

Grâce au **Repository Pattern**, la migration vers une vraie base de données ne nécessite que :

1. Ajouter le package NuGet approprié (Dapper, etc.)
2. Créer de nouvelles classes de repositories implémentant les mêmes interfaces
3. Modifier l'enregistrement DI dans `Program.cs`

**Aucun autre code ne change** : ni le Domain, ni l'Application, ni l'API.
