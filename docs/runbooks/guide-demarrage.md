# Runbook : Guide de Démarrage

## Prérequis

| Outil | Version minimale | Vérification |
|-------|-----------------|--------------|
| .NET SDK | 8.0 | `dotnet --version` |
| IDE | Visual Studio 2022 ou VS Code + C# Extension | — |
| Git | 2.x | `git --version` |

## Démarrage rapide

### 1. Cloner le projet
```bash
git clone <https://github.com/MrYannWalter/AdvancedDevSample>
cd AdvancedDevSample
```

### 2. Restaurer les packages NuGet
```bash
dotnet restore
```

### 3. Compiler la solution
```bash
dotnet build
```

### 4. Lancer l'application
```bash
cd AdvancedDevSample.Api
dotnet run
```

L'application démarre sur :
- **HTTP** : `http://localhost:5089`
- **HTTPS** : `https://localhost:7010`

### 5. Accéder à Swagger
Ouvrir dans un navigateur : `https://localhost:7010/swagger`

## Premier test fonctionnel

### Créer un client
```bash
curl -X POST https://localhost:7010/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName": "Jean", "lastName": "Dupont", "email": "jean@test.com"}'
```

### Créer un produit
```bash
curl -X POST https://localhost:7010/api/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Laptop", "description": "PC portable 15 pouces", "price": 999.99}'
```

### Créer une commande
```bash
# Utiliser l'ID du client retourné
curl -X POST https://localhost:7010/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId": "<ID_CLIENT>"}'
```

### Ajouter un article à la commande
```bash
curl -X POST https://localhost:7010/api/orders/<ID_COMMANDE>/items \
  -H "Content-Type: application/json" \
  -d '{"productId": "<ID_PRODUIT>", "quantity": 2}'
```

### Confirmer la commande
```bash
curl -X PUT https://localhost:7010/api/orders/<ID_COMMANDE>/confirm
```

## Lancer les tests

```bash
# Tous les tests
dotnet test

# Tests avec détails
dotnet test --verbosity detailed

# Tests d'un projet spécifique
dotnet test AdvancedDevSample.Test
```

## Structure des URLs de l'API

```
https://localhost:7010/
├── swagger              ← Documentation interactive
├── api/products         ← CRUD Produits
├── api/customers        ← CRUD Clients
├── api/suppliers        ← CRUD Fournisseurs
└── api/orders           ← CRUD Commandes + gestion cycle de vie
```

## Conseils pour le développement

### Ajouter une nouvelle entité
1. Créer l'entité dans `Domain/Entities/`
2. Créer l'interface repository dans `Domain/Interfaces/`
3. Créer le service dans `Application/Services/`
4. Créer les DTOs dans `Application/DTOs/`
5. Créer le repository dans `Infrastructure/Repositories/`
6. Ajouter le dictionnaire dans `InMemoryDataStore`
7. Créer le controller dans `Api/Controllers/`
8. Enregistrer les dépendances dans `Program.cs`

### Modifier une entité existante
1. Modifier l'entité dans le Domain (ajouter la propriété/méthode)
2. Mettre à jour le DTO Response pour exposer la nouvelle propriété
3. Mettre à jour le DTO Request si c'est une donnée entrante
4. Mettre à jour `InMemoryDataStore` si nécessaire
5. Écrire les tests unitaires correspondants
