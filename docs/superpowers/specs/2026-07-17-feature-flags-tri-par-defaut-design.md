# Design — Feature flags ConfigCat pour le tri par défaut du portefeuille (back)

Date : 2026-07-17
Repo : Pulse.Back.Account — branche `feature/feature-flags-tri-par-defaut` (depuis `origin/main`, 0be50fb)

## Contexte

Deux fonctionnalités récentes modifient le tri par défaut de la liste des comptes du portefeuille
(endpoint `GET /accounts` → `AccountController.GetAccountsAsync`) :

- favoris en premier (`IsFavorite` desc) — PR 10422 mergée dans main ;
- puis dernière activité (`LastActivityDate` desc) — PR 10345 mergée dans main.

Ce comportement est aujourd'hui **inconditionnel** dans
`Account.Infrastructure/Repositories/AccountRepository.cs` (`GetAccountRolePairsSorted`, branche
« défaut » quand aucun `Sorting.Field` n'est fourni). Le métier veut pouvoir activer chaque
fonctionnalité via ConfigCat après sa communication.

Répartition : le front (Pulse.Web) est traité par un collègue (masquage colonne dernière activité +
filtres). **Ce design couvre uniquement le back.**

## Décisions validées

- **Le flag vit côté back** : le tri par défaut est une décision du back ; un flag uniquement côté
  front laisserait le nouveau défaut actif pour tout client n'envoyant pas de paramètre de tri, et
  ne permettrait pas de composer les deux fonctionnalités indépendamment (un seul `Sorting.Field`
  possible par requête). ConfigCat/OpenFeature est déjà en place côté back.
- **2 flags indépendants** : le métier peut activer favoris et dernière activité séparément.
- **Activation globale** (on/off simple, sans ciblage utilisateur) : évaluation sans email, comme
  l'usage existant (`AccountService.cs` → `IncludeProspectsInContactsSearch`).
- **Nommage** : `isFavoriteSortEnabled` (et non `isFavoriteFeatureEnabled`) car seul le **tri par
  défaut** par favoris est flagué — l'étoile et le filtre favoris restent actifs dans tous les cas.
  Côté dernière activité, `isLastActivityFeatureEnabled` flague la feature entière (colonne +
  filtre + tri), le nom existant est conservé.

## Design

### 1. Clés de flag — `Account.Core/Constants/FeatureFlagKeys.cs`

```csharp
public const string LastActivityFeature = "isLastActivityFeatureEnabled"; // existe déjà dans ConfigCat (créée côté front)
public const string FavoriteSort = "isFavoriteSortEnabled";               // à créer dans ConfigCat
```

Front et back doivent utiliser **les mêmes clés** pour que le métier bascule un seul interrupteur
par fonctionnalité.

### 2. Évaluation — `Account.Core/Services/AccountService.GetAccountsAsync`

Suivre le pattern existant (`_featureFlagService.IsEnabledAsync(...)`) : évaluer les deux flags
sans email, puis les transmettre au repository via un record dédié :

```csharp
public record DefaultSortOptions(bool FavoriteFirst, bool LastActivityFirst);
```

Nouveau paramètre de `IAccountRepository.GetAccountsAsync(criteria, pagination, defaultSort)`.

Un paramètre séparé (et non des propriétés dans `SearchAccountCriteria`) car le criteria est bindé
`[FromQuery]` : des propriétés settables y seraient injectables par query string.

### 3. Composition du tri — `AccountRepository.GetAccountRolePairsSorted`

Seule la branche « défaut » change ; switch explicite sur les deux booléens :

| FavoriteFirst | LastActivityFirst | Chaîne de tri |
|---|---|---|
| true | true | `IsFavorite` desc → `LastActivityDate` desc → `LegalName` (comportement actuel) |
| true | false | `IsFavorite` desc → `LegalName` |
| false | true | `LastActivityDate` desc → `LegalName` |
| false | false | `LegalName` (comportement historique) |

Inchangés : le tiebreaker final `AccountId` (déterminisme de pagination), la branche de tri
explicite (`sorting.Field` switch), et `GetAccountEntitiesSorted` (endpoint « tous les comptes »,
défaut `LegalName`).

### 4. Périmètre / cas limites

- **Le tracking de la dernière activité n'est pas flagué** : l'écriture de `LastActivityDate`
  (endpoint `updateLastActivityDate` et toute alimentation de la donnée) continue en permanence,
  flags ON ou OFF. Ainsi, à l'activation, la donnée est déjà à jour. Seuls l'affichage côté front
  et les tris par défaut sont conditionnés.
- Flag OFF ne bloque **pas** le tri explicite `lastActivityDate` ni les filtres
  `LastActivityDateFrom/To` et `IsFavoriteFilter` : le front masque les contrôles correspondants ;
  les rejeter côté back ajouterait de la complexité sans bénéfice. Les flags gouvernent uniquement
  l'expérience par défaut.
- Valeur par défaut SDK = `false` (`IsEnabledAsync` → `GetBooleanValueAsync(flagKey, false, ...)`)
  → si un flag est absent de ConfigCat, retour au comportement historique : sans risque.

### 5. Configuration

`appsettings.Development.json` → `FeatureFlags:Defaults` : ajouter les deux clés à `true`
(le fallback `InMemoryProvider` les sert en local quand `ConfigCat:SdkKey` est vide), afin que le
dev local conserve le comportement actuel.

### 6. Tests

- **Infrastructure** (tests de tri global existants) : couvrir les 4 combinaisons de
  `DefaultSortOptions` ; vérifier qu'un tri explicite ignore les flags.
- **Core** (`AccountService`) : mock `IFeatureFlagService`, vérifier que les deux flags sont
  évalués et transmis au repository (pattern des tests prospects existants).

## Points en suspens (hors code)

1. Créer `isFavoriteSortEnabled` dans ConfigCat (et prévenir le collègue front si le front doit
   aussi le consommer).
2. Vérifier que `ConfigCat:SdkKey` (back) et `VITE_CONFIGCAT_SDK_KEY` (front) pointent sur la même
   config ConfigCat ; sinon créer les flags dans les deux configs.
