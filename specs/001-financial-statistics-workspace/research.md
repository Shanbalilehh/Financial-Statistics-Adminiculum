# Research & Architectural Decisions: Financial Statistics Workspace

**Feature**: [`001-financial-statistics-workspace`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/spec.md)  
**Date**: 2026-09-13  
**Status**: Completed  

---

## 1. Frontend Architecture & Canvas Runtime

### Decision
Implement the frontend workspace as a standalone React application (`FinancialStatisticsAdminiculum.Web`) built with **React 18/19**, **TypeScript**, **Vite**, **@xyflow/react (React Flow)**, and **Zustand**.

### Rationale
- **Atomic Entity Manipulation**: React Flow provides first-class support for customizable node surfaces, custom input/output port handles, typed topological edge connections, smooth panning/zooming, and minimap navigation.
- **Sub-200ms Reactivity (SC-001)**: Zustand allows selector-based atomic state subscriptions. When a user drags a slider on an upstream entity (e.g. rolling window period), only the affected node and its direct downstream dependents re-render and recalculate, completely bypassing global canvas re-renders.
- **Fast Build & Dev Velocity**: Vite delivers instant Hot Module Replacement (HMR) and optimized ES module bundling for development and Docker containerization.

### Alternatives Considered
- *Custom HTML5 Canvas / WebGL*: Maximum theoretical throughput, but prohibitive maintenance overhead; embedding rich input sliders, formula badges, and dropdowns inside WebGL requires building a complete UI toolkit from scratch.
- *Konva.js / Pixi.js*: Excellent 2D rendering, but lacks native React component lifecycle integration for node interiors, making complex DOM inputs and sparklines awkward.
- *Redux Toolkit*: Robust, but introduces excessive boilerplate and action dispatch overhead compared to Zustand's lightweight, fine-grained store updates.

---

## 2. Statistical Knowledge Engine & Hybrid Calculation Pipeline

### Decision
Adopt a **hybrid calculation pipeline**:
1. **Client-Side Analytical Kernel (TypeScript)**: Executes lightweight atomic node recalculations (moving averages, exponential smoothing, rolling variance, z-scores, quantile buckets, skewness, kurtosis, and sparklines) locally in the browser memory using typed arrays and deterministic decimal/math helpers.
2. **Server-Side Engine (.NET 8 Web API & PostgreSQL)**: Manages bulk historical time-series storage, multi-asset database queries, long-running backtest simulations, and asynchronous background jobs via RabbitMQ.

### Rationale
- **Sub-200ms Response Time (SC-001)**: Roundtrips to the server for every single slider adjustment introduce 100–300ms of network and serialization latency. Executing deterministic statistical formulas locally on arrays up to 10,000 observations takes <15ms in modern V8/JavaScript engines.
- **First-Principles Understanding**: Users see calculations update instantaneously as they experiment with parameters, reinforcing immediate comprehension of statistical dynamics without network lag or loading spinners.

### Alternatives Considered
- *Server-Only Recalculation*: Sending HTTP POST requests to `.NET API` for every node parameter tick. Rejected because network latency and request queueing violate the sub-200ms requirement (SC-001) under interactive slider drag.
- *Client-Only Architecture*: Discarding the backend. Rejected because it cannot leverage existing PostgreSQL historical datasets, RabbitMQ background processing, or FunctionGemma AI inference.

---

## 3. NLP Command Entry & Dynamic Tool Orchestration

### Decision
Expose an omnipresent natural language command palette in the React UI that communicates with a dedicated backend endpoint: `POST /api/aianalysis/nlp-command`.
- The .NET backend orchestrates intent resolution through `IOrchestratorService` and `FunctionGemma.Api` (via ONNX Runtime GenAI).
- Dynamic tools implement `IGemmaTool` and register typed JSON schemas via `IAiSchemaAggregator` in compliance with Constitution Principle II.
- Successful tool execution returns a structured **Workspace Mutation Instruction** (e.g., `ADD_ENTITY`, `CONNECT_PORTS`, `UPDATE_PARAMETERS`, `CONFIGURE_VIEW`).
- The React canvas consumes the mutation instruction, updates its topology, pushes the previous snapshot onto an **Undo/Redo Stack**, and logs the action to a non-intrusive audit badge.

### Rationale
- Complies strictly with Constitution Principle II (Schema-First AI Tool Contracts & Guardrails).
- Separates AI intent parsing (server-side, leveraging GPU/ONNX runtime) from visual canvas rendering (client-side).
- Ensures 100% reversible operations (SC-007) via client-side undo history.

### Alternatives Considered
- *In-Browser LLM (WebLLM / WASM)*: Download sizes exceed 1.5 GB and inference latency is high on client machines without dedicated WebGPU hardware. Rejected in favor of the existing server-side `FunctionGemma.Api` container.
- *Unstructured Chat Window*: Traditional chat sidebar with markdown responses. Rejected because the user specifically requested "a NLP entry and dynamic tools actionable via the NLP entry" directly manipulating the workspace.

---

## 4. Extreme Multi-Format Export Pipeline

### Decision
Implement a multi-tier export engine:
1. **Vector Graphics (SVG)**: Client-side SVG serialization preserving crisp lines, typography, and axes scales at any zoom level.
2. **High-Resolution Raster (PNG 300+ DPI)**: Rendered via HTML5 Canvas API and `html-to-image` with a 3x/4x device pixel ratio multiplier.
3. **Publication-Grade PDF Reports**: Assembled client-side using `jspdf` and `jspdf-autotable` (with server-side compilation fallback via `QuestPDF` for complex multi-page dossiers), embedding high-res vector charts, descriptive statistics matrices, methodology definitions, and entity relationship diagrams.
4. **Lossless Structured Data (CSV/JSON)**: Client-side Blob streaming exporting full-precision timestamped numerical tables.
5. **Portable Workspace Manifest (JSON)**: Schema-validated JSON snapshot containing complete canvas topology, entity parameters, wiring, and view settings for import/export.

### Rationale
- Fulfills SC-005 (image export in <1s) and SC-006 (PDF report in <3s).
- Gives users complete portability to transfer findings into academic papers, presentations, spreadsheets, or other workspace instances.

### Alternatives Considered
- *Browser `window.print()` Print Stylesheet*: Lacks programmatic pagination control, clips wide diagrams, and cannot generate isolated PNG/SVG artifacts.
- *Headless Chromium Container (Puppeteer) exclusively*: High memory footprint and 3–6s latency per export, violating the sub-second image requirement.

---

## 5. Observability & Statistical Transparency UX

### Decision
Provide immediate, multi-tiered visual inspection:
1. **On-Node Sparkline & Metric Badges**: Live sparklines, current value, min/max, and health badges rendered directly on the entity canvas card.
2. **Dedicated "Statistics View" Inspector Drawer**: When any entity is selected, a slide-out panel displays:
   - Kernel Density Estimation (KDE) / Empirical Probability Density Plot
   - Cumulative Distribution Function (CDF)
   - Quantile-Quantile (Q-Q) normality plot
   - Rolling moment summary: Mean, Variance, Standard Deviation, Skewness, Kurtosis
   - Anomaly badges (points exceeding $\pm 2\sigma$ or $\pm 3\sigma$)
   - Mathematical formula breakdown with live substitution of active parameters.

### Rationale
- Directly delivers on "Everything immediately observable" and "Prioritize understanding not guideability".
- Eliminates mystery: users can trace why an indicator behaved in a certain way down to the exact formula step.

---

## Summary of Technology Stack Selections

| Layer | Selected Technologies | Justification |
| :--- | :--- | :--- |
| **Frontend Framework** | React 18+ with TypeScript | Robust ecosystem, typed safety, requested explicitly by user. |
| **Frontend Build Tool** | Vite | Lightning-fast HMR, lightweight containerization, modern ESM. |
| **Canvas & Flow Engine** | `@xyflow/react` (React Flow) | Production-tested node-based canvas, custom ports/handles, high performance. |
| **Frontend State & Reactivity** | Zustand | Selector-based fine-grained subscriptions ensuring sub-200ms reactive updates. |
| **Charting & Visuals** | Lightweight Charts & Chart.js / D3 | High FPS time-series rendering, custom statistical distributions and KDE plots. |
| **Export Engines** | `html-to-image`, `jspdf`, native Blob streaming | Instant client-side PNG/SVG/PDF/CSV/JSON generation. |
| **Backend API** | ASP.NET Core (.NET 8.0) Web API | Existing repository foundation, high throughput, Clean Architecture. |
| **AI Inference & Tooling** | FunctionGemma / ONNX Runtime GenAI | Schema-first dynamic tools (`IGemmaTool`, `IAiSchemaAggregator`). |
| **Telemetry & Observability**| OpenTelemetry + Serilog -> Seq | Project standard, distributed trace propagation. |
| **Database** | PostgreSQL 16 + EF Core | High-performance persistence with indexed JSONB and time-series. |
| **Messaging** | RabbitMQ | Resilient background processing and decoupled notifications. |
