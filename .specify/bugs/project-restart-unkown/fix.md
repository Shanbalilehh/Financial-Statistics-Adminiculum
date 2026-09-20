# Bug Fix: `project` Service in `compose.yaml` Restarts Indefinitely with Unknown Reason

- **Slug**: project-restart-unkown
- **Fixed**: 2026-09-19T19:10:00-05:00
- **Assessment**: ./assessment.md
- **Status**: applied

## Summary

Resolved the infinite restart loop and silent crashes in the `project` container (`FinancialStatisticsAdminiculum.Api`) by initializing Serilog bootstrap logging to console, returning exit code 1 on fatal exceptions, downgrading mismatched .NET 9 packages to .NET 8, registering `IMessagePublisher` in DI, removing unused required `Paths:modelPath` configuration, adding missing migration attributes, and fixing EF Core JSON mapping in `WorkspaceConfiguration` and `AnalysisJobConfiguration`.

## Changes

| File | Change | Notes |
|------|--------|-------|
| `FinancialStatisticsAdminiculum.Api/Program.cs` | modified | Added Serilog bootstrap logger, registered `IMessagePublisher`, removed unused `modelPath` check, and set `Environment.ExitCode = 1` on catch |
| `FinancialStatisticsAdminiculum.Application/FinancialStatisticsAdminiculum.Application.csproj` | modified | Downgraded `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` from `9.0.0` to `8.0.0` |
| `FinancialStatisticsAdminiculum.Infrastructure/FinancialStatisticsAdminiculum.Infrastructure.csproj` | modified | Downgraded `Microsoft.Extensions.Configuration*` from `9.0.0` to `8.0.0` |
| `FinancialStatisticsAdminiculum.Infrastructure/Migrations/20260913000000_AddWorkspaces.cs` | modified | Added `[DbContext(typeof(AppDbContext))]` and `[Migration("20260913000000_AddWorkspaces")]` attributes |
| `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs` | modified | Switched `Viewport`, `Entities`, and `Connections` to `jsonb` property conversions to prevent EF Core JSON collection ordinal key conflict |
| `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/AnalysisJobConfiguration.cs` | modified | Switched `History` to `jsonb` property conversion to prevent EF Core navigation mapping error on `ChatMessage.ToolCalls` |
| `FinancialStatisticsAdminiculum.Api.Tests/Infrastructure/ApiStartupTests.cs` | added | Integration test verifying all required services and dependencies resolve without DI or EF Core model validation errors |

## Diff Highlights

### `FinancialStatisticsAdminiculum.Api/Program.cs`
```csharp
         public static async Task Main(string[] args)
         {
+            Log.Logger = new LoggerConfiguration()
+                .WriteTo.Console()
+                .CreateBootstrapLogger();
+
             try
             {
-                string modelPath = builder.Configuration.GetValue<string>("Paths:modelPath") ?? throw new InvalidOperationException(...);
+                // Register RabbitMQ Message Publisher and Consumer
+                builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();
                 builder.Services.AddHostedService<RabbitMQMessageConsumer>();
                 ...
             }
             catch (Exception ex)
             {
                 Log.Fatal(ex, "Application terminated unexpectedly");
+                Environment.ExitCode = 1;
             }
         }
```

### `FinancialStatisticsAdminiculum.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs`
```csharp
-            builder.OwnsMany(w => w.Entities, e => { e.ToJson(); ... });
+            builder.Property(w => w.Entities)
+                .HasField("_entities")
+                .UsePropertyAccessMode(PropertyAccessMode.Field)
+                .HasColumnType("jsonb")
+                .HasConversion(
+                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
+                    v => JsonSerializer.Deserialize<List<AtomicEntity>>(v, (JsonSerializerOptions)null!) ?? new List<AtomicEntity>());
```

## Tests Added or Updated

- `FinancialStatisticsAdminiculum.Api.Tests/Infrastructure/ApiStartupTests.cs::DependencyInjection_ShouldResolveRequiredServices_IncludingMessagePublisher` — Verifies that `AppDbContext` model building succeeds, and all required services including `IOrchestratorService` and `IMessagePublisher` are resolvable in the DI container with `ValidateOnBuild` and `ValidateScopes` enabled.

## Local Verification

- Commands run:
  - `dotnet test` → All 12 unit and startup tests passed across `FinancialStatisticsAdminiculum.Application.Tests` and `FinancialStatisticsAdminiculum.Api.Tests`.
  - `docker build -f FinancialStatisticsAdminiculum.Api/Dockerfile -t test-project-fixed .` → Image built successfully.
  - `docker run --rm test-project-fixed:latest` → Outputs error cleanly to console with exit code `1` (no longer silently swallows exceptions or exits with 0).
  - `docker run --rm -e ASPNETCORE_ENVIRONMENT=Development test-project-fixed:latest` → Validates DI and EF Core model successfully, logs full diagnostic details to console, reaches database connection step.

## Deviations from Assessment

- During verification of the EF Core model initialization, EF Core 8 threw:
  1. `Entity type 'AtomicEntity' is part of a collection mapped to JSON and has its ordinal key defined explicitly` on `WorkspaceConfiguration`.
  2. `Unable to determine the relationship represented by navigation 'ChatMessage.ToolCalls'` on `AnalysisJobConfiguration`.
  Because `Workspace` and `AnalysisJob` store their JSON collections in PostgreSQL `jsonb` columns, `WorkspaceConfiguration` and `AnalysisJobConfiguration` were updated to use `HasConversion(...)` for JSON serialization instead of `OwnsMany(...).ToJson()`. This avoids EF Core's limitations on owned entity JSON collections having `Id` and `Dictionary<string, object>` properties.

## Follow-ups

- Run `/speckit-bug-test slug=project-restart-unkown` to perform end-to-end verification of the bug resolution.
