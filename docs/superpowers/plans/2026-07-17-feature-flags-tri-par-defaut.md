# Feature flags tri par défaut du portefeuille — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Conditionner le tri par défaut du portefeuille (favoris d'abord / dernière activité) à deux feature flags ConfigCat évalués côté back.

**Architecture:** `AccountService.GetAccountsAsync` évalue les deux flags via le `IFeatureFlagService` existant et transmet un record `DefaultSortOptions` au repository. `AccountRepository.GetAccountRolePairsSorted` compose la chaîne de tri par défaut selon les deux booléens. Le tri explicite (`Sorting.Field`), le tiebreaker `AccountId` et le chemin d'écriture de `LastActivityDate` sont inchangés.

**Tech Stack:** .NET 8, EF Core (InMemory pour les tests), xUnit + FluentAssertions + Moq + AutoFixture, OpenFeature/ConfigCat (déjà en place).

**Spec:** `docs/superpowers/specs/2026-07-17-feature-flags-tri-par-defaut-design.md`

## Global Constraints

- Repo : `C:\sources\back\Pulse.Back.Account`, branche `feature/feature-flags-tri-par-defaut`. Toutes les commandes s'exécutent depuis la racine du repo.
- Clés de flag exactes (partagées avec le front) : `isLastActivityFeatureEnabled` (existe déjà dans ConfigCat) et `isFavoriteSortEnabled` (à créer dans ConfigCat — hors code).
- Tout fichier `.cs` commence par l'en-tête `// <copyright file="X.cs" company="Pulse">` (StyleCop).
- Aucune nouvelle dépendance NuGet.
- Valeur par défaut d'un flag absent = `false` (comportement historique) — ne pas changer le défaut de `IsEnabledAsync`.
- `GetAccountEntitiesSorted` (endpoint « tous les comptes ») ne doit PAS être modifié.
- Solution : `01_account/Pulse.Back.Account.sln`.

---

### Task 1: Composition du tri par défaut dans le repository

**Files:**
- Create: `01_account/src/Account.Core/Requests/DefaultSortOptions.cs`
- Modify: `01_account/src/Account.Core/Interfaces/IAccountRepository.cs:15`
- Modify: `01_account/src/Account.Infrastructure/Repositories/AccountRepository.cs:87,103,130-154`
- Test: `01_account/tests/Account.Infrastructure.Tests/Repositories/AccountRepositoryTests.cs` (région `Default sort favorites first`, lignes ~4213-4261)

**Interfaces:**
- Consumes: helpers de test existants `CreateActivityContact(string email, int contactId)`, `CreateAccountWithActivity(ContactEntity contact, string legalName, DateTime? lastActivityDate, bool isFavorite = false)`, champ `ActivityReference` (DateTime), `TestAccountContext`.
- Produces: `public record DefaultSortOptions(bool FavoriteFirst, bool LastActivityFirst)` dans `Pulse.Account.Core.Requests` ; signature `Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination, DefaultSortOptions? defaultSort = null)` sur `IAccountRepository` (null ⇒ `(true, true)` = comportement actuel — utilisé par la Task 2).

- [ ] **Step 1: Créer le record `DefaultSortOptions`**

Créer `01_account/src/Account.Core/Requests/DefaultSortOptions.cs` :

```csharp
// <copyright file="DefaultSortOptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests;

/// <summary>
/// Options pilotant le tri par défaut du portefeuille quand aucun tri explicite n'est demandé.
/// Les valeurs proviennent des feature flags ConfigCat, évaluées dans AccountService.
/// </summary>
/// <param name="FavoriteFirst">Place les comptes favoris en tête de liste.</param>
/// <param name="LastActivityFirst">Trie ensuite par date de dernière activité décroissante.</param>
public record DefaultSortOptions(bool FavoriteFirst, bool LastActivityFirst);
```

- [ ] **Step 2: Écrire les 4 tests qui échouent (région `Default sort favorites first`)**

Dans `01_account/tests/Account.Infrastructure.Tests/Repositories/AccountRepositoryTests.cs`, ajouter dans la région `#region Default sort favorites first` (avant le `#endregion`, ligne ~4261) :

```csharp
[Fact]
public async Task GetAccountsAsync_WhenLastActivitySortDisabled_ShouldSortFavoritesThenAlphabeticalAsync()
{
    using var context = new TestAccountContext(_dbContextOptions);
    var contact = CreateActivityContact("current@test.fr", 500);
    context.AccountEntity.AddRange(
        CreateAccountWithActivity(contact, "bbb fav old", ActivityReference.AddDays(-40), isFavorite: true),
        CreateAccountWithActivity(contact, "zzz fav recent", ActivityReference.AddDays(-1), isFavorite: true),
        CreateAccountWithActivity(contact, "aaa reg recent", ActivityReference.AddDays(-2)));
    await context.SaveChangesAsync();
    var repository = new AccountRepository(context);
    var criteria = new SearchAccountCriteria
    {
        ContactId = contact.ContactId,
    };

    var result = await repository.GetAccountsAsync(
        criteria,
        new Pagination { PageNumber = 1, PageSize = 10 },
        new DefaultSortOptions(FavoriteFirst: true, LastActivityFirst: false));

    result.Items.Select(account => account.LegalName)
        .Should().Equal("bbb fav old", "zzz fav recent", "aaa reg recent");
}

[Fact]
public async Task GetAccountsAsync_WhenFavoriteSortDisabled_ShouldSortByActivityWithoutFavoritesFirstAsync()
{
    using var context = new TestAccountContext(_dbContextOptions);
    var contact = CreateActivityContact("current@test.fr", 500);
    context.AccountEntity.AddRange(
        CreateAccountWithActivity(contact, "fav old", ActivityReference.AddDays(-40), isFavorite: true),
        CreateAccountWithActivity(contact, "reg recent", ActivityReference.AddDays(-1)),
        CreateAccountWithActivity(contact, "reg none", null));
    await context.SaveChangesAsync();
    var repository = new AccountRepository(context);
    var criteria = new SearchAccountCriteria
    {
        ContactId = contact.ContactId,
    };

    var result = await repository.GetAccountsAsync(
        criteria,
        new Pagination { PageNumber = 1, PageSize = 10 },
        new DefaultSortOptions(FavoriteFirst: false, LastActivityFirst: true));

    result.Items.Select(account => account.LegalName)
        .Should().Equal("reg recent", "fav old", "reg none");
}

[Fact]
public async Task GetAccountsAsync_WhenBothDefaultSortsDisabled_ShouldSortAlphabeticallyAsync()
{
    using var context = new TestAccountContext(_dbContextOptions);
    var contact = CreateActivityContact("current@test.fr", 500);
    context.AccountEntity.AddRange(
        CreateAccountWithActivity(contact, "zzz fav recent", ActivityReference.AddDays(-1), isFavorite: true),
        CreateAccountWithActivity(contact, "mmm reg", ActivityReference.AddDays(-5)),
        CreateAccountWithActivity(contact, "aaa reg none", null));
    await context.SaveChangesAsync();
    var repository = new AccountRepository(context);
    var criteria = new SearchAccountCriteria
    {
        ContactId = contact.ContactId,
    };

    var result = await repository.GetAccountsAsync(
        criteria,
        new Pagination { PageNumber = 1, PageSize = 10 },
        new DefaultSortOptions(FavoriteFirst: false, LastActivityFirst: false));

    result.Items.Select(account => account.LegalName)
        .Should().Equal("aaa reg none", "mmm reg", "zzz fav recent");
}
```

```csharp
[Fact]
public async Task GetAccountsAsync_WhenExplicitLastActivitySortAndFlagsDisabled_ShouldStillSortByActivityAsync()
{
    using var context = new TestAccountContext(_dbContextOptions);
    var contact = CreateActivityContact("current@test.fr", 500);
    context.AccountEntity.AddRange(
        CreateAccountWithActivity(contact, "aaa old", ActivityReference.AddDays(-40)),
        CreateAccountWithActivity(contact, "zzz recent", ActivityReference.AddDays(-1)));
    await context.SaveChangesAsync();
    var repository = new AccountRepository(context);
    var criteria = new SearchAccountCriteria
    {
        ContactId = contact.ContactId,
        Sorting = new Sorting { Field = SortingConstants.LASTACTIVITYDATE, Descending = true },
    };

    var result = await repository.GetAccountsAsync(
        criteria,
        new Pagination { PageNumber = 1, PageSize = 10 },
        new DefaultSortOptions(FavoriteFirst: false, LastActivityFirst: false));

    result.Items.Select(account => account.LegalName)
        .Should().Equal("zzz recent", "aaa old");
}
```

Pourquoi ces jeux de données : chaque test contient un cas qui serait ordonné différemment si l'option désactivée était encore active (ex. test 1 : « zzz fav recent » passerait devant « bbb fav old » si le tri activité était actif ; test 4 : l'ordre serait alphabétique « aaa old » d'abord si les options par défaut s'appliquaient au tri explicite — prouve que flags OFF ne bloque pas le tri explicite, cf. spec « cas limites »).

- [ ] **Step 3: Vérifier que ça ne compile pas / échoue**

Run: `dotnet test 01_account/tests/Account.Infrastructure.Tests --filter "FullyQualifiedName~AccountRepositoryTests"`
Expected: FAIL — erreur de compilation CS1501 (`GetAccountsAsync` ne prend pas 3 arguments).

- [ ] **Step 4: Modifier l'interface `IAccountRepository`**

Dans `01_account/src/Account.Core/Interfaces/IAccountRepository.cs`, remplacer :

```csharp
Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination);
```

par :

```csharp
Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination, DefaultSortOptions? defaultSort = null);
```

(`using Pulse.Account.Core.Requests;` est déjà présent dans ce fichier.)

- [ ] **Step 5: Implémenter la composition dans `AccountRepository`**

Dans `01_account/src/Account.Infrastructure/Repositories/AccountRepository.cs` :

a) Signature de `GetAccountsAsync` (ligne 87) :

```csharp
public async Task<Paging<AccountModel>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination, DefaultSortOptions? defaultSort = null)
```

b) Appel du tri (ligne 103) :

```csharp
var pageAccountIds = await GetAccountRolePairsSorted(baseQuery, criteria.Sorting, defaultSort ?? new DefaultSortOptions(FavoriteFirst: true, LastActivityFirst: true))
```

(le fallback `(true, true)` préserve le comportement actuel pour les appels sans options ; en production `AccountService` passe toujours les valeurs des flags.)

c) `GetAccountRolePairsSorted` : remplacer la branche « défaut » par un appel à une nouvelle méthode `SortByDefault`, le reste est inchangé :

```csharp
private static IQueryable<AccountRolePair> GetAccountRolePairsSorted(IQueryable<AccountRolePair> query, Sorting? sorting, DefaultSortOptions defaultSort)
{
    var sorted = sorting == null || string.IsNullOrEmpty(sorting.Field)
        ? SortByDefault(query, defaultSort)
        : sorting.Field switch
        {
            SortingConstants.COMPANYNAME => query.OrderByDirection(x => x.Account.LegalName, sorting.Descending),
            SortingConstants.CUSTOMERCODE => query.OrderByDirection(x => x.Account.AccountNumber, sorting.Descending),
            SortingConstants.LEADER => query.OrderByDirection(SignatoryNameKey, sorting.Descending),
            SortingConstants.EMAIL => query.OrderByDirection(SignatoryEmailKey, sorting.Descending),
            SortingConstants.CITY => query.OrderByDirection(x => x.Account.AddressEntity.Select(a => a.City).FirstOrDefault(), sorting.Descending),
            SortingConstants.STATUS => query.OrderByDirection(x => x.Account.DeploymentEntity.Status, sorting.Descending),
            SortingConstants.LASTACTIVITYDATE => query
                .OrderByDirection(x => x.Role.LastActivityDate, sorting.Descending)
                .ThenBy(x => x.Account.LegalName),
            _ => throw new BadRequestException(
                Errors.BadRequestContactsAccountCode,
                string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field)),
        };

    return sorted.ThenBy(x => x.Account.AccountId);
}

private static IOrderedQueryable<AccountRolePair> SortByDefault(IQueryable<AccountRolePair> query, DefaultSortOptions options) =>
    (options.FavoriteFirst, options.LastActivityFirst) switch
    {
        (true, true) => query
            .OrderByDescending(x => x.Role.IsFavorite == true)
            .ThenByDescending(x => x.Role.LastActivityDate)
            .ThenBy(x => x.Account.LegalName),
        (true, false) => query
            .OrderByDescending(x => x.Role.IsFavorite == true)
            .ThenBy(x => x.Account.LegalName),
        (false, true) => query
            .OrderByDescending(x => x.Role.LastActivityDate)
            .ThenBy(x => x.Account.LegalName),
        (false, false) => query.OrderBy(x => x.Account.LegalName),
    };
```

Ajouter `using Pulse.Account.Core.Requests;` en tête de fichier s'il n'y est pas déjà (vérifier — `Sorting` vient déjà de ce namespace donc il devrait y être).

- [ ] **Step 6: Vérifier que les tests passent**

Run: `dotnet test 01_account/tests/Account.Infrastructure.Tests --filter "FullyQualifiedName~AccountRepositoryTests"`
Expected: PASS — les 4 nouveaux tests ET les tests existants (`GetAccountsAsync_WhenNoSorting_ShouldReturnFavoritesFirstThenStandardOrderAsync` reste vert grâce au fallback `(true, true)` ; `GetAccountsAsync_WhenSortingByCompanyName_ShouldNotPutFavoritesFirstAsync` reste vert car la branche explicite est inchangée).

- [ ] **Step 7: Commit**

```bash
git add 01_account/src/Account.Core/Requests/DefaultSortOptions.cs 01_account/src/Account.Core/Interfaces/IAccountRepository.cs 01_account/src/Account.Infrastructure/Repositories/AccountRepository.cs 01_account/tests/Account.Infrastructure.Tests/Repositories/AccountRepositoryTests.cs
git commit -m "feat(infrastructure): compose default portfolio sort from options"
```

---

### Task 2: Évaluation des flags dans `AccountService`

**Files:**
- Modify: `01_account/src/Account.Core/Constants/FeatureFlagKeys.cs`
- Modify: `01_account/src/Account.Core/Services/AccountService.cs:62-74`
- Test: `01_account/tests/Account.Core.Tests/Services/AccountServiceTests.cs`

**Interfaces:**
- Consumes: `DefaultSortOptions` et la signature 3-args de `IAccountRepository.GetAccountsAsync` (Task 1) ; `IFeatureFlagService.IsEnabledAsync(string flagKey, string? userEmail = null, CancellationToken cancellationToken = default)` (existant, déjà injecté comme `_featureFlagService`).
- Produces: constantes `FeatureFlagKeys.FavoriteSort = "isFavoriteSortEnabled"` et `FeatureFlagKeys.LastActivityFeature = "isLastActivityFeatureEnabled"` (utilisées par la config Task 3).

- [ ] **Step 1: Ajouter les clés de flag**

Dans `01_account/src/Account.Core/Constants/FeatureFlagKeys.cs`, ajouter dans la classe :

```csharp
public const string LastActivityFeature = "isLastActivityFeatureEnabled";

public const string FavoriteSort = "isFavoriteSortEnabled";
```

- [ ] **Step 2: Mettre à jour les mocks existants (obligatoire — mock strict)**

`_accountRepository` est un `Mock<IAccountRepository>(MockBehavior.Strict)` : les setups 2-args compilés avec `defaultSort = null` ne matcheront plus l'appel du service (qui passe un objet non-null) et feraient tout échouer.

Dans `01_account/tests/Account.Core.Tests/Services/AccountServiceTests.cs`, remplacer TOUTES les occurrences (setups ET verifies — lignes ~53, 81, 110, 149, 171, 185, 218, 227, 252, 267) de :

```csharp
repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>())
```

par :

```csharp
repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<DefaultSortOptions>())
```

- [ ] **Step 3: Écrire le test qui échoue (flags transmis au repository)**

Ajouter dans la région `#region GetAccountsAsync coverage additions` :

```csharp
[Theory]
[InlineData(true, true)]
[InlineData(true, false)]
[InlineData(false, true)]
[InlineData(false, false)]
public async Task GetAccountsAsync_ShouldForwardSortFeatureFlagsToRepository(bool favoriteSort, bool lastActivitySort)
{
    _featureFlagService.Setup(f => f.IsEnabledAsync(FeatureFlagKeys.FavoriteSort, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(favoriteSort);
    _featureFlagService.Setup(f => f.IsEnabledAsync(FeatureFlagKeys.LastActivityFeature, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(lastActivitySort);
    var accountMocked = _fixture.Create<Paging<AccountModel>>();
    _accountRepository.Setup(repository =>
            repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<DefaultSortOptions>()))
        .ReturnsAsync(accountMocked);
    var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

    await accountService.GetAccountsAsync(new SearchAccountCriteria { ContactId = 123 }, new Pagination());

    _accountRepository.Verify(
        repository => repository.GetAccountsAsync(
            It.IsAny<SearchAccountCriteria>(),
            It.IsAny<Pagination>(),
            It.Is<DefaultSortOptions>(options => options.FavoriteFirst == favoriteSort && options.LastActivityFirst == lastActivitySort)),
        Times.Once);
}
```

- [ ] **Step 4: Vérifier que le nouveau test échoue**

Run: `dotnet test 01_account/tests/Account.Core.Tests --filter "FullyQualifiedName~AccountServiceTests"`
Expected: FAIL — soit CS0117 si les constantes n'étaient pas ajoutées, sinon `GetAccountsAsync_ShouldForwardSortFeatureFlagsToRepository` échoue (MockException : le service appelle encore la version 2-args, donc `defaultSort = null` ne matche pas `It.Is<DefaultSortOptions>(...)`).

- [ ] **Step 5: Implémenter dans `AccountService.GetAccountsAsync`**

Dans `01_account/src/Account.Core/Services/AccountService.cs`, remplacer la ligne 73 :

```csharp
return await _accountRepository.GetAccountsAsync(criteria, pagination);
```

par :

```csharp
var defaultSort = new DefaultSortOptions(
    FavoriteFirst: await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.FavoriteSort),
    LastActivityFirst: await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.LastActivityFeature));

return await _accountRepository.GetAccountsAsync(criteria, pagination, defaultSort);
```

(`using Pulse.Account.Core.Constants;` et `using Pulse.Account.Core.Requests;` sont déjà présents.)

- [ ] **Step 6: Vérifier que les tests passent**

Run: `dotnet test 01_account/tests/Account.Core.Tests --filter "FullyQualifiedName~AccountServiceTests"`
Expected: PASS — les 4 cas du Theory + tous les tests existants (le setup catch-all du constructeur `IsEnabledAsync(...) => false` couvre les tests existants : ils passent `DefaultSortOptions(false, false)`, matché par `It.IsAny<DefaultSortOptions>()`).

- [ ] **Step 7: Commit**

```bash
git add 01_account/src/Account.Core/Constants/FeatureFlagKeys.cs 01_account/src/Account.Core/Services/AccountService.cs 01_account/tests/Account.Core.Tests/Services/AccountServiceTests.cs
git commit -m "feat(account): drive default portfolio sort with feature flags"
```

---

### Task 3: Defaults de configuration locale + vérification complète

**Files:**
- Modify: `01_account/src/Account.API/appsettings.json` (section `FeatureFlags:Defaults`)
- Modify: `01_account/src/Account.API/appsettings.Development.json` (section `FeatureFlags:Defaults`)

**Interfaces:**
- Consumes: clés `isLastActivityFeatureEnabled` / `isFavoriteSortEnabled` (Task 2) ; le fallback `InMemoryProvider` de `FeatureFlagExtensions` qui lit `FeatureFlags:Defaults` quand `ConfigCat:SdkKey` est vide.
- Produces: rien (fin de plan).

- [ ] **Step 1: Ajouter les defaults**

Dans `01_account/src/Account.API/appsettings.json` (indentation 4 espaces, aligner sur l'existant) :

```json
"FeatureFlags": {
    "Defaults": {
        "isProspectExperienceEnabled": false,
        "isLastActivityFeatureEnabled": false,
        "isFavoriteSortEnabled": false
    }
},
```

Dans `01_account/src/Account.API/appsettings.Development.json` (indentation 2 espaces — `true` pour que le dev local garde le comportement actuel) :

```json
"FeatureFlags": {
  "Defaults": {
    "isProspectExperienceEnabled": false,
    "isLastActivityFeatureEnabled": true,
    "isFavoriteSortEnabled": true
  }
},
```

Ne pas changer la valeur existante de `isProspectExperienceEnabled`.

- [ ] **Step 2: Build + suite de tests complète**

Run: `dotnet build 01_account/Pulse.Back.Account.sln`
Expected: Build succeeded, 0 erreur.

Run: `dotnet test 01_account/Pulse.Back.Account.sln`
Expected: PASS — tous projets (`Account.Api.Tests`, `Account.Core.Tests`, `Account.Infrastructure.Tests`), 0 échec.

- [ ] **Step 3: Commit**

```bash
git add 01_account/src/Account.API/appsettings.json 01_account/src/Account.API/appsettings.Development.json
git commit -m "chore(config): add default sort feature flag defaults"
```

---

## Hors code (rappel — actions manuelles)

1. Créer le flag `isFavoriteSortEnabled` dans ConfigCat (prévenir le collègue front).
2. Vérifier que `ConfigCat:SdkKey` (back) et `VITE_CONFIGCAT_SDK_KEY` (front) pointent sur la même config ConfigCat.
