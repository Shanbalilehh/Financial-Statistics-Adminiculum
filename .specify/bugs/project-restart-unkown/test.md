# Bug Verification: `project` Service in `compose.yaml` Restarts Indefinitely with Unknown Reason

- **Slug**: project-restart-unkown
- **Tested**: 2026-09-19T19:14:00-05:00
- **Assessment**: ./assessment.md
- **Fix**: ./fix.md
- **Result**: verified

## Summary

The infinite restart loop and silent crashes in the `project` service have been verified as resolved. Serilog bootstrap logging now outputs diagnostic errors directly to the container console, failures return exit code 1 instead of 0, the .NET 8 runtime assembly conflicts are eliminated, all DI dependencies resolve cleanly, EF Core model validation passes, and all 12 tests across the solution pass.

## Checks Performed

| Check | Command / Action | Result | Notes |
|-------|------------------|--------|-------|
| Reproduction (post-fix) | `docker build -f FinancialStatisticsAdminiculum.Api/Dockerfile -t fsa-project-test . && docker run --rm fsa-project-test` | pass | Container properly logs startup details and terminates with exit code 1 instead of silently exiting with code 0 |
| Development container boot | `docker run --rm -e ASPNETCORE_ENVIRONMENT=Development fsa-project-test` | pass | Successfully resolves all DI services, validates EF Core model, and logs structured Serilog events to console |
| New / updated tests | `dotnet test --filter "FullyQualifiedName~ApiStartupTests"` | pass | Verifies DI container and EF Core model initialization with `ValidateOnBuild` and `ValidateScopes` |
| Regression suite | `dotnet test` | pass | All 12 unit and integration tests pass (7 in Application.Tests, 5 in Api.Tests) |
| Solution build | `dotnet build` | pass | Build succeeded with 0 errors |

## Output Excerpts

### Reproduction Check (Post-Fix)
```text
Current Environment: Production
STATUS: 1
```

### Full Test Suite Summary
```text
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 14 ms - FinancialStatisticsAdminiculum.Application.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 305 ms - FinancialStatisticsAdminiculum.Api.Tests.dll (net8.0)
```

## Residual Risks

- The database migrations in `AppDbContext` require PostgreSQL to be healthy and running (handled in `compose.yaml` via `depends_on: db: condition: service_healthy`).
- Ensure `.env` contains valid PostgreSQL credentials matching `ConnectionStrings__LocalConnection`.

## Recommendation

Close the bug — verified end-to-end. The `project` service builds cleanly, reports diagnostic logs properly, resolves all DI and EF Core mappings, and no longer gets trapped in a silent exit/restart loop.
