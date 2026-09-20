# Bug Assessment: `project` Service in `compose.yaml` Restarts Indefinitely

- **Slug**: project-restart
- **Created**: 2026-09-19T19:07:00-05:00
- **Source**: pasted text
- **Verdict**: valid
- **Severity**: high

## Report (verbatim or summarized)

> "The 'project' defined in compose.yaml keep restarting indefenitely, the restart reason is unknown, there are not errors showing in the docker ps"

## Symptom

When launching services using `docker compose up`, the `project` container (running `FinancialStatisticsAdminiculum.Api`) repeatedly restarts indefinitely. In `docker ps`, the status constantly shows `Restarting (0) X seconds ago` with no error details or exit failure reason displayed, because the application swallowed fatal startup exceptions and exited cleanly with code 0.

## Reproduction

1. Build and run the `project` service using Docker Compose:
   ```bash
   docker compose up --build project
   ```
2. Observe container status with `docker ps` or `docker compose ps`:
   ```bash
   NAME                                     STATUS
   financial-statistics-adminiculum-project Restarting (0) X seconds ago
   ```
3. Observe that no exit error is shown in `docker ps` (exit status 0) and `docker compose logs project` only displays:
   ```text
   Current Environment: Development
   ```
   The process terminates with exit code 0 without outputting the error or stack trace, causing Docker's `restart: always` policy to restart the container indefinitely.

## Suspected Code Paths

- [FinancialStatisticsAdminiculum.Api/Program.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Api/Program.cs#L30-L189) —
  - `Log.Logger` is never initialized before `try { ... }`. When an exception is caught in `catch (Exception ex) { Log.Fatal(ex, ...); }`, Serilog's static logger is still a silent null logger, so the exception is completely swallowed and never logged to stdout/stderr.
  - The `catch` block does not rethrow or set `Environment.ExitCode = 1`, causing the process to exit with code 0 (clean exit), which Docker restarts indefinitely due to `restart: always`.
  - Line 58 enforces a mandatory `Paths:modelPath` configuration that was orphaned when Gemma was factored into `FunctionGemma.Api`.
  - Line 115 registers `OrchestratorService` via `AddProxiedScoped<IOrchestratorService, OrchestratorService, ...>`, but `IMessagePublisher` (required by `OrchestratorService`) is never registered in DI.
- [FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj#L11-L12) —
  - References `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` version `9.0.0` while targeting `net8.0` on `mcr.microsoft.com/dotnet/aspnet:8.0`, causing runtime assembly binding failure `FileLoadException: Could not load file or assembly 'Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0'`.
- [FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj#L21-L23) —
  - References `Microsoft.Extensions.Configuration*` version `9.0.0` in a `net8.0` project.
- [FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs#L9) —
  - Missing `[Migration("20260913000000_AddWorkspaces")]` and `[DbContext(typeof(AppDbContext))]` attributes, preventing EF Core from recognizing the migration during `db.Database.Migrate()`.
- [FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs) & [AnalysisJobConfiguration.cs](file:///home/chi/repos/Financial-Statistics-Adminiculum/FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/AnalysisJobConfiguration.cs) —
  - EF Core 8 `OwnsMany(...).ToJson()` fails on owned entity collections with explicit `Id` keys or dictionary properties (`AtomicEntity.Id`, `AtomicEntity.Parameters`, `ChatMessage.ToolCalls`).
- [compose.yaml](file:///home/chi/repos/Financial-Statistics-Adminiculum/compose.yaml#L26) —
  - Specifies `restart: always` on `project`, turning any exit into an infinite restart loop.

## Root Cause Hypothesis

The infinite restart loop is triggered by multiple compounding issues:
1. **Silent Crash & Swallowed Exit Code**: In `Program.cs`, Serilog's static `Log.Logger` is never configured to write to the console before `WebApplication.CreateBuilder` or inside the `catch` block (only host DI logging is configured inside `builder.Host.UseSerilog`). When an exception occurs during startup, `catch (Exception ex)` invokes `Log.Fatal(ex, "Application terminated unexpectedly")` on the uninitialized silent logger, discarding the stack trace. The `catch` block does not rethrow or set `Environment.ExitCode = 1`, causing the process to exit with status 0. In `docker ps`, no errors appear and the container simply shows `Restarting (0)` repeatedly.
2. **Assembly Version Incompatibility (.NET 9 in .NET 8)**: `FinancialStatisticsAdminiculum.Application.csproj` and `FinancialStatisticsAdminiculum.Infrastructure.csproj` reference `Microsoft.Extensions.*` version `9.0.0` inside a `net8.0` project. When executed on the `aspnet:8.0` runtime image, the CLR attempts to load `Microsoft.Extensions.DependencyInjection.Abstractions 9.0.0.0`, which conflicts with the .NET 8 shared framework, causing a fatal `FileLoadException` on startup.
3. **Missing DI Registration for `IMessagePublisher`**: `OrchestratorService` requires `IMessagePublisher` in its constructor, but `RabbitMQMessagePublisher` is never registered in `Program.cs`. When ASP.NET Core validates DI or attempts to construct `OrchestratorService`, it fails.
4. **EF Core JSON Mapping Limitations in EF Core 8**: `WorkspaceConfiguration` and `AnalysisJobConfiguration` mapped entities containing explicit `Id` and `Dictionary` properties via `OwnsMany(...).ToJson()`, which EF Core 8 rejects with model validation errors during `AppDbContext` initialization.
5. **Orphaned Required Configuration (`Paths:modelPath`)**: `Program.cs` throws an `InvalidOperationException` if `Paths:modelPath` is missing from configuration, even though `modelPath` is no longer used by `FinancialStatisticsAdminiculum.Api` after the Gemma AI engine was split into `FunctionGemma.Api`.

Confidence: high.

## Proposed Remediation

**Preferred**:
1. **Enable Proper Bootstrap Logging & Error Propagation**:
   - Initialize Serilog's static logger at the top of `Program.Main`:
     ```csharp
     Log.Logger = new LoggerConfiguration()
         .WriteTo.Console()
         .CreateBootstrapLogger();
     ```
   - In `catch (Exception ex)`, log the fatal error and set `Environment.ExitCode = 1;` so startup failures are visible in container logs and yield a non-zero exit code.
2. **Align NuGet Package Dependencies to .NET 8**:
   - In `FinancialStatisticsAdminiculum.Application.csproj`, downgrade `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` from `9.0.0` to `8.0.0`.
   - In `FinancialStatisticsAdminiculum.Infrastructure.csproj`, downgrade `Microsoft.Extensions.Configuration*` from `9.0.0` to `8.0.0`.
3. **Register `IMessagePublisher` in DI**:
   - In `Program.cs`, add:
     ```csharp
     builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();
     ```
4. **Clean up Orphaned `Paths:modelPath` Requirement**:
   - Remove the unused `Paths:modelPath` configuration check from `FinancialStatisticsAdminiculum.Api/Program.cs`.
5. **Fix Migration & EF Core JSON Configuration**:
   - Add `[DbContext(typeof(AppDbContext))]` and `[Migration("20260913000000_AddWorkspaces")]` to `20260913000000_AddWorkspaces.cs`.
   - Map `jsonb` collections in `WorkspaceConfiguration` and `AnalysisJobConfiguration` using `HasConversion(...)` to avoid EF Core 8 JSON owned entity key constraints.

**Alternatives**:
- In `compose.yaml`, change `restart: always` to `restart: on-failure`.

**Files likely to change**:
- `FinancialStatisticsAdminiculum.Api/Program.cs`
- `FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj`
- `FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj`
- `FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs`
- `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs`
- `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/AnalysisJobConfiguration.cs`

**Tests to add or update**:
- Add an API startup integration test verifying that all services can be resolved and EF Core model initialization passes.
- Verify container execution by building and running the `project` container in `compose.yaml`.

## Risks & Considerations

- Package downgrades to `8.0.0` are standard for .NET 8 compatibility.
- Ensure RabbitMQ container is reachable when `RabbitMQMessagePublisher` or `RabbitMQMessageConsumer` initializes.

## Open Questions

- None.
