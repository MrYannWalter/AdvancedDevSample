# ADR-004 : Hiérarchie d'Exceptions par Couche

## Statut
**Accepté** — Pattern transversal de gestion d'erreurs.

## Contexte

Dans une application en couches, les erreurs peuvent survenir à différents niveaux :
- Règle métier violée (prix négatif)
- Ressource non trouvée (produit inexistant)
- Problème technique (base de données inaccessible)

Nous avons besoin d'un mécanisme uniforme pour transformer ces erreurs en réponses HTTP cohérentes.

## Décision

Nous définissons **une exception typée par couche** :

| Couche | Exception | Code HTTP | Usage |
|--------|-----------|-----------|-------|
| Domain | `DomainException` | 400 Bad Request | Règle métier violée |
| Application | `ApplicationServiceException` | Variable (404, 409...) | Cas d'utilisation échoué |
| Infrastructure | `InfrastructureException` | 500 Internal Server Error | Problème technique |

Un **middleware global** (`ExceptionHandlingMiddleware`) intercepte ces exceptions et les transforme en réponses HTTP standardisées.

## Pourquoi un middleware plutôt que des try/catch ?

### Sans middleware (rejeté)
```csharp
// Chaque action de chaque controller doit gérer les erreurs
[HttpPut("{id}/price")]
public IActionResult ChangePrice(Guid id, ChangePriceRequest request)
{
    try { ... }
    catch (DomainException ex) { return BadRequest(ex.Message); }
    catch (ApplicationServiceException ex) { return NotFound(ex.Message); }
    catch (Exception ex) { return StatusCode(500); }
}
```
→ Duplication massive, risque d'oubli, code illisible.

### Avec middleware (adopté)
```csharp
// Le controller ne gère aucune erreur
[HttpPut("{id}/price")]
public IActionResult ChangePrice(Guid id, ChangePriceRequest request)
{
    _productService.ChangeProductPrice(id, request);
    return NoContent();
}
```
→ Le middleware se charge de tout de manière centralisée.

## Règles de logging

| Exception | Niveau de log | Justification |
|-----------|--------------|---------------|
| `DomainException` | Error | Erreur métier, potentiellement un bug ou une mauvaise utilisation |
| `ApplicationServiceException` | Warning | Situation attendue (ressource non trouvée) |
| `InfrastructureException` | Error | Problème technique à investiguer |
| `Exception` (autre) | Critical | Erreur inattendue, nécessite une intervention urgente |

## Conséquences

### Positives
- Réponses d'erreur cohérentes sur toute l'API
- Les controllers restent simples et sans try/catch
- Le logging est centralisé et systématique
- Les détails techniques ne sont jamais exposés au client (sécurité)

### Négatives
- Il faut penser à lever le bon type d'exception dans la bonne couche
- Le middleware est un point unique de transformation (SPOF de la gestion d'erreurs)
