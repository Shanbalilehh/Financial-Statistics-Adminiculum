# Tasks: Financial Statistics Workspace

**Feature**: [`001-financial-statistics-workspace`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/spec.md)  
**Input**: Plan from [`specs/001-financial-statistics-workspace/plan.md`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/plan.md)  
**Constitution**: [`constitution.md (v1.0.0)`](file:///home/chi/repos/Financial-Statistics-Adminiculum/.specify/memory/constitution.md)  
**Status**: Ready for Implementation  

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize the frontend React 18+ TypeScript application with Vite, Tailwind CSS, shadcn/ui, Zod, TanStack Query, Lucide React, Zustand, and React Visx primitives, alongside test tooling.

- [X] T001 Initialize React 18+ TypeScript project scaffold with Vite, Tailwind CSS, `@xyflow/react`, `zustand`, `lucide-react`, `zod`, `@tanstack/react-query`, `@visx/shape`, `@visx/scale`, `@visx/curve`, `@visx/gradient`, `@visx/axis`, `@visx/grid`, `@visx/responsive`, `@visx/tooltip`, `@visx/group`, `jspdf`, and `jspdf-autotable` in `FinancialStatisticsAdminiculum.Web/package.json`
- [X] T002 [P] Configure Tailwind CSS with CSS variables theme tokens (`--background`, `--foreground`, `--primary`, `--border`, `--radius`) for open visual customization in `FinancialStatisticsAdminiculum.Web/tailwind.config.js` and `FinancialStatisticsAdminiculum.Web/src/index.css`
- [X] T003 [P] Configure shadcn/ui settings (`components.json`) and scaffold base Radix UI primitives (`button.tsx`, `dialog.tsx`, `input.tsx`, `command.tsx`, `sheet.tsx`, `badge.tsx`, `slider.tsx`) in `FinancialStatisticsAdminiculum.Web/src/components/ui/`
- [X] T004 [P] Configure Vite development server and backend API reverse proxy (`/api` -> `http://localhost:5000`) in `FinancialStatisticsAdminiculum.Web/vite.config.ts`
- [X] T005 [P] Configure TypeScript compiler settings, strict null checks, and path aliases (`@/*` -> `./src/*`) in `FinancialStatisticsAdminiculum.Web/tsconfig.json`
- [X] T006 [P] Configure Vitest unit testing environment with jsdom and testing-library matchers in `FinancialStatisticsAdminiculum.Web/vitest.config.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain models, persistence layers, base API endpoints, Zod schemas, TanStack Query client, and containerized AI model communication client that MUST be complete before ANY user story can proceed.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T007 [P] Implement core `Workspace` entity with verbatim constraints (`id: UUID, required, immutable`, `name: string, required, 1-100 characters`, `description: string?, max 500 characters`, `viewport: ViewportState, zoom in [0.1, 4.0], x/y finite numbers`, `createdAt: ISO 8601 UTC`, `updatedAt: ISO 8601 UTC`) in `FinancialStatisticsAdminiculum.Core/Entities/Workspace.cs`
- [X] T008 [P] Implement core `AtomicEntity` entity with verbatim constraints (`id: UUID, required`, `workspaceId: UUID, required`, `type: EntityType enum (PriceStream, RollingWindow, MovingAverage, VolatilityEstimator, DistributionAnalyzer, CorrelationMatrix, SignalTrigger, CustomTransform)`, `label: string, required, 1-60 characters`, `position: { x: float, y: float }, finite numbers`, `parameters: Dictionary<string, object>`, `status: EntityStatus enum (Ready, Computing, Stale, Error, Warning)`) in `FinancialStatisticsAdminiculum.Core/Entities/AtomicEntity.cs`
- [X] T009 [P] Implement core `EntityConnection` entity with verbatim constraints (`id: string, unique within workspace`, `sourceEntityId: UUID, required`, `sourcePortId: string, required`, `targetEntityId: UUID, cannot equal sourceEntityId`, `targetPortId: string, required`) with transitive cycle detection in `FinancialStatisticsAdminiculum.Core/Entities/EntityConnection.cs`
- [X] T010 Configure EF Core DbContext entity configurations, relational tables, and JSONB value conversions for `Workspace`, `AtomicEntity`, and `EntityConnection` in `FinancialStatisticsAdminiculum.Infrastructure/AppDbContext.cs`
- [X] T011 Create and apply EF Core database migration for workspace entities in `FinancialStatisticsAdminiculum.Infrastructure/Migrations/`
- [X] T012 [P] Define `IWorkspaceService` and `IWorkspaceRepository` interfaces in `FinancialStatisticsAdminiculum.Application/Interfaces/IWorkspaceService.cs`
- [X] T013 Implement `WorkspaceService` handling CRUD operations, JSONB serialization, and validation in `FinancialStatisticsAdminiculum.Application/Services/WorkspaceService.cs`
- [X] T014 Implement `WorkspacesController` with endpoints (`GET /api/workspaces`, `POST /api/workspaces`, `GET /api/workspaces/{id}`, `PUT /api/workspaces/{id}`, `DELETE /api/workspaces/{id}`) per OpenAPI contract in `FinancialStatisticsAdminiculum.Api/Controllers/WorkspacesController.cs`
- [X] T015 [P] Implement `MarketDataController` serving asset listings (`GET /api/marketdata/assets`) and historical price series (`GET /api/marketdata/series`) in `FinancialStatisticsAdminiculum.Api/Controllers/MarketDataController.cs`
- [X] T016 [P] Implement Zod runtime schemas (`WorkspaceSchema`, `AtomicEntitySchema`, `EntityPortSchema`, `EntityConnectionSchema`, `WorkspaceManifestSchema`, `CsvUploadPreviewSchema`) and export inferred TypeScript types in `FinancialStatisticsAdminiculum.Web/src/schemas/workspace.ts`
- [X] T017 [P] Configure TanStack Query client, default query options, and API client wrapper (`apiClient.ts`) in `FinancialStatisticsAdminiculum.Web/src/services/apiClient.ts`
- [X] T018 [P] Implement TanStack Query hooks (`useWorkspaces`, `useWorkspace`, `useSaveWorkspace`, `useMarketAssets`, `useMarketSeries`) in `FinancialStatisticsAdminiculum.Web/src/hooks/useWorkspaces.ts`
- [X] T019 [P] Implement containerized FunctionGemma HTTP client interface and implementation (`IFunctionGemmaClient`, `FunctionGemmaHttpClient`) connecting to the standalone model microservice on port 8080 in `FinancialStatisticsAdminiculum.Application/AI/Interfaces/IFunctionGemmaClient.cs` and `FinancialStatisticsAdminiculum.Infrastructure/AI/FunctionGemmaHttpClient.cs`

**Checkpoint**: Foundation ready — database tables, backend APIs, Zod schemas, TanStack Query client abstractions, and containerized model HTTP client are established. User story implementation can begin.

---

## Phase 3: User Story 1 - Unguided First-Principles Construction via Atomic Entities (Priority: P1) 🎯 MVP

**Goal**: Enable users to freely assemble, configure, and connect atomic statistical entities on an unguided canvas with sub-200ms reactive parameter recalculation and live visual graph updates.

**Independent Test**: Launch the workspace on a clean canvas, add a `PriceStream` node and a `MovingAverage` node, connect their ports, adjust the period slider, and observe immediate deterministic recalculation without loading indicators or modal interruptions.

### Tests for User Story 1 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T020 [P] [US1] Unit test statistical calculations (SMA, EMA, Rolling Volatility, Z-Scores) for numeric precision and edge cases (zero variance, flatline series) in `FinancialStatisticsAdminiculum.Web/tests/kernel/statisticsKernel.test.ts`
- [X] T021 [P] [US1] Unit test Zustand workspace store (adding nodes, removing nodes, updating parameters, wiring ports, detecting cyclic connections) in `FinancialStatisticsAdminiculum.Web/tests/store/workspaceStore.test.ts`

### Implementation for User Story 1

- [X] T022 [P] [US1] Implement high-precision client-side statistical calculation kernel (`calculateSma`, `calculateEma`, `calculateRollingVolatility`, `calculateZScores`) using deterministic arithmetic in `FinancialStatisticsAdminiculum.Web/src/kernel/statisticsKernel.ts`
- [X] T023 [US1] Implement Zustand workspace store managing canvas nodes, edges, topological recalculation cascades (<200ms for 10,000 observations), and cycle prevention in `FinancialStatisticsAdminiculum.Web/src/store/workspaceStore.ts`
- [X] T024 [P] [US1] Implement base node card wrapper (`BaseEntityNode.tsx`) using unopinionated shadcn/ui primitives, customizable Tailwind styling, typed input/output handles, and Lucide status icons in `FinancialStatisticsAdminiculum.Web/src/components/nodes/BaseEntityNode.tsx`
- [X] T025 [P] [US1] Implement `PriceStreamNode` supporting asset selection (`AAPL`, `MSFT`, `SPY`) and lookback periods in `FinancialStatisticsAdminiculum.Web/src/components/nodes/PriceStreamNode.tsx`
- [X] T026 [P] [US1] Implement `RollingWindowNode` and `MovingAverageNode` with period slider (5–200) and method selector (SMA, EMA, WMA) in `FinancialStatisticsAdminiculum.Web/src/components/nodes/MovingAverageNode.tsx`
- [X] T027 [P] [US1] Implement `VolatilityEstimatorNode` with rolling window and annualization parameters (252 days) in `FinancialStatisticsAdminiculum.Web/src/components/nodes/VolatilityEstimatorNode.tsx`
- [X] T028 [P] [US1] Implement `DistributionAnalyzerNode` and `CorrelationMatrixNode` with configurable bin counts and confidence levels in `FinancialStatisticsAdminiculum.Web/src/components/nodes/DistributionAnalyzerNode.tsx`
- [X] T029 [P] [US1] Implement `SignalTriggerNode` with threshold slider and condition comparators (`GreaterThan`, `LessThan`, `CrossesAbove`) in `FinancialStatisticsAdminiculum.Web/src/components/nodes/SignalTriggerNode.tsx`
- [X] T030 [US1] Implement interactive React Flow canvas component with node palette, customizable grid, and navigation minimap in `FinancialStatisticsAdminiculum.Web/src/components/canvas/WorkspaceCanvas.tsx`
- [X] T031 [US1] Implement parameter inspector sidebar allowing direct fine-grained mathematical parameter editing in `FinancialStatisticsAdminiculum.Web/src/components/inspector/ParameterInspector.tsx`
- [X] T032 [US1] Assemble main unguided application layout in `FinancialStatisticsAdminiculum.Web/src/App.tsx` ensuring zero mandatory tutorial wizards or walkthrough modals appear on startup

**Checkpoint**: User Story 1 is fully functional. The unguided atomic entity workbench operates with sub-200ms reactivity (MVP achieved).

---

## Phase 4: User Story 2 - Natural Language Command Interface & Dynamic Tool Execution (Priority: P1)

**Goal**: Provide an omnipresent natural language processing (NLP) command bar that dynamically resolves analytical tools through containerized FunctionGemma (`docker run`), applies canvas mutations, and provides 1-click undo, with clear offline diagnostic indicators.

**Independent Test**: Press `Cmd+K`, type "Add a 30-day volatility estimator for AAPL and wire an alert when annualized volatility > 25%", verify that the backend resolves the tool via the containerized model microservice, mutates the canvas, and displays an undo badge that reverts the change on click. When the model container is stopped, verify the "AI Offline" notice disables prompt submission while canvas remains operable.

### Tests for User Story 2 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T033 [P] [US2] Contract test for `POST /api/workspaces/{id}/nlp-command` per OpenAPI contract in `FinancialStatisticsAdminiculum.Api.Tests/Controllers/NlpCommandControllerTests.cs`
- [X] T034 [P] [US2] Unit test dynamic tool schema registration and containerized FunctionGemma tool resolver with mocked HTTP client in `FinancialStatisticsAdminiculum.Application.Tests/AI/DynamicToolTests.cs`
- [X] T035 [P] [US2] Unit test client-side Undo/Redo mutation transaction stack in `FinancialStatisticsAdminiculum.Web/tests/store/undoManager.test.ts`

### Implementation for User Story 2

- [X] T036 [P] [US2] Implement dynamic `VolatilityToolHandler` conforming to `IGemmaTool` with JSON schema in `FinancialStatisticsAdminiculum.Application/AI/Tools/VolatilityToolHandler.cs`
- [X] T037 [P] [US2] Implement dynamic `SignalTriggerToolHandler` conforming to `IGemmaTool` with JSON schema in `FinancialStatisticsAdminiculum.Application/AI/Tools/SignalTriggerToolHandler.cs`
- [X] T038 [US2] Implement `NlpCommandService` in `FinancialStatisticsAdminiculum.Application/AI/Services/NlpCommandService.cs` executing through `IToolResolver`, `IAiSchemaAggregator`, and `IFunctionGemmaClient` against the containerized model service
- [X] T039 [US2] Implement `POST /api/workspaces/{id}/nlp-command` endpoint with timeout handling and offline fallback in `FinancialStatisticsAdminiculum.Api/Controllers/AiAnalysisController.cs`
- [X] T040 [US2] Implement client-side Undo/Redo transaction manager for atomic canvas mutations in `FinancialStatisticsAdminiculum.Web/src/store/undoManager.ts`
- [X] T041 [P] [US2] Implement omnipresent NLP command bar (`Cmd+K` / `Ctrl+K`) using shadcn/ui `Command` component with dynamic tool suggestions in `FinancialStatisticsAdminiculum.Web/src/components/nlp/NlpCommandBar.tsx`
- [X] T042 [US2] Implement non-blocking NLP audit trail banner with 1-click Undo button and "AI Offline" diagnostic notice per FR-008 when the containerized inference service is unreachable in `FinancialStatisticsAdminiculum.Web/src/components/nlp/NlpAuditBadge.tsx`

**Checkpoint**: User Stories 1 AND 2 are complete. Canvas can be driven manually or synthesized via natural language commands with instant undo.

---

## Phase 5: User Story 3 - Immediate Observability & Statistical Transparency (Priority: P2)

**Goal**: Render live sparklines and status badges directly on entity nodes using pure React visx primitives, accompanied by an inspectable "Statistics View" displaying probability density functions (KDE), cumulative distributions (CDF), empirical quantiles, and step-by-step mathematical formula traces.

**Independent Test**: Select any computational node on the canvas, open the Statistics View inspector, and confirm real-time rendering of Visx SVG KDE distribution curves, moments table, anomaly markers ($\pm 2\sigma$), and parameter-substituted formulas.

### Tests for User Story 3 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T043 [P] [US3] Unit test Kernel Density Estimation (Gaussian KDE), empirical quantiles, and moments (skewness, kurtosis) in `FinancialStatisticsAdminiculum.Web/tests/kernel/distributionCalculations.test.ts`
- [X] T044 [P] [US3] Unit test React visx chart components (`DistributionPlot.tsx`, `NodeSparkline.tsx`) for pure SVG DOM element rendering and responsive scaling in `FinancialStatisticsAdminiculum.Web/tests/components/VisxCharts.test.tsx`

### Implementation for User Story 3

- [X] T045 [P] [US3] Implement Gaussian KDE, CDF, empirical quantiles ($p01$–$p99$), and outlier anomaly detection ($\pm 2\sigma$, $\pm 3\sigma$) in `FinancialStatisticsAdminiculum.Web/src/kernel/distributionCalculations.ts`
- [X] T046 [P] [US3] Implement on-node live sparkline and mini-metric summary component using React visx (`@visx/shape` `LinePath`, `@visx/scale` `scaleLinear`) styled with Tailwind CSS tokens in `FinancialStatisticsAdminiculum.Web/src/components/nodes/NodeSparkline.tsx`
- [X] T047 [P] [US3] Implement probability density (KDE) and histogram chart component using React visx (`@visx/shape` `Bar`, `LinePath`, `@visx/curve` `curveBasis`, `@visx/axis` `AxisBottom`, `AxisLeft`, `@visx/grid` `GridRows`, `GridColumns`, `@visx/tooltip`) in `FinancialStatisticsAdminiculum.Web/src/components/inspector/DistributionPlot.tsx`
- [X] T048 [P] [US3] Implement statistical moments breakdown table (Mean, Variance, StdDev, Skewness, Kurtosis, Quantiles) in `FinancialStatisticsAdminiculum.Web/src/components/inspector/MomentsTable.tsx`
- [X] T049 [P] [US3] Implement mathematical formula trace viewer showing LaTeX equations with active substituted parameter values in `FinancialStatisticsAdminiculum.Web/src/components/inspector/FormulaTraceView.tsx`
- [X] T050 [US3] Assemble the "Statistics View" slide-out inspector drawer using shadcn/ui `Sheet` in `FinancialStatisticsAdminiculum.Web/src/components/inspector/StatisticsViewDrawer.tsx`

**Checkpoint**: User Stories 1, 2, and 3 are complete. Mathematical concepts and distributions are transparently observable on demand via pure React SVG visx charts.

---

## Phase 6: User Story 4 - Extreme Multi-Format Exportability (Priority: P3)

**Goal**: Provide instant high-resolution image exports (PNG 300 DPI, lossless vector SVG serialized directly from Visx SVG DOM nodes), publication-grade multi-page PDF analytical reports, lossless tabular data downloads (CSV/JSON), CSV auto-detection with interactive preview, and portable JSON workspace manifests with PostgreSQL hybrid persistence.

**Independent Test**: Trigger an export of a multi-node workspace in PNG, PDF, SVG, CSV, and manifest formats; verify that SVG is pure vector XML, PNG is $<1$s at 300 DPI, PDF is $<3$s with embedded vector charts and formula breakdowns, CSV preserves decimal precision, and manifest can be re-imported cleanly. Upload a custom CSV to verify auto-detection and interactive preview.

### Tests for User Story 4 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T051 [P] [US4] Unit test CSV auto-detection, JSON serialization, and manifest Zod schema compliance in `FinancialStatisticsAdminiculum.Web/tests/services/exportService.test.ts`

### Implementation for User Story 4

- [X] T052 [P] [US4] Implement CSV upload parsing and auto-detection utility (verbatim constraints: `delimiter: enum (',', ';', '\t', '|')`, `decimalSeparator: enum ('.', ',')`, `dateFormat: string`) in `FinancialStatisticsAdminiculum.Web/src/services/csvDetector.ts`
- [X] T053 [P] [US4] Implement interactive CSV upload preview dialog with manual override controls per FR-003a using shadcn/ui `Dialog` and `Table` in `FinancialStatisticsAdminiculum.Web/src/components/export/CsvUploadDialog.tsx`
- [X] T054 [P] [US4] Implement native vector SVG serialization (extracting pure Visx SVG DOM nodes via `XMLSerializer`) and high-resolution raster (PNG 300+ DPI via HTML5 canvas drawImage of the SVG) export service in `FinancialStatisticsAdminiculum.Web/src/services/imageExportService.ts`
- [X] T055 [P] [US4] Implement publication-grade multi-page PDF analytical dossier generator embedding vector SVG charts, statistical tables, and methodology notes in `FinancialStatisticsAdminiculum.Web/src/services/pdfExportService.ts`
- [X] T056 [P] [US4] Implement raw and processed time-series tabular CSV and JSON export service in `FinancialStatisticsAdminiculum.Web/src/services/dataExportService.ts`
- [X] T057 [P] [US4] Implement workspace manifest export and import validator adhering to `workspace-manifest.schema.json` via Zod in `FinancialStatisticsAdminiculum.Web/src/services/manifestService.ts`
- [X] T058 [US4] Implement export action menu and format configuration dialog using shadcn/ui `DropdownMenu` in `FinancialStatisticsAdminiculum.Web/src/components/export/ExportMenu.tsx`
- [X] T059 [US4] Implement background auto-save sync persisting workspace topology and job states to PostgreSQL via backend API per FR-021 in `FinancialStatisticsAdminiculum.Web/src/services/persistenceService.ts`

**Checkpoint**: All 4 user stories are fully implemented and independently verifiable.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Containerized model startup scripting, distributed telemetry verification, edge-case hardening, and end-to-end acceptance run.

- [X] T060 [P] Add OpenTelemetry tracing instrumentation and Serilog contextual enrichment to workspace and NLP endpoints in `FinancialStatisticsAdminiculum.Api/Program.cs`
- [X] T061 [P] Provide Dockerfile and compose/launch scripts for containerizing the FunctionGemma model service via `docker run` on port 8080 in `docker/model/Dockerfile` and `compose.yaml`
- [X] T062 [P] Implement global React error boundary and fallback recovery for corrupted canvas topologies in `FinancialStatisticsAdminiculum.Web/src/components/common/ErrorBoundary.tsx`
- [X] T063 Run end-to-end validation scenarios per `quickstart.md` across all 4 user stories (verifying Visx charts and containerized model invocation) and document results in `specs/001-financial-statistics-workspace/quickstart.md`

---

## Dependencies & Execution Order

```mermaid
flowchart TD
    P1["Phase 1: Setup (T001-T006)"] --> P2["Phase 2: Foundational (T007-T019)"]
    P2 --> US1["Phase 3: US1 - Atomic Entities MVP (T020-T032)"]
    P2 --> US2["Phase 4: US2 - NLP Dynamic Tools (T033-T042)"]
    P2 --> US3["Phase 5: US3 - Immediate Observability (T043-T050)"]
    P2 --> US4["Phase 6: US4 - Multi-Format Export (T051-T059)"]
    US1 --> US2
    US1 --> US3
    US1 --> US4
    US2 --> Polish["Phase 7: Polish & Validation (T060-T063)"]
    US3 --> Polish
    US4 --> Polish
```

### User Story Dependencies

- **User Story 1 (P1)**: Depends ONLY on Phase 2 (Foundational). Can be delivered as standalone MVP.
- **User Story 2 (P1)**: Depends on Phase 2 and hooks into the canvas store created in US1 to apply mutations.
- **User Story 3 (P2)**: Depends on Phase 2 and reads node statistical state from US1 to populate Visx distribution charts and formulas.
- **User Story 4 (P3)**: Depends on Phase 2 and captures rendered Visx SVG elements from US1/US3 and tabular metrics.
- **Polish (Phase 7)**: Depends on all desired user stories being complete.

---

## Parallel Execution Examples

### User Story 1 Parallel Stream
```bash
# Tests (TDD - run first):
Task T020: Unit test statistical calculations in tests/kernel/statisticsKernel.test.ts
Task T021: Unit test Zustand workspace store in tests/store/workspaceStore.test.ts

# Custom Atomic Nodes (in parallel once BaseEntityNode is drafted):
Task T025: Implement PriceStreamNode in src/components/nodes/PriceStreamNode.tsx
Task T026: Implement MovingAverageNode in src/components/nodes/MovingAverageNode.tsx
Task T027: Implement VolatilityEstimatorNode in src/components/nodes/VolatilityEstimatorNode.tsx
Task T028: Implement DistributionAnalyzerNode in src/components/nodes/DistributionAnalyzerNode.tsx
Task T029: Implement SignalTriggerNode in src/components/nodes/SignalTriggerNode.tsx
```

### User Story 2 Parallel Stream
```bash
# Backend Tool Handlers (in parallel):
Task T036: Implement VolatilityToolHandler in Application/AI/Tools/VolatilityToolHandler.cs
Task T037: Implement SignalTriggerToolHandler in Application/AI/Tools/SignalTriggerToolHandler.cs

# Frontend NLP Components (in parallel):
Task T041: Implement NlpCommandBar in Web/src/components/nlp/NlpCommandBar.tsx
Task T042: Implement NlpAuditBadge in Web/src/components/nlp/NlpAuditBadge.tsx
```

### User Story 3 Parallel Stream (Visx Charts)
```bash
# Observability components (in parallel):
Task T046: Implement NodeSparkline using React visx in src/components/nodes/NodeSparkline.tsx
Task T047: Implement DistributionPlot using React visx in src/components/inspector/DistributionPlot.tsx
Task T048: Implement MomentsTable in src/components/inspector/MomentsTable.tsx
Task T049: Implement FormulaTraceView in src/components/inspector/FormulaTraceView.tsx
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Complete Phase 1: Setup (T001–T006).
2. Complete Phase 2: Foundational (T007–T019) — BLOCKS all stories.
3. Complete Phase 3: User Story 1 (T020–T032).
4. **STOP and VALIDATE**: Test User Story 1 independently on `http://localhost:5173`.
5. Verify unguided first-principles construction, atomic node placement, and sub-200ms reactive calculations.

### Incremental Delivery
1. Deliver Setup + Foundational → Foundation ready.
2. Deliver User Story 1 → Test independently → Deploy/Demo (MVP!).
3. Deliver User Story 2 → Add NLP command interface (`Cmd+K`), containerized FunctionGemma tool calling, and AI offline notice.
4. Deliver User Story 3 → Add deep observability drawer (React visx KDE plots, CDF curves, quantiles, formulas).
5. Deliver User Story 4 → Add CSV auto-detection/preview, pure SVG / high-DPI PNG exports, multi-page analytical PDF dossiers, and PostgreSQL hybrid persistence.
6. Run Phase 7: Polish & Cross-Cutting Concerns.
