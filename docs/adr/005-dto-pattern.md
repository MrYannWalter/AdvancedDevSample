# ADR-005 : Utilisation du Pattern DTO (Data Transfer Objects)

## Statut
**Accepté** — Pattern de communication entre couches API et Application.

## Contexte

Les entités du Domain ont des setters privés et contiennent de la logique métier. Elles ne sont pas adaptées pour :
- Recevoir directement les données JSON d'une requête HTTP
- Être sérialisées directement en réponse JSON (risque d'exposer des données internes)

## Décision

Nous utilisons des **DTOs** (Data Transfer Objects) pour séparer les données entrantes/sortantes des entités métier :

- **Request DTOs** : objets simples avec des propriétés publiques et des attributs de validation (`[Required]`, `[Range]`, `[EmailAddress]`)
- **Response DTOs** : objets contenant une méthode `static FromEntity()` pour mapper une entité vers une réponse

### Convention de nommage

| Type | Pattern | Exemple |
|------|---------|---------|
| Création | `Create{Entity}Request` | `CreateProductRequest` |
| Modification | `Update{Entity}Request` | `UpdateProductRequest` |
| Action spécifique | `{Action}Request` | `ChangePriceRequest`, `AddOrderItemRequest` |
| Réponse | `{Entity}Response` | `ProductResponse`, `OrderResponse` |

## Pourquoi pas de bibliothèque de mapping (AutoMapper) ?

Pour un projet de cette taille, le mapping est simple et explicite. La méthode `FromEntity()` statique :
- Est facile à comprendre
- Ne nécessite aucune configuration
- Est vérifiable à la compilation
- Évite une dépendance externe

Si le projet grandit et que le mapping devient complexe, on pourra adopter AutoMapper plus tard.

## Validation à deux niveaux

```
Requête HTTP → DTO (validation format) → Service → Entité (validation métier)
```

- **Niveau 1 (DTO)** : validation de format par ASP.NET Core (le champ est-il rempli ? le nombre est-il positif ?)
- **Niveau 2 (Entité)** : validation métier par le Domain (le produit est-il actif ? la commande est-elle en attente ?)

Les deux niveaux sont complémentaires et ne se remplacent pas.

## Conséquences

### Positives
- Les entités du Domain restent protégées
- Les données exposées à l'API sont contrôlées
- La validation de format se fait avant d'atteindre le métier
- On peut faire évoluer l'API sans changer le Domain

### Négatives
- Plus de classes à créer (Request + Response par entité)
- Le mapping manuel peut être oublié si on ajoute une propriété
