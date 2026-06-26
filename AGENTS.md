# Pulse Backend — Development Conventions

## Project

- Stack: .NET 8, C# 13, EF Core 8, ASP.NET Core, Azure Service Bus
- Architecture: Clean Architecture (Domain → Application → Infrastructure → Web)
- Database: SQL Server (CI_AS collation — case insensitive)
- CI/CD: Azure DevOps Pipelines
- Hosting: Azure App Services, Azure Functions


## Project Structure

```
src/
  {Name}.Domain/          # Entities, enums, domain exceptions — zero dependencies
  {Name}.Application/     # Interfaces, services, DTOs, validators, mapping
  {Name}.Infrastructure/  # Repositories, DbContext, HTTP clients, event handlers
  {Name}.WebApi/          # Controllers, Program.cs, middleware, config
tests/
  {Name}.Application.UnitTests/
  {Name}.Infrastructure.UnitTests/
  {Name}.WebApi.UnitTests/
```

| File                      | Location                              |
|---------------------------|---------------------------------------|
| Interfaces (contracts)    | `Application/Interfaces/`             |
| Services                  | `Application/Services/`               |
| DTOs (Request/Response)   | `Application/Requests/`, `Application/Responses/` |
| Validators                | `WebApi/Validators/` or `Application/Validators/` |
| Mappers                   | `Application/Mappings/`               |
| Entities                  | `Domain/Entities/`                    |
| Enums                     | `Domain/Enums/`                       |
| Repositories              | `Infrastructure/Repositories/`        |
| HTTP clients              | `Infrastructure/Clients/`             |
| Event handlers            | `Infrastructure/Handlers/`            |
| DI registration           | `Application/DependencyInjection.cs`, `Infrastructure/DependencyInjection.cs` |
| DbContext                 | `Infrastructure/Data/`                |


## Project Configuration

- `<AnalysisLevel>latest-recommended</AnalysisLevel>` in `Directory.Build.props`
- `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`
- `.editorconfig` at the repo root


## Naming

- Private fields: `_camelCase` (e.g. `private readonly IUserService _userService;`)
- Constants: `PascalCase` — never `SCREAMING_CASE`
- Async methods: `Async` suffix — except controller actions
- Booleans: `Is/Has/Can/Any` prefix
- Interfaces: `I` prefix
- No abbreviations (except Id, Xml, Ftp, Uri, Url, Http, Dto)
- No Hungarian notation (`iCounter`, `strName`) — use meaningful names
- Use C# aliases (`string`, `int`, `object`) not BCL names (`String`, `Int32`, `Object`)
- Enum names: singular (`Status` not `Statuses`), no `Enum`/`Flag` suffix — `[Flags]` enums use plural
- Method names: Verb + Resource — `CreateOrder`, `DeleteBusiness`, `GetConfiguration` — never just `Create`, `Delete`, `Get`. The parameters already indicate the lookup key — avoid redundant `ByXxx` suffixes (e.g. `GetConfiguration(int configurationId)` not `GetConfigurationById(int configurationId)`)
- Code in English only (comments and commits may be in French)

### Required Suffixes

| Type            | Suffix                   | Example                      |
|-----------------|--------------------------|------------------------------|
| Controller      | `*Controller`            | `SubscriptionController`     |
| Service         | `*Service`               | `SubscriptionService`        |
| Repository      | `*Repository`            | `SubscriptionRepository`     |
| Event Handler   | `*EventHandler`          | `AccountCreatedEventHandler` |
| Provider        | `*Provider`              | `NotificationProvider`       |
| Entity          | `*Entity`                | `SubscriptionEntity`         |
| Incoming DTO    | `*Request` or `*Command` | `CreateOrderRequest`         |
| Outgoing DTO    | `*Response` or `*Dto`    | `OrderResponse`              |


## Clean Architecture — Layer Rules

Dependency direction: Web → Application → Domain, Infrastructure → Application → Domain

### Forbidden imports

| If file is in  | NEVER use these `using`                                                                |
|----------------|----------------------------------------------------------------------------------------|
| DOMAIN         | `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, `System.Net.Http`, `FluentValidation`, `Microsoft.Graph`, `Newtonsoft.Json`, `Azure.*`, `Polly`, `MediatR` |
| APPLICATION    | `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, `IHttpContextAccessor`, `System.Net.Http`, `Microsoft.Graph`, `Azure.Messaging`, `Microsoft.Data.*` |
| INFRASTRUCTURE | `Microsoft.AspNetCore.Mvc`, `Microsoft.AspNetCore.Http` (except middleware)            |

### Forbidden instantiations

| If file is in | NEVER `new X()` where X is                         |
|---------------|-----------------------------------------------------|
| APPLICATION   | Any class from Infrastructure namespace             |
| WEB           | Any repository or Infrastructure class              |
| Any layer     | `new HttpClient()` — use `IHttpClientFactory`       |

### Forbidden direct calls

| Caller      | NEVER call                             | Instead                                   |
|-------------|----------------------------------------|-------------------------------------------|
| WEB         | `IRepository`, `DbContext`             | Call service → service calls repository   |
| APPLICATION | `HttpClient`, `GraphServiceClient`     | Call Infrastructure interface             |
| DOMAIN      | Any interface                          | Domain has zero dependencies              |

### Interfaces & DI

- Interfaces defined in Application, implemented in Infrastructure
- DI registration in `DependencyInjection.cs` (per layer), never in `Program.cs`


## Business Logic Placement

### Controller (WEB) — orchestration only
- No business logic in controllers — delegate to Application services
- NEVER return an entity directly from a controller action — always map to DTO

### Repository (INFRASTRUCTURE) — data access only
- No business logic in repositories — only data persistence

### Domain — zero dependencies
- No I/O (HTTP, DB, file, message bus), no EF Core, no logging, no framework


## Validation

- Input validation → FluentValidation `AbstractValidator<T>` on `*Request`/`*Command`


## EF Core

- Always `AsNoTracking()` for read-only queries
- Always `SaveChangesAsync()`, never `SaveChanges()`
- Never `.ToLower()` or `.ToUpper()` in LINQ queries — SQL Server CI_AS handles it
- Use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for bulk operations when no change tracking, domain events, or cascade behavior is needed
- Explicitly `.Include()` navigations — no lazy loading
- Never expose `IQueryable` outside repository — materialize with `ToListAsync()`
- `AsSplitQuery()` only when multiple sibling collection navigations are included — not based on Include count. Default to single query unless performance issue observed. Split queries do separate roundtrips with no consistency guarantee
- Prefer `AddDbContextPool` over `AddDbContext` — pooling prevents constructor injection of scoped services and custom state is not reset between uses
- `EnableRetryOnFailure()` for Azure SQL — EF Core default is 6 retries, do not hardcode a lower value without justification
- Enum properties persisted as strings: `.HasConversion<string>()` in `OnModelCreating` — `JsonStringEnumConverter` handles API serialization
- Wrap multiple `ExecuteUpdateAsync`/`ExecuteDeleteAsync` in an explicit transaction
- Never mix `SaveChangesAsync` and `ExecuteUpdateAsync`/`ExecuteDeleteAsync` on the same entities in one unit of work
- Anti-pattern `Detach + Update` — modify only the needed properties on the tracked entity instead


## HTTP Clients

- Interface defined in Application, implementation in Infrastructure
- `ResiliencePipeline` / Polly policy built once (`private static readonly`) — never inside a method body
- NEVER `Task.Delay` for propagation timing — use a readiness probe with Polly retry (exponential backoff + `ShouldHandle` on a typed exception)
- Prefer `AddStandardResilienceHandler()` as starting point for HTTP clients (combines rate limiter, timeout, retry, circuit breaker, attempt timeout)
- NEVER put `new HttpClient()` anywhere — use `IHttpClientFactory`
- Mandatory timeout on every external HTTP call
- Use idempotency keys for POST retries to external services


## Logging

- Structured templates: `_logger.LogInformation("User {UserId} created", userId);`
- NEVER string interpolation: `$"User {userId}"` — FORBIDDEN
- NEVER concatenation: `"User " + userId` — FORBIDDEN
- PascalCase placeholders: `{UserId}` not `{userid}`
- Propagate CorrelationId via `X-Correlation-Id` header
- NEVER log PII (email, phone, name, address) — log IDs only
- NEVER log secrets (tokens, passwords, connection strings)
- Use correct log levels: Warning = recoverable anomaly, Error = failure requiring attention — do not confuse
- Prefer `LoggerMessage` source-generated delegates on hot paths for zero-allocation logging


## Async

- Always `async/await`, never `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`
- Never `async void`
- Parallelize independent calls with `Task.WhenAll`
- Never `Task.Run` in the HTTP request path
- No fire-and-forget from controllers
- Do NOT use `ConfigureAwait(false)` in application code — unnecessary in ASP.NET Core. Shared libraries consumed by UI apps may use it


## API REST

- Plural resources: `/users`, `/subscriptions`
- No verbs in URIs: `/users` not `/getUsers`
- kebab-case for multi-word segments: `/booking-links`
- POST creation → `201 Created` with `CreatedAtAction()`
- Empty collection → `200 OK` with `[]`, not `404`
- Errors → `ProblemDetails` (RFC 9457)
- Mandatory pagination on GET collections (default: 20, max: 100)
- `[ProducesResponseType]` on every controller action
- JSON in camelCase, ignore nulls in serialization only (`WhenWritingNull`) — never ignore nulls in deserialization (explicit `null` in PATCH means deletion intent)
- 401 = not authenticated, 403 = not authorized
- DELETE success → `204 No Content`, not `200 OK`
- API versioning via URL path (`/api/v1/users`) — never break a published version
- Maintain at least 2 active versions during migration before retiring the old one
- GET with body is forbidden (RFC 9110)
- POST for reading data is forbidden — use GET with query parameters
- URI depth max 3 levels: `/api/v1/users/{id}/subscriptions` — no deeper nesting


## Security

- Never hardcode secrets in code or config files
- Use Managed Identity instead of connection strings with keys
- Verify ownership (BOLA): `if (entity.OwnerId != currentUserId)` → return 404 (not 403)
- HTTPS required, restrictive CORS (no `AllowAnyOrigin()` in production)
- Middleware order: UseRouting → UseCors → UseAuthentication → UseAuthorization → MapControllers
- `contactId` is ALWAYS from `CurrentUser` header (Gateway-resolved) — never from query/body
- No PII in Service Bus events
- Prefer `FromSql` / `FromSqlInterpolated` to ensure parameterization and avoid SQL injection. Use `FromSqlRaw` only when parameters are explicitly handled — NEVER with string interpolation or concatenation
- NEVER use entity as `[FromBody]` parameter — over-posting / mass assignment
- NEVER return `ex.Message` or stack trace in API response — leaks internals
- Status/type fields MUST be enums (not `string`) — persist with `.HasConversion<string>()` in EF Core, guard transitions with methods on the entity (`MarkAsCompleted()`, `ResetForRetry()`) that throw `InvalidOperationException` on invalid transitions — never assign the property directly from outside the entity
- No custom authentication — use ASP.NET Core authentication mechanisms
- `FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()` — all endpoints authenticated by default, opt-out with `[AllowAnonymous]`
- Token in query string is forbidden — always in `Authorization` header
- BFLA: `[Authorize(Roles = ...)]` on sensitive actions (Gateway-level only)
- Never `TypeNameHandling.All` or `TypeNameHandling.Auto` in JSON deserialization — type injection risk
- SSRF: whitelist allowed domains when URLs come from user input
- Path traversal: validate file paths with `Path.GetFileName()` — never use raw user input in file paths
- Security headers: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Strict-Transport-Security`
- `AddServerHeader = false` on Kestrel — do not expose server version
- Headers: whitelist which headers to log — never log all request headers (may contain tokens)
- Limit request body size (`MaxRequestBodySize`) on Kestrel/IIS
- Rate limiting (.NET 7+) on authentication and sensitive endpoints
- Debug endpoints (`/swagger`, `/health-details`) disabled or restricted in production
- `UseExceptionHandler()` in non-Development environments — never expose developer exception page


## Exceptions

- `throw;` to re-raise, never `throw ex;` (CA2200)
- Use `Pulse.ExceptionMiddleware` for centralized error handling — do NOT implement `IExceptionHandler` directly
- Use typed exceptions with HTTP code — never `throw new Exception("...")`
- No empty catch blocks — always log or re-throw
- `catch (Exception)` only at outermost boundary (BackgroundService, middleware)
- Prefer specific `catch` with `when` clause over generic `catch (Exception)` — e.g. `catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)`
- `Try*` prefixed methods MUST return `bool` (with `out` for result) or `(bool Success, T Value)` tuple for async — never throw, caller decides how to handle failure. Inner non-blocking operations (e.g. link persistence) use a nested try-catch with log warning


## DateTime

- Never `DateTime.Now` — always UTC. In services, inject `TimeProvider` for testability; elsewhere use `DateTime.UtcNow`


## Event-Driven

- Event handlers MUST be idempotent (check existence before AddAsync)
- Never `catch (Exception) { return; }` in a Service Bus handler (= data loss)
- Naming: `<Entity><Action>Event` (e.g. `AccountCreatedEvent`)
- Never remove a field from an existing event schema (breaking change)
- Distinguish domain events (in-process, same transaction) from integration events (cross-service, Service Bus)
- Dead letter queue MUST be monitored — messages in DLQ represent potential data loss
- Recommended event structure: `EventId` (Guid), `EventType`, `OccurredAt` (UTC), `CorrelationId`
- Bounded retry on consumers: configure `MaxDeliveryCount` — messages exceeding it go to DLQ
- Topic naming: kebab-case `{entity}-{action}` (e.g. `account-created`), subscription naming: `{consumer}-{topic}`


## Tests

- Naming: `MethodName_WhenCondition_ShouldExpectedResult`
- Pattern: Arrange-Act-Assert
- One test = one behavior
- Use `await` in async tests (not `.Result`)
- Centralize test data in `ModelsBuilder`
- Frameworks: xUnit, NSubstitute (preferred for new projects) or Moq, FluentAssertions
- Integration tests: `WebApplicationFactory` + Testcontainers + Respawn
- `IAsyncLifetime` for async setup/teardown in xUnit
- Deterministic tests: use `FakeTimeProvider` instead of `DateTime.UtcNow` for time-dependent logic
- Don't mock what you don't own — mock your own interfaces, not framework types (e.g. mock `IUserRepository`, not `DbContext`)


## Commits

- Convention: Angular / Conventional Commits
- Format: `<type>(<scope>): <subject>`
- Types: feat, fix, refactor, perf, test, docs, style, build, ci, chore, revert
- Scope: feature within the service (e.g. `booking-link`, `account-experts`) — never a ticket number, never the API name
- Subject in imperative present tense, no initial capital, no trailing period
- Breaking changes: `!` after type (e.g. `feat(api)!: remove legacy endpoint`) or `BREAKING CHANGE:` footer
- Body (optional) explains the "why", not the "what" — the diff shows the what


## Methods & SOLID

- Maximum 4 parameters per method — beyond that, group into request/command object
- Maximum 4 constructor dependencies — beyond that, extract facade or service
- Keep method signatures concise and readable — use request objects to reduce parameter count when line length is exceeded
- Avoid multiple boolean parameters — use explicit methods or enum when ambiguous. Simple optional flags are acceptable (`bool includeDeleted = false`)
- A service with too many dependencies or too many lines likely violates SRP — extract a dedicated class whose name describes its single responsibility
- Magic values → named constants or enums — never hardcode strings or numbers with business meaning
- Thread safety in singletons: use `ConcurrentDictionary` not `Dictionary`, `Interlocked` not `++`


## DI

- Scoped for business services and repositories
- Singleton for configuration and caches
- NEVER inject Scoped into Singleton (captive dependency)
- `ValidateScopes = true` in Development
- No `BuildServiceProvider()` in extension methods
- ISP split: register concrete class once, forward each sub-interface via `sp.GetRequiredService<TImpl>()` — all share the same scoped instance
- Use `IServiceScopeFactory` when a Singleton needs to consume Scoped services
- Keyed Services (.NET 8+) for multiple implementations of the same interface
- `IHttpContextAccessor` forbidden in Application/Domain — abstract behind `IAuthenticationContext` interface
- Use `IOptions<T>` / `IOptionsMonitor<T>` for configuration — never `Environment.GetEnvironmentVariable()`
- `ValidateOnStart()` on options registration for fail-fast configuration validation


## Performance

- `StringBuilder` for string concatenation in loops — never `+=` in a loop
- Materialize `IEnumerable` (`ToList()`) before multiple enumeration — avoid deferred execution surprises
- Avoid double-check existence patterns (SELECT then INSERT) — use a single query or upsert
- Minimize allocations on hot paths: prefer `Span<T>`, `ArrayPool`, stackalloc when appropriate
