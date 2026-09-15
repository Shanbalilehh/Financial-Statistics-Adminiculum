# Tasks: Financial Statistics Workspace

**Feature**: [`001-financial-statistics-workspace`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/spec.md)  
**Input**: Plan from [`specs/001-financial-statistics-workspace/plan.md`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/plan.md)  
**Constitution**: [`constitution.md (v1.0.0)`](file:///home/chi/repos/Financial-Statistics-Adminiculum/.specify/memory/constitution.md)  
**Status**: Ready for Implementation  

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize the frontend React 18+ TypeScript application and test configurations within the workspace.

- [X] T001 Initialize React 18+ TypeScript project scaffold with Vite, `@xyflow/react`, `zustand`, `lucide-react`, `chart.js`, `react-chartjs-2`, `lightweight-charts`, `jspdf`, `jspdf-autotable`, and `html-to-image` in `FinancialStatisticsAdminiculum.Web/package.json`
- [X] T002 [P] Configure Vite development server and backend API reverse proxy (`/api` -> `http://localhost:5000`) in `FinancialStatisticsAdminiculum.Web/vite.config.ts`
- [X] T003 [P] Configure TypeScript compiler settings, strict null checks, and path aliases (`@/*` -> `./src/*`) in `FinancialStatisticsAdminiculum.Web/tsconfig.json`
- [X] T004 [P] Configure Vitest unit testing environment with jsdom and testing-library matchers in `FinancialStatisticsAdminiculum.Web/vitest.config.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain models, persistence layers, and base API endpoints that MUST be complete before ANY user story can proceed.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 [P] Implement core `Workspace` entity with validation (Id: UUID, Name: 1-100 chars, Description: max 500 chars, ViewportState, Timestamps) in `FinancialStatisticsAdminiculum.Core/Entities/Workspace.cs`
- [X] T006 [P] Implement core `AtomicEntity` entity with validation (Type: Enum, Label: 1-60 chars, Position: x/y finite numbers, Parameters: JSONB, Status: Enum) in `FinancialStatisticsAdminiculum.Core/Entities/AtomicEntity.cs`
- [X] T007 [P] Implement core `EntityConnection` entity (SourceEntityId: UUID, SourcePortId: string, TargetEntityId: UUID, TargetPortId: string) with circular dependency prevention in `FinancialStatisticsAdminiculum.Core/Entities/EntityConnection.cs`
- [X] T008 Configure EF Core DbContext entity configurations and JSONB value conversions for `Workspace`, `AtomicEntity`, and `EntityConnection` in `FinancialStatisticsAdminiculum.Infrastructure/AppDbContext.cs`
- [X] T009 Create and apply EF Core database migration for workspace entities in `FinancialStatisticsAdminiculum.Infrastructure/Migrations/`
- [X] T010 [P] Define `IWorkspaceService` and `IWorkspaceRepository` interfaces in `FinancialStatisticsAdminiculum.Application/Interfaces/IWorkspaceService.cs`
- [X] T011 Implement `WorkspaceService` handling CRUD operations, JSONB serialization, and validation in `FinancialStatisticsAdminiculum.Application/Services/WorkspaceService.cs`
- [X] T012 Implement `WorkspacesController` with endpoints (`GET /api/workspaces`, `POST /api/workspaces`, `GET /api/workspaces/{id}`, `PUT /api/workspaces/{id}`, `DELETE /api/workspaces/{id}`) per contract in `FinancialStatisticsAdminiculum.Api/Controllers/WorkspacesController.cs`
- [X] T013 [P] Implement `MarketDataController` serving asset listings (`GET /api/marketdata/assets`) and historical price series (`GET /api/marketdata/series`) in `FinancialStatisticsAdminiculum.Api/Controllers/MarketDataController.cs`
- [X] T014 [P] Define TypeScript interfaces for Workspace, AtomicEntity, EntityPort, and EntityConnection mirroring data contracts in `FinancialStatisticsAdminiculum.Web/src/types/workspace.ts`
- [X] T015 Implement API client service wrapping fetch for workspace CRUD and market data retrieval in `FinancialStatisticsAdminiculum.Web/src/services/apiClient.ts`

**Checkpoint**: Foundation ready — database tables, backend APIs, and frontend client abstractions are established. User story implementation can begin.

---

## Phase 3: User Story 1 - Unguided First-Principles Construction via Atomic Entities (Priority: P1) 🎯 MVP

**Goal**: Enable users to freely assemble, configure, and connect atomic statistical entities on an unguided canvas with sub-200ms reactive parameter recalculation and live visual graph updates.

**Independent Test**: Launch the workspace on a clean canvas, add a `PriceStream` node and a `MovingAverage` node, connect their ports, adjust the period slider, and observe immediate deterministic recalculation without loading indicators or modal interruptions.

### Tests for User Story 1 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T016 [P] [US1] Unit test statistical calculations (SMA, EMA, Rolling Volatility, Z-Scores) for numeric precision and edge cases (zero variance, missing values) in `FinancialStatisticsAdminiculum.Web/tests/kernel/statisticsKernel.test.ts`
- [X] T017 [P] [US1] Unit test Zustand workspace store (adding nodes, removing nodes, updating parameters, wiring ports, detecting cyclic connections) in `FinancialStatisticsAdminiculum.Web/tests/store/workspaceStore.test.ts`

### Implementation for User Story 1

- [X] T018 [P] [US1] Implement high-precision client-side statistical calculation kernel (`calculateSma`, `calculateEma`, `calculateRollingVolatility`, `calculateZScores`) in `FinancialStatisticsAdminiculum.Web/src/kernel/statisticsKernel.ts`
- [X] T019 [US1] Implement Zustand workspace store managing canvas nodes, edges, topological recalculation cascades, and parameter modifications in `FinancialStatisticsAdminiculum.Web/src/store/workspaceStore.ts`
- [X] T020 [P] [US1] Implement base node card wrapper with custom input/output handle ports and health status badges in `FinancialStatisticsAdminiculum.Web/src/components/nodes/BaseEntityNode.tsx`
- [X] T021 [P] [US1] Implement `PriceStreamNode` supporting symbol selection (`AAPL`, `MSFT`, `SPY`) and lookback periods in `FinancialStatisticsAdminiculum.Web/src/components/nodes/PriceStreamNode.tsx`
- [X] T022 [P] [US1] Implement `MovingAverageNode` with period slider (5–200) and calculation method selector (SMA, EMA) in `FinancialStatisticsAdminiculum.Web/src/components/nodes/MovingAverageNode.tsx`
- [X] T023 [P] [US1] Implement `VolatilityEstimatorNode` with rolling window and annualization parameters (252 days) in `FinancialStatisticsAdminiculum.Web/src/components/nodes/VolatilityEstimatorNode.tsx`
- [X] T024 [P] [US1] Implement `SignalTriggerNode` with threshold slider and condition comparators (`GreaterThan`, `LessThan`) in `FinancialStatisticsAdminiculum.Web/src/components/nodes/SignalTriggerNode.tsx`
- [X] T025 [US1] Implement interactive React Flow canvas component with entity palette, grid background, and navigation minimap in `FinancialStatisticsAdminiculum.Web/src/components/canvas/WorkspaceCanvas.tsx`
- [X] T026 [US1] Implement parameter inspector sidebar allowing direct fine-grained mathematical parameter editing in `FinancialStatisticsAdminiculum.Web/src/components/inspector/ParameterInspector.tsx`
- [X] T027 [US1] Assemble main application layout in `FinancialStatisticsAdminiculum.Web/src/App.tsx` ensuring zero mandatory tutorial modals or walkthrough gates appear on startup

**Checkpoint**: User Story 1 is fully functional. The unguided atomic entity workbench operates with sub-200ms reactivity (MVP achieved).

---

## Phase 4: User Story 2 - Natural Language Command Interface & Dynamic Tool Execution (Priority: P1)

**Goal**: Provide an omnipresent natural language processing (NLP) command bar that dynamically resolves analytical tools through FunctionGemma, applies canvas mutations, and provides 1-click undo.

**Independent Test**: Press `Cmd+K`, type "Add a 30-day volatility estimator for AAPL and wire an alert when annualized volatility > 25%", verify that the backend resolves the tool, mutates the canvas, and displays an undo badge that reverts the change on click.

### Tests for User Story 2 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T028 [P] [US2] Contract test for `POST /api/workspaces/{id}/nlp-command` per OpenAPI contract in `FinancialStatisticsAdminiculum.Api.Tests/Controllers/NlpCommandControllerTests.cs`
- [X] T029 [P] [US2] Unit test dynamic tool schema registration and FunctionGemma tool resolver in `FinancialStatisticsAdminiculum.Application.Tests/AI/DynamicToolTests.cs`
- [X] T030 [P] [US2] Unit test client-side Undo/Redo mutation transaction stack in `FinancialStatisticsAdminiculum.Web/tests/store/undoManager.test.ts`

### Implementation for User Story 2

- [X] T031 [P] [US2] Implement dynamic `VolatilityToolHandler` conforming to `IGemmaTool` with JSON schema in `FinancialStatisticsAdminiculum.Application/AI/Tools/VolatilityToolHandler.cs`
- [X] T032 [P] [US2] Implement dynamic `SignalTriggerToolHandler` conforming to `IGemmaTool` with JSON schema in `FinancialStatisticsAdminiculum.Application/AI/Tools/SignalTriggerToolHandler.cs`
- [X] T033 [US2] Implement `NlpCommandService` in `FinancialStatisticsAdminiculum.Application/AI/Services/NlpCommandService.cs` translating FunctionGemma function calls into canvas mutations (`ADD_ENTITY`, `CONNECT_PORTS`, `UPDATE_PARAMETERS`)
- [X] T034 [US2] Implement `POST /api/workspaces/{id}/nlp-command` endpoint in `FinancialStatisticsAdminiculum.Api/Controllers/AiAnalysisController.cs`
- [X] T035 [US2] Implement client-side Undo/Redo transaction manager for canvas mutations in `FinancialStatisticsAdminiculum.Web/src/store/undoManager.ts`
- [X] T036 [P] [US2] Implement omnipresent NLP command bar (`Cmd+K` / `Ctrl+K`) with suggestions and execution status in `FinancialStatisticsAdminiculum.Web/src/components/nlp/NlpCommandBar.tsx`
- [X] T037 [US2] Implement non-blocking NLP audit trail banner with 1-click Undo button in `FinancialStatisticsAdminiculum.Web/src/components/nlp/NlpAuditBadge.tsx`

**Checkpoint**: User Stories 1 AND 2 are complete. Canvas can be driven manually or synthesized via natural language commands with instant undo.

---

## Phase 5: User Story 3 - Immediate Observability & Statistical Transparency (Priority: P2)

**Goal**: Render live sparklines and status badges directly on entity nodes, accompanied by an inspectable "Statistics View" displaying probability density functions (KDE), cumulative distributions, empirical quantiles, and step-by-step mathematical formula traces.

**Independent Test**: Select any computational node on the canvas, open the Statistics View inspector, and confirm real-time rendering of KDE distribution curves, moments table, anomaly markers ($\pm 2\sigma$), and parameter-substituted formulas.

### Tests for User Story 3 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T038 [P] [US3] Unit test Kernel Density Estimation (KDE), empirical quantiles, and higher-order moments (skewness, kurtosis) in `FinancialStatisticsAdminiculum.Web/tests/kernel/distributionCalculations.test.ts`

### Implementation for User Story 3

- [X] T039 [P] [US3] Implement Kernel Density Estimation (Gaussian KDE), CDF, empirical quantiles ($p01$–$p99$), and outlier detection ($\pm 2\sigma$, $\pm 3\sigma$) in `FinancialStatisticsAdminiculum.Web/src/kernel/distributionCalculations.ts`
- [X] T040 [P] [US3] Implement on-node live sparkline and mini-metric summary component in `FinancialStatisticsAdminiculum.Web/src/components/nodes/NodeSparkline.tsx`
- [X] T041 [P] [US3] Implement probability density (KDE) and histogram chart component using Chart.js in `FinancialStatisticsAdminiculum.Web/src/components/inspector/DistributionPlot.tsx`
- [X] T042 [P] [US3] Implement statistical moments breakdown table (Mean, Variance, StdDev, Skewness, Kurtosis, Quantiles) in `FinancialStatisticsAdminiculum.Web/src/components/inspector/MomentsTable.tsx`
- [X] T043 [P] [US3] Implement mathematical formula trace viewer showing LaTeX equations with active substituted parameter values in `FinancialStatisticsAdminiculum.Web/src/components/inspector/FormulaTraceView.tsx`
- [X] T044 [US3] Assemble the "Statistics View" slide-out inspector drawer in `FinancialStatisticsAdminiculum.Web/src/components/inspector/StatisticsViewDrawer.tsx` and wire to selected canvas entity

**Checkpoint**: User Stories 1, 2, and 3 are complete. Mathematical concepts and distributions are transparently observable on demand.

---

## Phase 6: User Story 4 - Extreme Multi-Format Exportability (Priority: P3)

**Goal**: Provide instant high-resolution image exports (PNG 300 DPI, vector SVG), publication-grade multi-page PDF analytical reports, lossless tabular data downloads (CSV/JSON), and portable JSON workspace manifests.

**Independent Test**: Trigger an export of a multi-node workspace in PNG, PDF, CSV, and manifest formats; verify that PNG is $<1$s, PDF is $<3$s with full vector charts and formula breakdowns, CSV preserves decimal precision, and manifest can be re-imported cleanly.

### Tests for User Story 4 ⚠️
> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T045 [P] [US4] Unit test CSV formatting, JSON serialization, and manifest schema compliance in `FinancialStatisticsAdminiculum.Web/tests/services/exportService.test.ts`

### Implementation for User Story 4

- [X] T046 [P] [US4] Implement raster (PNG 300+ DPI) and vector (SVG) capture service using `html-to-image` in `FinancialStatisticsAdminiculum.Web/src/services/imageExportService.ts`
- [X] T047 [P] [US4] Implement publication-grade multi-page PDF analytical dossier generator embedding vector charts, statistical tables, and methodology notes in `FinancialStatisticsAdminiculum.Web/src/services/pdfExportService.ts`
- [X] T048 [P] [US4] Implement raw and processed time-series tabular CSV and JSON export service in `FinancialStatisticsAdminiculum.Web/src/services/dataExportService.ts`
- [X] T049 [P] [US4] Implement workspace manifest export and import validator adhering to `workspace-manifest.schema.json` in `FinancialStatisticsAdminiculum.Web/src/services/manifestService.ts`
- [X] T050 [US4] Implement export action menu and format configuration dialog in `FinancialStatisticsAdminiculum.Web/src/components/export/ExportMenu.tsx`

**Checkpoint**: All 4 user stories are fully implemented and independently verifiable.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Distributed telemetry verification, container updates, edge-case hardening, and end-to-end acceptance run.

- [X] T051 [P] Add OpenTelemetry tracing instrumentation and Serilog contextual enrichment to new workspace and NLP endpoints in `FinancialStatisticsAdminiculum.Api/Program.cs`
- [X] T052 [P] Update `compose.yaml` to include the `web` frontend service with hot reloading for development in `compose.yaml`
- [X] T053 [P] Implement global React error boundary and fallback recovery for corrupted canvas topologies in `FinancialStatisticsAdminiculum.Web/src/components/common/ErrorBoundary.tsx`
- [X] T054 Run end-to-end validation scenarios per `quickstart.md` across all 4 user stories and document results in `specs/001-financial-statistics-workspace/quickstart.md`

---

## Dependencies & Execution Order

```mermaid
flowchart TD
    P1["Phase 1: Setup (T001-T004)"] --> P2["Phase 2: Foundational (T005-T015)"]
    P2 --> US1["Phase 3: US1 - Atomic Entities MVP (T016-T027)"]
    P2 --> US2["Phase 4: US2 - NLP Dynamic Tools (T028-T037)"]
    P2 --> US3["Phase 5: US3 - Immediate Observability (T038-T044)"]
    P2 --> US4["Phase 6: US4 - Multi-Format Export (T045-T050)"]
    US1 --> US2
    US1 --> US3
    US1 --> US4
    US2 --> Polish["Phase 7: Polish & Validation (T051-T054)"]
    US3 --> Polish
    US4 --> Polish
```

### User Story Dependencies

- **User Story 1 (P1)**: Depends ONLY on Phase 2 (Foundational). Can be delivered as standalone MVP.
- **User Story 2 (P1)**: Depends on Phase 2 and hooks into the canvas store created in US1 to apply mutations.
- **User Story 3 (P2)**: Depends on Phase 2 and reads node statistical state from US1 to populate distribution charts and formulas.
- **User Story 4 (P3)**: Depends on Phase 2 and captures rendered visual cards from US1/US3 and tabular metrics.
- **Polish (Phase 7)**: Depends on all desired user stories being complete.

---

## Parallel Execution Examples

### User Story 1 Parallel Stream
```bash
# Tests (TDD - run first):
Task T016: Unit test statistical calculations in tests/kernel/statisticsKernel.test.ts
Task T017: Unit test Zustand workspace store in tests/store/workspaceStore.test.ts

# Custom Atomic Nodes (in parallel once BaseEntityNode is drafted):
Task T021: Implement PriceStreamNode in src/components/nodes/PriceStreamNode.tsx
Task T022: Implement MovingAverageNode in src/components/nodes/MovingAverageNode.tsx
Task T023: Implement VolatilityEstimatorNode in src/components/nodes/VolatilityEstimatorNode.tsx
Task T024: Implement SignalTriggerNode in src/components/nodes/SignalTriggerNode.tsx
```

### User Story 2 Parallel Stream
```bash
# Backend Tool Handlers (in parallel):
Task T031: Implement VolatilityToolHandler in Application/AI/Tools/VolatilityToolHandler.cs
Task T032: Implement SignalTriggerToolHandler in Application/AI/Tools/SignalTriggerToolHandler.cs

# Frontend NLP Components (in parallel):
Task T036: Implement NlpCommandBar in Web/src/components/nlp/NlpCommandBar.tsx
Task T037: Implement NlpAuditBadge in Web/src/components/nlp/NlpAuditBadge.tsx
```

---

## Implementation Strategy

1. **MVP First (User Story 1 Only)**:
   - Deliver Setup (T001–T004) and Foundation (T005–T015).
   - Implement User Story 1 (T016–T027).
   - Validate unguided first-principles construction on `http://localhost:5173`. Users can assemble and recalculate atomic entities immediately.
2. **Incremental Enhancements**:
   - Deliver User Story 2: Add NLP command interface (`Cmd+K`) and FunctionGemma tool calling with 1-click undo.
   - Deliver User Story 3: Add deep observability drawer (KDE plots, CDF curves, quantiles, formulas).
   - Deliver User Story 4: Add high-DPI PNG/SVG, multi-page analytical PDF dossiers, and portable workspace manifests.
3. **Cross-Cutting Verification**:
   - Run full test suites (`dotnet test` and `npm test`).
   - Validate performance against gates ($<200$ms recalculation, $<2$s NLP roundtrip, $<1$s image export, $<3$s PDF report).

---

## Phase 8: Convergence

**Purpose**: Address gaps and constitutional non-compliances identified by `/speckit-converge` relative to `spec.md`, `plan.md`, `constitution.md`, and existing tasks.

- [ ] T055 [P] Scaffold backend test projects (`FinancialStatisticsAdminiculum.Api.Tests` and `FinancialStatisticsAdminiculum.Application.Tests`) and implement `NlpCommandControllerTests.cs` and `DynamicToolTests.cs` per Constitution V and tasks T028/T029 (missing)
- [ ] T056 [P] Remove `Castle.Core` package reference from `FinancialStatisticsAdminiculum.Core.csproj` to enforce pure domain isolation per Constitution I (contradicts)
- [ ] T057 [P] Fix KDE density calculation, moment variance rounding, formula trace syntax, and CSV header formatting to achieve 100% passing tests in `FinancialStatisticsAdminiculum.Web` per Constitution V and tasks T016/T038/T045 (contradicts)
- [ ] T058 Implement atomic batch transaction undo in `workspaceStore.ts` and `NlpCommandBar.tsx` so all mutations from a single NLP execution revert on one Undo action per FR-013, SC-007, US2/AC2 (contradicts)
- [ ] T059 [P] Implement dedicated node cards, palette buttons, and recalculation handlers for `RollingWindow`, `DistributionAnalyzer`, and `CorrelationMatrix` in `FinancialStatisticsAdminiculum.Web` per FR-003, US1/AC1 (partial)
- [ ] T060 Refactor `NlpCommandService.cs` to execute through `IToolResolver` and `IAiSchemaAggregator` with schema validation, and convert `SignalTriggerToolHandler` to `decimal` per Constitution II, FR-009, FR-010 (partial)
- [ ] T061 Add transitive cycle detection to `Workspace.AddConnection` in `Workspace.cs` to prevent cyclic entity graphs at the domain model level per task T007 and spec Edge Cases (partial)
- [ ] T062 Remove legacy endpoints and commented-out methods in `AiAnalysisController.cs` to maintain clean OpenAPI contract boundaries per plan and task T034 (unrequested)
