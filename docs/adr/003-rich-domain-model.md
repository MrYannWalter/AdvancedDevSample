# ADR-003 : Modèle de Domaine Riche (Rich Domain Model)

## Statut
**Accepté** — Choix fondamental de conception du Domain.

## Contexte

Il existe deux approches pour modéliser les entités métier :

1. **Modèle anémique** : les entités sont de simples conteneurs de données (getters/setters publics). La logique est dans les services.
2. **Modèle riche** : les entités contiennent leur propre logique métier. Les setters sont privés et les modifications passent par des méthodes explicites.

## Décision

Nous adoptons un **modèle de domaine riche**. Les entités :

- Ont des **setters privés** : on ne peut pas modifier directement une propriété
- Exposent des **méthodes métier** : `ChangePrice()`, `Confirm()`, `Cancel()`
- Valident leurs **invariants** à chaque modification
- Lèvent des `DomainException` si une règle métier est violée

### Exemple concret

```csharp
// ❌ Modèle anémique (rejeté)
product.Price = -5;           // Aucune validation !
product.IsActive = false;     // On peut tout faire

// ✅ Modèle riche (adopté)
product.ChangePrice(-5);      // → DomainException: "Le prix doit être positif"
product.Deactivate();         // Seule façon de désactiver, contrôlée
```

## Pourquoi ce choix ?

### Le problème du modèle anémique
Quand les entités ne sont que des "sacs de données", la logique métier se retrouve dispersée dans les services. Résultat :
- Un développeur peut oublier une validation
- La même vérification est dupliquée dans plusieurs services
- Les tests unitaires doivent tester le service entier au lieu de l'entité seule

### L'avantage du modèle riche
La logique est **au plus près des données**. L'entité Product sait elle-même qu'un prix doit être positif. Aucun service ne peut contourner cette règle.

## Alternatives considérées

### Modèle anémique + services riches
- **Avantage** : Plus simple à comprendre pour les débutants
- **Inconvénient** : Duplication de la logique, risque d'incohérence
- **Rejeté** car le Domain perdrait son rôle de garant des règles métier.

## Conséquences

### Positives
- Les règles métier sont centralisées et impossibles à contourner
- Les entités sont testables unitairement (pas besoin de services)
- Le code est auto-documenté : `order.Confirm()` est plus clair que `orderService.UpdateStatus(order, "Confirmed")`

### Négatives
- Les constructeurs sont plus complexes (validations)
- EF Core nécessite un constructeur vide pour le mapping (constructeur ORM)
- Les setters privés demandent une configuration EF spécifique (`PropertyAccessMode.Field`)
