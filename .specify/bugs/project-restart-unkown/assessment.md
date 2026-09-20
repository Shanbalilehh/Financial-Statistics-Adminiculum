# Bug Assessment: `project` Service in `compose.yaml` Restarts Indefinitely with Unknown Reason

- **Slug**: project-restart-unkown
- **Created**: 2026-09-19T19:08:00-05:00
- **Source**: pasted text
- **Verdict**: valid
- **Severity**: high

## Report (verbatim or summarized)

> "The 'project defined in compose.yaml keep restarting indefenitely, the restart reason is unknown, there are not errors showing in the p"

## Symptom

When executing `docker compose up`, the `project` service continuously restarts in an infinite loop without displaying error logs or reasons in `docker ps` / `docker compose logs project`. In `docker ps`, the container status repeatedly reads `Restarting (0) X seconds ago` with exit code 0.

## Reproduction

1. Run the `project` service using Docker Compose:
   ```bash
   docker compose up --build project
   ```
2. Observe the container status with `docker ps`:
   ```bash
   STATUS: Restarting (0) X seconds ago
   ```
3. Check container logs:
   ```bash
   docker compose logs project
   ```
   No error messages or stack traces are visible; the process outputs its environment and terminates cleanly with exit code 0. Docker's `restart: always` policy restarts the container indefinitely.

## Suspected Code Paths

- [FinancialStatisticsAdminiculum.Api/Program.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Api/Program.cs#L30-L189) —
  - `Log.Logger` was never configured before `try { ... }`. When an unhandled exception occurred, `catch (Exception ex) { Log.Fatal(ex, ...); }` called the uninitialized silent logger, discarding the error details.
  - The `catch` block did not set `Environment.ExitCode = 1` or rethrow, so the process exited with code 0.
  - Line 58 required `Paths:modelPath` even though the model was moved to `FunctionGemma.Api`.
  - Line 115 registered `OrchestratorService` without registering `IMessagePublisher` in DI.
- [FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj#L11-L12) —
  - Referenced `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` at version `9.0.0` inside a `net8.0` project, causing `FileLoadException` on `Microsoft.Extensions.DependencyInjection.Abstractions 9.0.0.0` at runtime on `aspnet:8.0`.
- [FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj#L21-L23) —
  - Referenced `Microsoft.Extensions.Configuration*` version `9.0.0` in `net8.0`.
- [FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs#L9) —
  - Lacked `[DbContext(typeof(AppDbContext))]` and `[Migration("20260913000000_AddWorkspaces")]` attributes.
- [FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs) & [AnalysisJobConfiguration.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/AnalysisJobConfiguration.cs) —
  - Mapped entity collections with explicit `Id` and `Dictionary` properties via `OwnsMany(...).ToJson()`, violating EF Core 8 JSON collection constraints.
- [compose.yaml](file:///home/chi/repos/Financial-Statistics-Adminiculum/compose.yaml#L26) —
  - Configured with `restart: always`, which causes Docker to restart the container indefinitely whenever it exits.

## Root Cause Hypothesis

The restart reason was unknown and no errors appeared because:
1. Serilog's static `Log.Logger` was never initialized with a console sink prior to application startup; `builder.Host.UseSerilog` only configures DI-managed logging, not the static `Log` class used in `catch (Exception ex) { Log.Fatal(ex, ...); }`.
2. The `catch` block swallowed the exception and exited with code 0 instead of propagating a failure exit code (`Environment.ExitCode = 1`).
3. Underneath the swallowed exception, multiple startup failures were occurring simultaneously: .NET 9 assembly load conflicts in a .NET 8 runtime, missing `IMessagePublisher` in DI, EF Core 8 JSON collection model validation errors, and missing `Paths:modelPath` configuration.

Confidence: high.

## Proposed Remediation

**Preferred**:
1. **Initialize Serilog Bootstrap Logger**:
   Add `Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();` at the beginning of `Main`, and set `Environment.ExitCode = 1` inside `catch (Exception ex)` so errors are immediately written to stdout and failures produce non-zero exit codes.
2. **Align NuGet Packages to .NET 8**:
   Downgrade `Microsoft.Extensions.*` packages from `9.0.0` to `8.0.0` in `Application.csproj` and `Infrastructure.csproj`.
3. **Register `IMessagePublisher`**:
   Add `builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();` in `Program.cs`.
4. **Remove Unused Configuration**:
   Remove `Paths:modelPath` check from `Program.cs`.
5. **Fix EF Core Mapping and Migrations**:
   Decorate `20260913000000_AddWorkspaces.cs` with migration attributes, and configure `jsonb` collections using `HasConversion(...)` in `WorkspaceConfiguration` and `AnalysisJobConfiguration`.

**Files likely to change**:
- `FinancialStatisticsAdminiculum.Api/Program.cs`
- `FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj`
- `FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj`
- `FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs`
- `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs`
- `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/AnalysisJobConfiguration.cs`

**Tests to add or update**:
- `FinancialStatisticsAdminiculum.Api.Tests/Infrastructure/ApiStartupTests.cs` to verify DI container resolution and EF Core model initialization.

## Risks & Considerations

- Standardizing on .NET 8 packages ensures compatibility across the container runtime.
- Ensure RabbitMQ and PostgreSQL containers are reachable via Docker Compose service names.

## Open Questions

- None.
