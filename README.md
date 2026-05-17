# Bunnings — Sizzling Hot Products

A small .NET 10 service that ranks products by net sales for a given window and exposes two HTTP endpoints over a Postgres-backed dataset seeded from JSON. Built as a code-challenge submission.

The interesting bits live in:

- `src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Domain/` — pure domain types (no framework deps).
- `src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Application/` — query/handler pipeline (`OrderReducer` → `ProductSaleCounter` → `TopProductSelector`) and FluentValidation validators.
- `src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Infrastructure/` — EF Core `DbContext`, persistence row DTOs, repository implementations, and the JSON seeder.
- `src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Api/` — ASP.NET Core 10 host, controllers, `Program.cs`.
- `src/BunningsSizzlingHotProducts/tests/` — xUnit projects for Domain, Application, and Api integration tests.
- `inputs/` — the dataset (`orders.json`, `products.json`) provided by the challenge.

## Prerequisites

| Tool | Minimum | Notes |
|---|---|---|
| .NET SDK | 10.0 | `dotnet --list-sdks` to confirm |
| Docker Desktop (or compatible) | 20.10+ | Only needed for the Postgres container and the Compose path |
| EF Core tools | 10.0.8 | `dotnet tool update --global dotnet-ef --version 10.0.8` |

No frontend is included in this submission; the API is intended to be exercised directly (curl, Postman, the OpenAPI document, or integration tests).

## Running the app

You have two paths. Pick whichever fits.

### A. Docker Compose (recommended for graders)

Brings up Postgres and the API in one shot. The API auto-runs EF migrations and seeds the database on startup from the JSON files mounted at `/app/inputs`.

```bash
# from the repo root
docker compose up --build
```

> **Note — exposing the API to your host.** The `backend` service uses `expose: ["8080"]`, which makes port 8080 reachable only from other containers on the Compose network. To call the API from your machine, add a host-port publish to `docker-compose.yml`:
>
> ```yaml
>   backend:
>     ...
>     ports:
>       - "8080:8080"
> ```
>
> Then `curl http://localhost:8080/api/top-product/daily?date=2026-04-21`.

Shut down with `docker compose down` (use `docker compose down -v` if you want to wipe the Postgres volume between runs).

### B. Local dev (Postgres in Docker, API on host)

Useful for fast iteration with breakpoints in your IDE.

```bash
# 1. start just the database
docker compose up -d db

# 2. point the API at the host-local Postgres + the repo-root inputs folder
# (PowerShell)
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=sizzling;Username=postgres;Password=postgres"
$env:Seeding__InputsPath          = "$PWD\inputs"

# 3. run the API
dotnet run --project src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Api
```

Kestrel prints the listening URL on startup (`Now listening on: http://localhost:5xxx`). The OpenAPI document is at `/openapi/v1.json`.

If migrations have not yet been applied (or you want a fresh schema), the seeder takes care of `MigrateAsync` on startup. To regenerate migrations from a clean state during development:

```bash
dotnet ef migrations add InitialCreate \
  --project src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Infrastructure \
  --startup-project src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Api \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Infrastructure \
  --startup-project src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Api
```

## API

Two endpoints, both `GET`, returning `200 OK` with `application/json` or `400 Bad Request` (RFC 7807 `ProblemDetails`) on validation failure.

### `GET /api/top-product/daily`

Top-selling product for a single date.

| Query parameter | Type | Notes |
|---|---|---|
| `date` | `DateOnly` (`yyyy-MM-dd`) | Required. Must be a valid past or present date. |

```bash
curl "http://localhost:8080/api/top-product/daily?date=2026-04-21"
```

```json
{ "from": "2026-04-21", "to": "2026-04-21", "productName": "Aandleford Black Seaford Post Mounted Letterbox" }
```

### `GET /api/top-product/rolling`

Top-selling product over a rolling N-day window ending today.

| Query parameter | Type | Default | Notes |
|---|---|---|---|
| `days` | `int` | `3` | Validated `> 0`. |

```bash
curl "http://localhost:8080/api/top-product/rolling?days=7"
```

```json
{ "from": "2026-05-10", "to": "2026-05-17", "productName": "Aandleford Black Seaford Post Mounted Letterbox" }
```

## Testing

### Run all tests

```bash
dotnet test src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.slnx
```

Three test projects run:

- **`BunningsSizzlingHotProducts.Domain.Tests`** — pure domain logic. No I/O.
- **`BunningsSizzlingHotProducts.Application.Tests`** — handlers + pipeline + validators against in-memory fakes of the repositories.
- **`BunningsSizzlingHotProducts.Api.IntegrationTests`** — full HTTP round-trip via `WebApplicationFactory`. Uses the `Testing` environment, which short-circuits the JSON seeder; integration tests seed their own data through the EF context.

### Code coverage report

The CI workflow produces a coverage report automatically (see below). To generate one locally:

```bash
dotnet test src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.slnx \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# install once
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator \
  -reports:"./TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./TestResults/CoverageReport" \
  -reporttypes:"Html;TextSummary"

# open ./TestResults/CoverageReport/index.html
```

## CI

`.github/workflows/ci.yml` runs on every pull request and push to `main`:

1. Restore + build the backend solution in Release.
2. Run all tests with the `XPlat Code Coverage` collector.
3. Generate an HTML + markdown coverage report via ReportGenerator.
4. Append the markdown summary to `$GITHUB_STEP_SUMMARY` so it renders inline on the workflow run page.
5. Upload the .trx test results and the HTML coverage report as workflow artifacts.

A failing test fails the build. CI does not currently enforce a coverage threshold.

## Input handling — important note for graders

The `inputs/*.json` files supplied with this challenge contain stray `CRLF` sequences inserted **inside** JSON string literals (a line-wrap artifact of the dataset, not the structure between tokens). Strict JSON does not permit unescaped control characters inside strings, so `System.Text.Json` rejects the files as-supplied.

Rather than pre-process the inputs (which would silently break if you supply replacement files with the same characteristic), the seeder reads them through `SeedJsonReader`, which:

1. Attempts a strict parse first.
2. On a specific `JsonException` ("invalid within a JSON string"), strips stray `\r\n` / `\r` and retries once.

**Assumption:** any CR/LF that appears inside a string value is a wrap artifact, not legitimate data — true for this dataset (product names, IDs, dates). If you supply replacement input files where a string value *must* contain a literal newline, escape it as `\n` per RFC 8259 — strict parsing will succeed first and the tolerant fallback will never run.

The tolerant fallback is narrowly scoped (`when` clause on the catch) so unrelated JSON errors still surface as themselves and don't get silently masked. See `tests/BunningsSizzlingHotProducts.Infrastructure.Tests/Seeding/SeedJsonReaderTests.cs` for both the strict-parse and dirty-input cases.

## Design notes

- **CQRS-flavoured pipeline.** Each query is handled by a single `*Handler` that fetches data via repository interfaces, runs a synchronous functional pipeline (`OrderReducer` → `ProductSaleCounter` → `TopProductSelector`), and returns a strongly-typed `*Result`. Handlers do not contain business logic; they orchestrate.
- **Persistence DTOs (`*Row`) are separate from domain types.** EF Core maps to flat row types in `Infrastructure/Persistence/Models/`; the repository assembles domain `Order` / `Product` aggregates. This keeps the domain layer free of `Microsoft.EntityFrameworkCore` references.
- **Cancellations are netted out.** Cancellation rows carry an `OriginalOrderDate` so the reducer can subtract them from the correct day's completed sales. The seeder populates this by cross-referencing completed and cancelled rows for the same `OrderId`.
- **Validation lives at the API edge** via FluentValidation. Validators are auto-registered by `AddValidatorsFromAssemblyContaining<...>`, and failures are translated to RFC 7807 `ProblemDetails` responses.
- **Tie-breaking is deterministic.** When two products have the same total, the selector falls back to alphabetical (`StringComparer.Ordinal`) so results are reproducible across machines regardless of locale.

## Known limitations / future work

- No authentication or rate-limiting — the API is open by design for the challenge.
- The rolling window is anchored to "today" (the host's clock via `IClock`). For deterministic responses across environments, inject a fixed clock or expose `from` / `to` query params explicitly.
- Single-instance only. Startup seeding has no advisory lock, so running multiple replicas against a fresh DB would race.
- No pagination or alternative result shapes; the brief asks for a single top product per window.
