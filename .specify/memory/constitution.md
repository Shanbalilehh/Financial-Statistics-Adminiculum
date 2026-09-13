<!--
SYNC IMPACT REPORT
==================
Version Change: Uninitialized ([CONSTITUTION_VERSION]) → 1.0.0
Ratification Date: 2026-02-14
Last Amended Date: 2026-09-12

Modified Principles:
- [PRINCIPLE_1_NAME] → I. Layered Architecture & Strict Dependency Inversion
- [PRINCIPLE_2_NAME] → II. Schema-First AI Tool Contracts & Guardrails
- [PRINCIPLE_3_NAME] → III. Comprehensive Observability & Structured Telemetry
- [PRINCIPLE_4_NAME] → IV. Asynchronous Resiliency & Message Idempotency
- [PRINCIPLE_5_NAME] → V. Test-First Quality & Financial Determinism (NON-NEGOTIABLE)

Added Sections:
- Technology Standards & Operational Constraints (replaces [SECTION_2_NAME])
- Development Workflow & Quality Gates (replaces [SECTION_3_NAME])

Removed Sections:
- None

Follow-up TODOs:
- None
-->

# Financial Statistics Adminiculum Constitution

## Core Principles

### I. Layered Architecture & Strict Dependency Inversion
The system MUST strictly adhere to Clean Architecture layering: `Core` → `Application` → `Infrastructure` / `Api`.
- `FinancialStatisticsAdminiculum.Core` MUST contain pure domain entities, value objects, domain exceptions, and abstractions with zero external dependencies on third-party frameworks, persistence libraries, or web transports.
- `FinancialStatisticsAdminiculum.Application` MUST contain business logic, use cases, orchestrations, and service interfaces; it MUST NOT reference concrete infrastructure implementations.
- `FinancialStatisticsAdminiculum.Infrastructure` and `FinancialStatisticsAdminiculum.Api` MUST depend inward on Core and Application abstractions. Dependencies MUST be resolved via dependency injection at the composition root.
- Cross-boundary communication between services MUST use explicit data transfer objects (DTOs) or shared contracts (`Shared.Contracts`).

*Rationale*: Isolating core financial domain calculations from external technologies guarantees high testability, prevents vendor lock-in, and enforces clear boundaries across services.

### II. Schema-First AI Tool Contracts & Guardrails
All AI model integrations and tool-calling execution paths MUST adhere to rigorous contract definitions and validation guardrails.
- Every executable AI tool handler MUST implement `IGemmaTool` (or its designated abstraction) and declare an unambiguous JSON schema registered via `IAiSchemaAggregator`.
- Both inputs supplied to and outputs returned from AI tool executions MUST undergo strict validation against their respective schema definitions before execution.
- Tool handlers MUST catch domain-specific execution errors gracefully and return structured failure responses rather than allowing unhandled exceptions to crash the host application.
- Direct execution of unchecked user or LLM payloads against underlying persistence stores or external systems is strictly prohibited.

*Rationale*: AI model outputs from FunctionGemma and ONNX Runtime GenAI are inherently non-deterministic. Strict schema contracts prevent prompt injection vulnerabilities, malformed parameter execution, and application state corruption.

### III. Comprehensive Observability & Structured Telemetry
Every service, HTTP endpoint, and background consumer MUST produce actionable, structured telemetry across its lifecycle.
- All application logging MUST use Serilog with structured message templates and typed properties; raw `Console.WriteLine` or string-concatenated logging is forbidden in production paths.
- Distributed tracing MUST be maintained across HTTP boundaries, Entity Framework Core queries, outgoing HttpClient calls, Npgsql connections, RabbitMQ messaging, and AI inferences using OpenTelemetry.
- Unhandled exceptions MUST be captured by centralized exception middleware (`GlobalExceptionHandler`), returning standardized ProblemDetails while preserving trace correlation IDs.
- Sensitive financial data, secrets, and credentials MUST NEVER be logged or traced in clear text.

*Rationale*: Distributed asynchronous architectures and AI-driven processing pipelines require end-to-end trace correlation to identify bottlenecks, diagnose failure points, and ensure operational auditability.

### IV. Asynchronous Resiliency & Message Idempotency
Asynchronous message processing between services MUST be resilient to transient network anomalies and duplicate message deliveries.
- Inter-service messaging (via RabbitMQ) MUST rely on explicitly versioned, backward-compatible contracts defined in `Shared.Contracts`.
- All message consumers (such as `RabbitMQMessageConsumer`) MUST be idempotent; reprocessing an identical message MUST NOT produce duplicate entity mutations, incorrect financial aggregations, or inconsistent states.
- Long-running background jobs (e.g., `AnalysisJob`) MUST track lifecycle states (`Pending`, `Processing`, `Completed`, `Failed`) with resilient error handling, retry policies, and dead-letter queue routing for poison messages.

*Rationale*: In distributed message queues, at-least-once delivery is normal; idempotency guarantees data integrity across network blips, consumer restarts, and cluster failovers.

### V. Test-First Quality & Financial Determinism (NON-NEGOTIABLE)
Financial calculations and critical business logic MUST be deterministic, exact, and validated through comprehensive automated testing.
- Test-First Development (TDD) is non-negotiable: unit tests MUST be written to specify expected behavior before implementation begins, following the Red-Green-Refactor cycle.
- All monetary values, rates, moving averages, and statistical metrics MUST use high-precision numeric types (`decimal`), never floating-point types (`float`, `double`), unless explicitly required for machine-learning tensor representations.
- Repository implementations, database queries, and transaction management (`IUnitOfWork`) MUST be validated against database integration tests.
- Pull requests that introduce statistical algorithms or AI parsing pipelines without accompanying automated test suites MUST be rejected.

*Rationale*: In financial domain software, subtle rounding inaccuracies or regressions undermine analytical credibility and carry significant real-world costs. Automated tests enforce rigor.

## Technology Standards & Operational Constraints
The project adheres to a standardized technology stack and operational guidelines across all deployment targets:
- **Platform & Runtime**: .NET 8.0 SDK / C# 12.
- **Persistence**: PostgreSQL 16 managed exclusively via Entity Framework Core migrations (`dotnet ef migrations add`). Manual unversioned SQL modifications to schemas in shared environments are prohibited.
- **Inter-Process Messaging**: RabbitMQ 3 with AMQP protocol using structured message payloads.
- **AI Inference Runtime**: Dedicated FunctionGemma service running ONNX Runtime GenAI.
- **Observability**: OpenTelemetry exported via OTLP (HTTP/Protobuf) to Seq collectors.
- **Containerization**: Multi-container Docker Compose configuration (`compose.yaml`) with explicit service health checks and isolated backend networks.

## Development Workflow & Quality Gates
All contributions MUST pass rigorous quality gates before merging into the main branch:
- **Compilation**: The solution MUST build cleanly with zero compiler warnings or errors (`dotnet build`).
- **Automated Testing**: All unit and integration test suites MUST pass (`dotnet test`) in local and continuous integration environments.
- **Database Migrations**: Any schema mutation MUST include an accompanying EF Core migration and a verified rollback path.
- **Code Reviews**: Every pull request requires architectural review verifying adherence to layering boundaries, schema validation for AI tools, and test coverage.
- **Documentation**: Changes modifying service interfaces, AI tools, or shared contracts MUST be reflected in `CLAUDE.md` and related documentation.

## Governance
This constitution supersedes all informal team practices, ad-hoc documentation, and tribal knowledge.
- **Compliance**: All team members, external contributors, and AI pair-programming agents MUST verify compliance with these principles during feature design, implementation, and code review.
- **Amendments**:
  - Amendments to the constitution require an explicit proposal detailing rationale, architectural impact, and migration steps for existing code.
  - Amendments require consensus approval from core project maintainers.
- **Versioning Policy**:
  - **MAJOR** version bump: Incompatible governance changes, principle removals, or fundamental architecture restructuring.
  - **MINOR** version bump: Addition of new principles, sections, or materially expanded governance rules.
  - **PATCH** version bump: Clarifications, wording improvements, non-semantic refinements, or typo fixes.
- **Review Cadence**: Constitutional adherence is reviewed during pull request evaluations and retrospective milestones.

**Version**: 1.0.0 | **Ratified**: 2026-02-14 | **Last Amended**: 2026-09-12
