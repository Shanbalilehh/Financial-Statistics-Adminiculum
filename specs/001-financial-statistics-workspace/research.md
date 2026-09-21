# Research & Architectural Decisions: Financial Statistics Workspace

**Feature**: [`001-financial-statistics-workspace`](file:///home/chi/repos/Financial-Statistics-Adminiculum/specs/001-financial-statistics-workspace/spec.md)  
**Date**: 2026-09-13  
**Status**: Completed  

---

## 1. Frontend Architecture, Canvas Runtime & UI Stack

### Decision
Implement the frontend workspace as a standalone React application (`FinancialStatisticsAdminiculum.Web`) built with **React 18/19**, **TypeScript**, **Vite**, **Tailwind CSS**, **shadcn/ui**, **Zod**, **TanStack Query**, **Lucide React**, **Zustand**, **@xyflow/react (React Flow)**, and **React visx (`@visx/*`)**.

The visual design is explicitly left open to customization: UI components use unopinionated shadcn/ui primitives (Radix UI) and Tailwind CSS utility classes driven by CSS variable theme tokens (`--background`, `--foreground`, `--primary`, `--border`, `--radius`, etc.), ensuring complete aesthetic flexibility without modifying component logic.

### Rationale & Role of Each Stack Element
- **Vite**: Provides instant Hot Module Replacement (HMR) and optimized ES module bundling for fast local development and lightweight Docker containerization.
- **Tailwind CSS & shadcn/ui**: Provides an unopinionated, copy-and-own component architecture based on accessible Radix UI primitives. The visual design is not hardcoded; developers or users can easily customize color palettes, border radiuses, typography, and dark/light mode themes via CSS variables.
- **React visx (`@visx/*`)**: Supplies modular, low-level visualization primitives (`@visx/shape`, `@visx/scale`, `@visx/curve`, `@visx/gradient`, `@visx/axis`, `@visx/grid`, `@visx/responsive`, `@visx/tooltip`). Because visx renders pure React SVG DOM elements rather than HTML5 `<canvas>` bitmaps, it offers two decisive advantages:
  1. *Theme Token Integration*: Charts and sparklines can directly use Tailwind CSS classes and CSS variable tokens (`stroke-primary`, `fill-primary/20`, `stroke-muted`), perfectly matching the open visual design principle.
  2. *Lossless Vector SVG Export*: Visx graphics live in the real DOM as scalable vector paths, enabling instant, 100% vector-crisp SVG exports and 300+ DPI raster captures without any canvas blur.
- **Zod**: Enforces contract-first runtime schema validation across all boundaries: validating user-uploaded CSV parsing configurations, dynamic tool execution payloads from FunctionGemma, node parameter inputs, and exported/imported workspace JSON manifests. Inferred types (`z.infer<typeof Schema>`) ensure zero drift between runtime validation and compile-time TypeScript types.
- **TanStack Query (React Query)**: Manages all server state, HTTP caching, optimistic updates, and background refetching for interactions with the .NET backend API (`WorkspacesController`, `MarketDataController`, `AiAnalysisController`). Decouples server data fetching and query lifecycle from UI rendering.
- **Zustand**: Manages local, high-frequency canvas state (node positions, viewport zoom/pan, topological connections, active parameter values, and the undo/redo history stack). Selector-based subscriptions ensure sub-200ms reactive updates (SC-001) by re-rendering only affected nodes on parameter slider drag without triggering full canvas re-renders.
- **Lucide React**: Supplies clean, consistent, scalable SVG iconography across canvas toolbars, entity operational health badges, and export actions.
- **@xyflow/react (React Flow)**: Provides first-class support for customizable node surfaces, custom input/output port handles, typed topological edge connections, smooth panning/zooming, and minimap navigation.

### Alternatives Considered
- *Chart.js / HTML5 Canvas*: Rejected for primary statistical graphics and distribution plots. Canvas-based charts render raster pixels that blur when scaled, cannot be directly styled using Tailwind CSS variables or theme tokens, and cannot produce true lossless vector SVG exports. Visx renders native SVG DOM elements, solving all three limitations.
- *Hardcoded Component Styling (e.g. fixed CSS/styled-components theme)*: Rejected in favor of Tailwind CSS + shadcn/ui with CSS variables, which leaves visual design open to user and developer customization.
- *Redux Toolkit / Context API*: Rejected because Redux introduces excessive action boilerplate and Context API causes widespread unnecessary re-renders across the canvas, violating the <200ms reactivity threshold. Zustand provides precise selector-based subscriptions.
- *Custom HTML5 Canvas / WebGL*: Maximum theoretical throughput, but prohibitive maintenance overhead; embedding rich input sliders, formula badges, and dropdowns inside WebGL requires building a complete UI toolkit from scratch.
- *Manual Fetch / Axios without TanStack Query*: Requires writing bespoke caching, loading state, error retry, and query invalidation logic across every component, creating boilerplate and potential state desynchronization.

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

## 3. NLP Command Entry, Dynamic Tool Orchestration & Model Containerization

### Decision
Expose an omnipresent natural language command palette in the React UI that communicates with a dedicated backend endpoint: `POST /api/aianalysis/nlp-command`.
- **Model Containerization Strategy (`docker run`)**:
  - The FunctionGemma AI model is packaged into an isolated, self-contained container image (`financial-statistics/functiongemma-model:latest`).
  - The model container encapsulates the ONNX Runtime GenAI engine, model weights, tokenizers, and a lightweight HTTP/REST inference server exposing `/invocations` and `/health`.
  - It is executed directly via `docker run`:
    ```bash
    docker run -d \
      --name functiongemma-model \
      -p 8080:8080 \
      -e MODEL_PATH=/app/models/functiongemma_oga \
      -e EXECUTION_PROVIDER=CPU \
      --health-cmd="curl -f http://localhost:8080/health || exit 1" \
      financial-statistics/functiongemma-model:latest
    ```
  - The .NET backend orchestrates intent resolution through `IOrchestratorService` calling the containerized model over HTTP.
- Dynamic tools implement `IGemmaTool` and register typed JSON schemas via `IAiSchemaAggregator` in compliance with Constitution Principle II.
- Successful tool execution returns a structured **Workspace Mutation Instruction** (e.g., `ADD_ENTITY`, `CONNECT_PORTS`, `UPDATE_PARAMETERS`, `CONFIGURE_VIEW`).
- The React canvas consumes the mutation instruction, updates its topology, pushes the previous snapshot onto an **Undo/Redo Stack**, and logs the action to a non-intrusive audit badge.

### Rationale
- Decouples large model weights (>1.5 GB) and specialized inference runtimes (ONNX Runtime GenAI, CUDA/DirectML dependencies) from the .NET application build lifecycle.
- Facilitates swapping, versioning, and scaling models independently (e.g. running multiple model containers for different statistical tasks) without touching application binaries.
- Complies strictly with Constitution Principle II (Schema-First AI Tool Contracts & Guardrails).
- Ensures 100% reversible operations (SC-007) via client-side undo history.

### Alternatives Considered
- *Monolithic Docker Build with Embedded Model*: Embedding model download and inference binaries directly into the .NET API container. Rejected because it results in 5+ GB container image bloat, slow CI/CD pipelines, and prevents independent model scaling or GPU hardware allocation.
- *In-Browser LLM (WebLLM / WASM)*: Download sizes exceed 1.5 GB and inference latency is high on client machines without dedicated WebGPU hardware. Rejected in favor of the containerized model running via `docker run`.
- *Unstructured Chat Window*: Traditional chat sidebar with markdown responses. Rejected because the user specifically requested "a NLP entry and dynamic tools actionable via the NLP entry" directly manipulating the workspace.

---

## 4. Extreme Multi-Format Export Pipeline

### Decision
Implement a multi-tier export engine:
1. **Vector Graphics (SVG)**: Native SVG DOM serialization from React visx preserving crisp lines, typography, and axes scales at any zoom level.
2. **High-Resolution Raster (PNG 300+ DPI)**: Rendered via `html-to-image` from the visx SVG DOM with a 3x/4x device pixel ratio multiplier.
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
1. **On-Node Sparkline & Metric Badges**: Live sparklines rendered with `@visx/shape` (`LinePath`), current value, min/max, and health badges rendered directly on the entity canvas card.
2. **Dedicated "Statistics View" Inspector Drawer**: When any entity is selected, a slide-out panel displays:
   - Kernel Density Estimation (KDE) / Empirical Probability Density Plot (built with `@visx/shape` `AreaClosed` and `LinePath`)
   - Cumulative Distribution Function (CDF) (built with `@visx/shape` `LinePath`)
   - Histogram distribution bins (built with `@visx/shape` `Bar`)
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
| **UI Components & Styling** | Tailwind CSS & shadcn/ui (Radix UI) | Accessible, unopinionated primitives; visual design left open to customization via CSS variables. |
| **Schema Validation** | Zod | Runtime validation for tool payloads, manifests, entity params, and CSV uploads with inferred types. |
| **Server State & Caching**| TanStack Query (React Query) | Declarative data fetching, automatic caching, background polling, and optimistic mutations for API endpoints. |
| **Canvas & Flow Engine** | `@xyflow/react` (React Flow) | Production-tested node-based canvas, custom ports/handles, high performance. |
| **Frontend State & Reactivity** | Zustand | Selector-based fine-grained subscriptions ensuring sub-200ms reactive updates. |
| **Iconography** | Lucide React | Clean, scalable, lightweight SVG icons for toolbars, badges, and controls. |
| **Graphics & Charts** | React visx (`@visx/*`) | Pure React SVG primitives (`@visx/shape`, `@visx/scale`, `@visx/curve`, `@visx/grid`, `@visx/axis`, `@visx/responsive`, `@visx/tooltip`). Direct Tailwind CSS theming and lossless vector SVG export. |
| **Export Engines** | `html-to-image`, `jspdf`, native Blob streaming | Instant client-side PNG/SVG/PDF/CSV/JSON generation. |
| **Backend API** | ASP.NET Core (.NET 8.0) Web API | Existing repository foundation, high throughput, Clean Architecture. |
| **AI Inference & Tooling** | Containerized Model via `docker run` (FunctionGemma + ONNX Runtime GenAI) | Independent model containerization, isolated hardware allocation, schema-first dynamic tools (`IGemmaTool`, `IAiSchemaAggregator`). |
| **Telemetry & Observability**| OpenTelemetry + Serilog -> Seq | Project standard, distributed trace propagation. |
| **Database** | PostgreSQL 16 + EF Core | High-performance persistence with indexed JSONB and time-series. |
| **Messaging** | RabbitMQ | Resilient background processing and decoupled notifications. |
