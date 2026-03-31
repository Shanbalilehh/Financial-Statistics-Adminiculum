# MVP vs Production-Grade Architecture Review

## Scope
This review compares the current MVP architecture with production-grade expectations for:
- API design and dependency wiring
- Application and domain boundaries
- AI orchestration and tool execution
- Persistence and reliability patterns
- Observability, security, and operability

---

## What is strong in the MVP

1. **Clear layered intent (API/Application/Core/Infrastructure)**
   - The solution is split into projects that broadly follow Clean Architecture boundaries.
   - DI registration demonstrates awareness of composition root practices.

2. **Interface-first abstractions in Core/Application**
   - `INlpEngine`, `IRepository<T>`, `IUnitOfWork`, and app-service interfaces are good seams for testing and substitution.

3. **Structured logging and global exception handling**
   - Serilog is configured with JSON output and rolling file retention.
   - API-level global exception mapping exists via `IExceptionHandler`.

4. **Tool-oriented AI orchestration pattern**
   - Orchestrator + keyed handlers is a scalable idea for multi-tool expansion.

---

## Gap analysis: MVP vs production grade

### 1) Composition root and environment portability

**Current MVP**
- `Program.cs` has dense wiring and a **hardcoded model path** (`/home/chi/models/functiongemma_oga`).
- `GemmaOnnxService` is registered as singleton and owns unmanaged resources.

**Production expectation**
- Externalize model location and tool prompt settings via strongly typed options.
- Validate startup config and fail fast with actionable messages.
- Separate bootstrapping into extension methods per concern (Persistence, AI, API, Observability).

**Impact**
- Deployments will break across environments/containers.
- Harder to support blue/green or multiple model configurations.

---

### 2) DI correctness and boundary consistency

**Current MVP**
- Controller depends on concrete `OrchestratorService` instead of `IOrchestratorService`.
- `SmaToolHandler` depends on concrete `TrendAnalysisService` instead of interface.

**Production expectation**
- API layer depends only on application interfaces.
- Tool handlers depend on interfaces, enabling mocks and safe refactors.

**Impact**
- Tight coupling increases regression risk and impedes isolated testing.

---

### 3) Error policy architecture has correctness risks

**Current MVP**
- `SecurityExceptionInterceptor.DetermineRiskCommunity` maps both Orchestrator and TrendAnalysis to `NlpCommunity`.
- No `DefaultCommunity` keyed expert registration is present.
- `Environment.FailFast` can be triggered from app code path.

**Production expectation**
- Deterministic and complete risk-community mapping.
- Policy-based resilience (retry/circuit breaker/timeouts) around I/O boundaries.
- Avoid process termination from request pipeline unless strictly required and controlled.

**Impact**
- Potential misclassification of persistence faults.
- Possible runtime crash path (`DefaultCommunity` missing) and abrupt process termination.

---

### 4) Data and query quality controls

**Current MVP**
- Generic repository provides broad `GetAllAsync()` and basic predicate querying.
- `TrendAnalysisService` does not ensure ordering before SMA calculation.
- Input parsing uses `DateTime.Parse` and no invariant culture/validation.

**Production expectation**
- Domain-specific repositories or query services for critical read paths.
- Explicit ordering and limits for time-series processing.
- Strict DTO validation, date format contracts, and defensive guards.

**Impact**
- Non-deterministic analytics if DB order differs.
- Data quality and API contract fragility.

---

### 5) AI orchestration robustness and safety

**Current MVP**
- JSON extraction uses first `{` and last `}` substring strategy.
- No schema validation for tool arguments beyond deserialization.
- Prompt + tool schema are concatenated directly.

**Production expectation**
- Strict structured output contract (JSON schema validation + recoverable parsing path).
- Tool allow-list and argument guardrails (ranges, symbol checks, date windows).
- Telemetry for hallucinated/nonexistent tool names and malformed payload rates.

**Impact**
- Prompt injection and malformed output can degrade reliability.
- Harder to reason about failure modes under load.

---

### 6) Observability and SRE readiness

**Current MVP**
- Logs exist, but no health checks, metrics, traces, or readiness/liveness endpoints.
- No request correlation strategy shown.

**Production expectation**
- OpenTelemetry tracing + metrics + structured logging correlation.
- `/health/live` and `/health/ready` including DB + model checks.
- SLI/SLO dashboards and alerting.

**Impact**
- Difficult incident diagnosis and limited production confidence.

---

### 7) Security and API hardening

**Current MVP**
- No authentication/authorization policy configuration.
- No explicit rate limits, request size limits, or anti-abuse controls.
- Seeder runs at startup unconditionally.

**Production expectation**
- AuthN/AuthZ, throttling, and abuse prevention at API boundary.
- Safe startup migrations/seeding strategy controlled by environment.
- Secret management and restricted model/file access.

**Impact**
- Increased abuse risk and unpredictable startup behavior in production.

---

### 8) Delivery maturity

**Current MVP**
- No tests present in repository (unit/integration/contract/perf).
- No CI quality gates visible.

**Production expectation**
- Minimum test pyramid, contract tests for API + tool calls, and basic load tests.
- CI pipeline with build, test, static analysis, vulnerability scan, and migration checks.

**Impact**
- Changes are harder to validate safely and repeatedly.

---

## Prioritized roadmap (MVP -> Production)

### P0 (Immediate)
1. Remove hardcoded model path; move to options + environment config.
2. Fix DI boundary leaks (controller/tool handlers depend on interfaces).
3. Correct risk community mapping and register all keys actually returned.
4. Add request DTO validation and strict date parsing (`DateOnly/DateTimeOffset` strategy).
5. Enforce ordered time-series reads before indicators.

### P1 (Near term)
1. Add health checks (DB + model readiness), OpenTelemetry, and correlation IDs.
2. Introduce resiliency policies around DB and model inference boundaries.
3. Harden tool-call parsing with schema validation and explicit error taxonomy.
4. Add AuthN/AuthZ + rate limiting.

### P2 (Scale readiness)
1. Split AI inference into dedicated worker/service if latency/load grows.
2. Add caching strategy for repeated indicator windows.
3. Add CI/CD quality gates and performance baselines.

---

## Suggested target architecture (production)

- **API**: thin controllers, validation, auth, rate limits, correlation.
- **Application**: use-case services, interfaces only, orchestration policies.
- **Domain/Core**: deterministic indicator logic, domain invariants.
- **Infrastructure**: EF Core, ONNX inference adapters, telemetry exporters.
- **Cross-cutting**: resilience policies, health checks, configuration options, secrets.

---

## Overall assessment

- **MVP score (for learning/prototype): 7/10**
  - Good structural intent and promising AI-tool orchestration pattern.
- **Production readiness score: 3.5/10**
  - Main blockers are reliability hardening, operational readiness, security controls, and test coverage.

With the P0/P1 roadmap completed, this design can evolve into a robust production foundation without a full rewrite.
