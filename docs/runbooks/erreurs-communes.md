# Runbook : Erreurs Communes et Résolution

## Vue d'ensemble

Ce guide répertorie les erreurs que vous pourriez rencontrer lors du développement ou de l'utilisation de l'application, avec des étapes de diagnostic et de résolution.

---

## Erreurs HTTP côté client

### HTTP 400 - Bad Request (Erreur Métier)

**Message type** : `{ "title": "Erreur Métier", "detail": "Le prix doit être strictement positif." }`

**Origine** : Une `DomainException` a été levée par une entité du Domain.

**Causes fréquentes** :

| Message | Cause | Solution |
|---------|-------|----------|
| "Le prix doit être strictement positif." | Prix envoyé ≤ 0 | Envoyer un prix > 0 |
| "Le produit est inactif." | Tentative de modification d'un produit désactivé | Activer le produit d'abord (`PUT /api/products/{id}/activate`) |
| "Le nom du produit est obligatoire." | Champ `name` vide ou null | Fournir un nom non vide |
| "Impossible d'ajouter un article à une commande qui n'est pas en attente." | La commande est déjà confirmée/expédiée | Créer une nouvelle commande |
| "Impossible de confirmer une commande sans articles." | La commande n'a aucun article | Ajouter au moins un article avant de confirmer |
| "Impossible d'annuler une commande déjà livrée." | La commande est au statut Delivered | Aucune action possible, gérer via un retour |
| "La quantité doit être supérieure à zéro." | Quantité ≤ 0 dans une ligne de commande | Envoyer une quantité ≥ 1 |

**Diagnostic** :
1. Lire le champ `detail` de la réponse — il contient le message d'erreur métier exact
2. Vérifier les données envoyées dans le body de la requête
3. Si la ressource est désactivée, l'activer avant de la modifier

---

### HTTP 404 - Not Found (Ressource Introuvable)

**Message type** : `{ "title": "Ressource introuvable", "detail": "Produit introuvable." }`

**Origine** : Une `ApplicationServiceException` a été levée par un service de la couche Application.

**Causes fréquentes** :

| Message | Cause | Solution |
|---------|-------|----------|
| "Produit introuvable." | L'ID produit n'existe pas en base | Vérifier l'ID, lister les produits (`GET /api/products`) |
| "Client introuvable." | L'ID client n'existe pas | Vérifier l'ID, lister les clients (`GET /api/customers`) |
| "Commande introuvable." | L'ID commande n'existe pas | Vérifier l'ID |
| "Fournisseur introuvable." | L'ID fournisseur n'existe pas | Vérifier l'ID |

**Diagnostic** :
1. Vérifier que l'ID dans l'URL est bien un GUID valide
2. Appeler le endpoint GET correspondant pour vérifier l'existence de la ressource
3. Avec InMemoryDatabase : les données sont perdues au redémarrage !

---

### HTTP 500 - Internal Server Error (Erreur Technique)

**Message type** : `{ "error": "Erreur Technique" }` ou `{ "error": "Erreur Interne" }`

**Origine** : `InfrastructureException` ou exception non gérée.

**Diagnostic** :
1. **Consulter les logs** de l'application (console ou fichier)
2. Chercher les entrées `LogError` (InfrastructureException) ou `LogCritical` (exception inattendue)
3. L'erreur détaillée est dans les logs, jamais dans la réponse HTTP (sécurité)

**Causes fréquentes** :
- Base de données inaccessible (si migré vers SQLite/SQL Server)
- Violation de contrainte d'intégrité (FK, unicité)
- Problème de configuration Entity Framework

**Résolution** :
1. Vérifier la connection string dans `appsettings.json`
2. Vérifier que la base de données est accessible
3. Vérifier que les migrations EF ont été appliquées (si applicable)

---

## Erreurs au démarrage de l'application

### "Unable to resolve service for type..."

**Cause** : Une dépendance n'est pas enregistrée dans `Program.cs`.

**Résolution** :
1. Vérifier que le service manquant est bien enregistré dans `Program.cs`
2. Exemple : `builder.Services.AddScoped<IProductRepository, EfProductRepository>();`
3. Vérifier les dépendances transitives (un service a besoin d'un autre service)

### "No database provider has been configured for this DbContext"

**Cause** : `AppDbContext` n'est pas configuré dans `Program.cs`.

**Résolution** :
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AdvancedDevSampleDb"));
```

### "The entity type 'X' requires a primary key to be defined"

**Cause** : EF Core ne trouve pas la clé primaire d'une entité.

**Résolution** :
- Vérifier que l'entité a une propriété `Id` de type `Guid`
- Ou ajouter `entity.HasKey(e => e.Id)` dans la configuration Fluent API

---

## Erreurs lors des tests

### Tests d'intégration qui échouent avec "Connection refused"

**Cause** : `WebApplicationFactory` n'arrive pas à démarrer le serveur de test.

**Résolution** :
1. Vérifier que `Program.cs` est accessible (`public partial class Program {}` ou `InternalsVisibleTo`)
2. Vérifier que `CustomWebApplicationFactory` remplace bien les dépendances de test

### Tests unitaires Domain qui échouent

**Cause** : Règle métier qui a changé ou test obsolète.

**Résolution** :
1. Lire le message d'erreur du test
2. Comparer avec la règle métier actuelle dans l'entité
3. Mettre à jour le test si la règle a été intentionnellement modifiée

---

## Erreurs fréquentes en développement

### "InvalidOperationException: The instance of entity type 'X' cannot be tracked"

**Cause** : Deux instances de la même entité (même Id) sont traquées par le même DbContext.

**Résolution** :
- Utiliser `AsNoTracking()` pour les lectures seules
- Ou s'assurer qu'une seule instance est chargée par requête

### Données perdues après redémarrage

**Cause** : Comportement normal avec InMemoryDatabase.

**Résolution** :
- C'est attendu en développement
- Pour persister : migrer vers SQLite (voir ADR-002)
- Pour les tests : utiliser le Seed dans les fixtures de test
