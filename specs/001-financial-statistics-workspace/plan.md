# Implementation Plan: Financial Statistics Workspace

**Branch**: `001-financial-statistics-workspace` | **Date**: 2026-09-13 | **Spec**: [`spec.md`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/spec.md)

**Input**: Feature specification from [`specs/001-financial-statistics-workspace/spec.md`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/spec.md)

---

## Summary

Build an unguided, high-performance financial statistics workspace application for experimenting with financial concepts from first principles. The application combines an interactive React-based node canvas (`@xyflow/react`) featuring fine-grained atomic computational entities, an omnipresent natural language command palette powered by containerized AI models (e.g., FunctionGemma running in standalone containers launched via `docker run`), real-time observability into statistical distributions and formula mechanics using React visx pure-SVG graphics (`@visx/*`), and extreme multi-format export capabilities (lossless vector SVG, high-DPI PNG, publication-grade PDF dossiers, CSV, and portable workspace manifests).

---

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 (Backend), TypeScript 5.4+ / Node.js 20+ (Frontend)

**Primary Dependencies**: 
- *Backend*: ASP.NET Core 8.0, Entity Framework Core 8 (`Npgsql.EntityFrameworkCore.PostgreSQL`), RabbitMQ.Client, Serilog, OpenTelemetry (Traces + Metrics), Castle DynamicProxy.
- *Frontend*: React 18+, Vite, Tailwind CSS, shadcn/ui (Radix primitives), Zod (schema validation), TanStack Query (server state & caching), Lucide React (icons), Zustand (fine-grained reactive store), `@xyflow/react` (React Flow), `@visx/*` (Airbnb visx for React SVG graphics: `@visx/shape`, `@visx/scale`, `@visx/curve`, `@visx/gradient`, `@visx/axis`, `@visx/grid`, `@visx/responsive`, `@visx/tooltip`), `html-to-image`, `jspdf`, `jspdf-autotable`.
- *AI Models*: Containerized inference microservices deployed via `docker run` (packaging FunctionGemma with ONNX Runtime GenAI, tokenizer, and weights, exposing HTTP/REST `/invocations` and `/health` endpoints).

**Visual Design**: Open to customization. The UI layer uses Tailwind CSS utility classes and unopinionated shadcn/ui component primitives styled via CSS variables (theme tokens for colors, borders, typography, and spacing). Visual styling is intentionally decoupled from layout and mathematical logic, allowing rapid theming and customization without rewriting components. React visx renders native SVG elements, allowing charts, curves, and sparklines to directly consume Tailwind theme tokens (`stroke-primary`, `fill-primary/20`, etc.).

**Storage**: PostgreSQL 16 (relational tables for assets/time-series, JSONB for workspace topology snapshots and audit logs), Browser LocalStorage/IndexedDB for local workspace caching and uncommitted drafts.

**Testing**: 
- *Backend*: xUnit, FluentAssertions, Moq, Testcontainers for PostgreSQL / RabbitMQ.
- *Frontend*: Vitest, React Testing Library, jsdom.

**Target Platform**: Modern Desktop Web Browsers (Chrome, Firefox, Safari, Edge) on Linux, macOS, and Windows; Linux containerized backend and models via `docker run` / Docker Compose.

**Project Type**: Multi-tier Web Application (React SPA frontend + ASP.NET Core Web API backend + Containerized AI model services + PostgreSQL + RabbitMQ).

**Performance Goals**:
- $<200$ ms reactive recalculation across connected atomic entities on parameter change (up to 10,000 observations).
- $<2.0$ seconds roundtrip for natural language command parsing, dynamic tool resolution, and canvas mutation.
- $<1.0$ second instant generation of high-resolution vector/raster graphics (lossless vector SVG / PNG at 300+ DPI via visx SVG DOM).
- $<3.0$ seconds compilation and delivery of publication-grade multi-page PDF dossiers.

**Constraints**:
- Strictly unguided UX: zero onboarding tutorials, modal wizards, or forced linear setup sequences.
- Everything immediately observable: compact live sparklines on nodes and deep statistical inspectors (KDE, CDF, quantiles, formulas) built with React visx.
- High financial precision: calculations enforce deterministic numeric representations (`decimal`).
- 100% reversible operations: single-click undo for all NLP-driven structural modifications.
- Model Containerization: AI models MUST be containerized independently and runnable via `docker run`, isolating model runtimes and weights from the core .NET application lifecycle.

**Scale/Scope**: Support 50+ interconnected atomic entities per canvas with 10,000+ observations per time-series stream.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Constitutional Principle | Status | Evaluation & Compliance Notes |
| :--- | :--- | :--- |
| **I. Layered Architecture & Dependency Inversion** | **PASS** | `Core` maintains pure domain entities (`Asset`, `PricePoint`, `TimeSeries`, `Workspace`, `AtomicEntity`) with zero external dependencies. `Application` defines orchestration and dynamic tools. `Api` and `Web` interact purely through typed REST contracts (`workspace-api.yaml`). |
| **II. Schema-First AI Tool Contracts & Guardrails** | **PASS** | All dynamic NLP tools implement `IGemmaTool` with explicit JSON schemas validated via `IAiSchemaAggregator`. Tool mutations are strictly validated via Zod schemas on the frontend and schema contracts on the backend before mutating canvas state. |
| **III. Comprehensive Observability & Telemetry** | **PASS** | Serilog structured logging and OpenTelemetry tracing configured across API, database queries, RabbitMQ messaging, and AI inferences. No unstructured console logging. |
| **IV. Asynchronous Resiliency & Message Idempotency**| **PASS** | Inter-service events use versioned contracts in `Shared.Contracts`. Message consumers implement idempotent handling. Long-running export or simulation jobs track discrete states. |
| **V. Test-First Quality & Financial Determinism** | **PASS** | TDD mandatory for statistical calculation routines (moving averages, rolling volatility, quantiles) using high-precision numeric types (`decimal`). Integration tests validate EF Core persistence. |

*Gate Result*: **PASSED**. No constitutional violations. No exemptions required.

---

## Project Structure

### Documentation (this feature)

```text
specs/001-financial-statistics-workspace/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (architectural decisions & trade-offs)
├── data-model.md        # Phase 1 output (entities, schemas, relationships)
├── quickstart.md        # Phase 1 output (end-to-end validation scenarios)
├── contracts/           # Phase 1 output (API specifications & JSON schemas)
│   ├── workspace-api.yaml
│   ├── nlp-tools.schema.json
│   └── workspace-manifest.schema.json
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (/speckit-tasks command - to be created)
```

### Source Code (repository root)

```text
# Backend Projects (.NET 8)
FinancialStatisticsAdminiculum.Api/
├── Controllers/
│   ├── AiAnalysisController.cs      # AI & NLP command endpoints
│   ├── WorkspacesController.cs      # Workspace CRUD & manifest import/export
│   └── MarketDataController.cs      # Asset listings & historical time-series
├── DTOs/                            # Request/response contracts
├── Middleware/                      # Global exception handling & tracing
└── Program.cs                       # DI composition root & middleware pipeline

FinancialStatisticsAdminiculum.Application/
├── AI/
│   ├── Tools/                       # IGemmaTool implementations (SMA, Volatility, etc.)
│   ├── SchemaAggregators/           # Dynamic JSON schema aggregation
│   └── Services/                    # OrchestratorService & prompt processing
├── Interfaces/                      # Application contracts & service interfaces
└── Services/                        # Statistical calculation & workspace services

FinancialStatisticsAdminiculum.Core/
├── Entities/                        # Workspace, AtomicEntity, Asset, TimeSeries
├── Interfaces/                      # Repository and UnitOfWork abstractions
└── Exceptions/                      # Domain exception definitions

FinancialStatisticsAdminiculum.Infrastructure/
├── AppDbContext.cs                  # EF Core database context
├── Persistence/                     # Seeding & configuration
├── Repositories/                    # Generic repository implementations
└── Messaging/                       # RabbitMQ consumers & publishers

# Frontend Application (React 18/19 + Vite + Tailwind + shadcn/ui)
FinancialStatisticsAdminiculum.Web/
├── index.html
├── package.json
├── vite.config.ts
├── tailwind.config.js
├── postcss.config.js
├── components.json                  # shadcn/ui configuration
├── src/
│   ├── components/
│   │   ├── ui/                      # shadcn/ui primitives (button, dialog, input, command, sheet, badge, etc.)
│   │   ├── canvas/                  # React Flow canvas, grid, controls, minimap (@xyflow/react)
│   │   ├── nodes/                   # Custom atomic entity nodes (Price, MA, Vol, etc.) with Lucide icons
│   │   ├── nlp/                     # Omnipresent NLP command bar & audit badge (shadcn Command component)
│   │   ├── inspector/               # Statistics View drawer / Sheet (KDE plots, CDF, formulas)
│   │   └── export/                  # Multi-format export dialogs (PDF, PNG, CSV)
│   ├── kernel/                      # High-precision client-side statistical engine
│   ├── hooks/                       # Custom React hooks & TanStack Query wrappers
│   ├── queries/                     # TanStack Query definitions for API endpoints
│   ├── schemas/                     # Zod schemas for validation (manifests, entity params, CSV uploads)
│   ├── services/                    # API client (Axios/fetch), SSE stream handler, export generator
│   ├── store/                       # Zustand workspace state & undo/redo history
│   └── types/                       # TypeScript interfaces inferred from Zod schemas & contracts
└── tests/                           # Vitest component & kernel unit tests

# Cross-Service & Supporting Projects
FunctionGemma.Api/                   # Dedicated ONNX Runtime GenAI inference service (containerized via docker run)
Shared/                              # Shared.Contracts & Shared.Entities
compose.yaml                         # Multi-service container orchestration
```

**Structure Decision**: A modern decoupled web architecture pairing the existing ASP.NET Core Clean Architecture backend with a React TypeScript application (`FinancialStatisticsAdminiculum.Web`). The frontend leverages Vite, Tailwind CSS, shadcn/ui, Zod, TanStack Query, Lucide React, Zustand, and React visx (`@visx/*`) to deliver high-performance reactivity, contract-first runtime validation, clean iconography, pure-SVG vector graphics, and open customizable styling. AI models are containerized independently and executed via `docker run`.

---

## Complexity Tracking

*No constitutional violations identified. Clean architecture boundaries maintained.*

| Potential Concern | Resolution | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| Client-Side Statistical Kernel | Implemented in pure TypeScript utility functions | Sending all slider tweaks to the server violates the 200ms interactive threshold (SC-001). |
| React Flow Canvas | Leverages `@xyflow/react` | Building custom canvas nodes, zoom/pan math, and bezier connection lines from scratch introduces massive accidental complexity. |
| React visx for Graphics | Pure SVG React primitives (`@visx/shape`, `@visx/scale`, `@visx/curve`, `@visx/grid`, `@visx/axis`) | HTML5 Canvas-based libraries (e.g. Chart.js) render raster bitmaps, preventing lossless vector SVG exports and blocking direct styling via Tailwind CSS variables and theme tokens. |
| Model Containerization via `docker run` | Self-contained model containers packaging weights, ONNX Runtime GenAI, and REST/gRPC endpoints | Embedding model weights and Python/C++ ONNX dependencies inside the .NET application container bloats build times, couples inference hardware dependencies, and complicates independent model versioning and scaling. |
| Hybrid Export Strategy | Client-side PNG/SVG/CSV + Server/Client PDF | Browser `window.print()` cannot produce standalone publication-grade assets or vector diagrams. |
