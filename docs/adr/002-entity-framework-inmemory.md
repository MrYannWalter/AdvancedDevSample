# ADR-002 : Utilisation d'Entity Framework Core avec InMemoryDatabase

## Statut
**Accepté** — Choix technique pour la persistance.

## Contexte

L'application a besoin de stocker et retrouver des données (produits, clients, commandes). En phase de développement et de prototypage, nous voulons minimiser les dépendances externes (pas d'installation de SQL Server, PostgreSQL...).

## Décision

Nous utilisons **Entity Framework Core 8.0** comme ORM avec une base **InMemoryDatabase** pour le développement.

Le package **SQLite** est également inclus pour permettre une transition facile vers une persistance durable.

### Packages NuGet
- `Microsoft.EntityFrameworkCore 8.0.0`
- `Microsoft.EntityFrameworkCore.InMemory 8.0.0`
- `Microsoft.EntityFrameworkCore.Sqlite 8.0.0`

### Configuration
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AdvancedDevSampleDb"));
```

## Alternatives considérées

### Dapper (micro-ORM)
- **Avantage** : Performances brutes, contrôle total du SQL
- **Inconvénient** : Pas de tracking d'entités, pas de migrations, plus de code SQL à écrire
- **Rejeté** car EF Core offre un meilleur rapport productivité/fonctionnalités pour un projet de cette taille.

### Fichiers JSON
- **Avantage** : Ultra-simple, aucune dépendance
- **Inconvénient** : Pas de requêtes efficaces, pas de relations, pas de transactions
- **Rejeté** car insuffisant pour gérer des relations (commandes/articles).

### SQL Server directement
- **Avantage** : Robuste, performant en production
- **Inconvénient** : Nécessite une installation, une connection string, un serveur
- **Rejeté pour le développement**, mais recommandé pour la production.

## Conséquences

### Positives
- Démarrage immédiat : aucune installation de base de données requise
- Tests d'intégration rapides (base en mémoire)
- Transition vers SQLite ou SQL Server possible en changeant une ligne de configuration
- EF Core gère automatiquement le mapping entités/tables

### Négatives
- Les données sont perdues à chaque redémarrage de l'application
- InMemory ne supporte pas toutes les fonctionnalités SQL (transactions, contraintes FK)
- Certains comportements peuvent différer d'une vraie base de données

## Migration vers la production

Pour passer en production avec SQLite :
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=advanceddevsample.db"));
```

Pour SQL Server :
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
