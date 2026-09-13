import { create } from 'zustand';
import { Node, Edge, Connection, addEdge } from '@xyflow/react';
import { EntityType, EntityStatus, AtomicEntity, DiagnosticRecord } from '../types/workspace';
import { calculateSma, calculateEma, calculateRollingVolatility, calculateMoments } from '../kernel/statisticsKernel';

export interface WorkspaceNodeData extends Record<string, unknown> {
  id: string;
  type: EntityType;
  label: string;
  parameters: Record<string, any>;
  status: EntityStatus;
  errorMessage?: string;
  calculatedValues?: {
    timestamps?: string[];
    series?: number[];
    metrics?: Record<string, number>;
    sparkline?: number[];
  };
  diagnostics?: DiagnosticRecord;
}

export interface WorkspaceState {
  workspaceId: string;
  workspaceName: string;
  nodes: Node<WorkspaceNodeData>[];
  edges: Edge[];
  selectedNodeId: string | null;
  isEvaluating: boolean;
  history: { nodes: Node<WorkspaceNodeData>[]; edges: Edge[] }[];
  future: { nodes: Node<WorkspaceNodeData>[]; edges: Edge[] }[];

  // Actions
  setWorkspaceName: (name: string) => void;
  selectNode: (id: string | null) => void;
  addEntity: (type: EntityType, position: { x: number; y: number }, initialParams?: Record<string, any>) => string;
  removeEntity: (id: string) => void;
  updateNodeParameters: (id: string, parameters: Record<string, any>) => void;
  updateNodePosition: (id: string, position: { x: number; y: number }) => void;
  addConnection: (connection: Connection) => boolean;
  removeConnection: (edgeId: string) => void;
  recalculateGraph: () => void;
  resetWorkspace: () => void;
  loadWorkspace: (id: string, name: string, nodes: Node<WorkspaceNodeData>[], edges: Edge[]) => void;
  undo: () => void;
  redo: () => void;
}

const DEFAULT_PARAMS: Record<EntityType, Record<string, any>> = {
  PriceStream: { symbol: 'AAPL', lookback: 252, interval: '1d' },
  MovingAverage: { period: 20, method: 'SMA' },
  RollingWindow: { windowSize: 30, minPeriods: 5 },
  VolatilityEstimator: { period: 30, annualizationFactor: 252 },
  DistributionAnalyzer: { numBins: 40, confidenceLevel: 0.95 },
  CorrelationMatrix: { benchmarkSymbol: 'SPY' },
  SignalTrigger: { condition: 'GreaterThan', threshold: 25.0 },
  CustomTransform: { multiplier: 1.0, offset: 0.0 },
};

export const useWorkspaceStore = create<WorkspaceState>((set, get) => ({
  workspaceId: crypto.randomUUID ? crypto.randomUUID() : 'default-ws',
  workspaceName: 'Financial Statistics Workspace',
  nodes: [],
  edges: [],
  selectedNodeId: null,
  isEvaluating: false,
  history: [],
  future: [],

  setWorkspaceName: (name) => set({ workspaceName: name }),

  selectNode: (id) => set({ selectedNodeId: id }),

  addEntity: (type, position, initialParams) => {
    const id = `node_${type}_${Date.now()}_${Math.floor(Math.random() * 1000)}`;
    const params = { ...DEFAULT_PARAMS[type], ...initialParams };
    const label = `${type}`;

    const newNode: Node<WorkspaceNodeData> = {
      id,
      type,
      position,
      data: {
        id,
        type,
        label,
        parameters: params,
        status: 'Ready',
        calculatedValues: {
          series: [],
          sparkline: [],
          metrics: {},
        },
      },
    };

    set((state) => ({
      history: [...state.history, { nodes: state.nodes, edges: state.edges }],
      future: [],
      nodes: [...state.nodes, newNode],
      selectedNodeId: id,
    }));

    get().recalculateGraph();
    return id;
  },

  removeEntity: (id) => {
    set((state) => ({
      history: [...state.history, { nodes: state.nodes, edges: state.edges }],
      future: [],
      nodes: state.nodes.filter((n) => n.id !== id),
      edges: state.edges.filter((e) => e.source !== id && e.target !== id),
      selectedNodeId: state.selectedNodeId === id ? null : state.selectedNodeId,
    }));

    get().recalculateGraph();
  },

  updateNodeParameters: (id, parameters) => {
    set((state) => ({
      nodes: state.nodes.map((node) => {
        if (node.id === id) {
          return {
            ...node,
            data: {
              ...node.data,
              parameters: { ...node.data.parameters, ...parameters },
            },
          };
        }
        return node;
      }),
    }));

    get().recalculateGraph();
  },

  updateNodePosition: (id, position) => {
    set((state) => ({
      nodes: state.nodes.map((node) => (node.id === id ? { ...node, position } : node)),
    }));
  },

  addConnection: (connection) => {
    if (connection.source === connection.target) return false;

    const { nodes, edges } = get();
    // Check if edge causes a cycle
    const adjacency: Record<string, string[]> = {};
    edges.forEach((e) => {
      if (!adjacency[e.source]) adjacency[e.source] = [];
      adjacency[e.source].push(e.target);
    });
    if (!adjacency[connection.source!]) adjacency[connection.source!] = [];
    adjacency[connection.source!].push(connection.target!);

    const visited = new Set<string>();
    const recStack = new Set<string>();

    const hasCycle = (curr: string): boolean => {
      visited.add(curr);
      recStack.add(curr);
      const neighbors = adjacency[curr] || [];
      for (const neighbor of neighbors) {
        if (!visited.has(neighbor) && hasCycle(neighbor)) return true;
        if (recStack.has(neighbor)) return true;
      }
      recStack.delete(curr);
      return false;
    };

    for (const node of nodes) {
      if (!visited.has(node.id) && hasCycle(node.id)) {
        return false; // Reject cycle
      }
    }

    set((state) => ({
      history: [...state.history, { nodes: state.nodes, edges: state.edges }],
      future: [],
      edges: addEdge({ ...connection, animated: true }, state.edges),
    }));

    get().recalculateGraph();
    return true;
  },

  removeConnection: (edgeId) => {
    set((state) => ({
      history: [...state.history, { nodes: state.nodes, edges: state.edges }],
      future: [],
      edges: state.edges.filter((e) => e.id !== edgeId),
    }));
    get().recalculateGraph();
  },

  recalculateGraph: () => {
    const { nodes, edges } = get();
    if (nodes.length === 0) return;

    // Topological order evaluation
    const inDegree: Record<string, number> = {};
    const adj: Record<string, { targetId: string; edge: Edge }[]> = {};

    nodes.forEach((n) => {
      inDegree[n.id] = 0;
      adj[n.id] = [];
    });

    edges.forEach((e) => {
      if (inDegree[e.target] !== undefined) inDegree[e.target]++;
      if (adj[e.source]) adj[e.source].push({ targetId: e.target, edge: e });
    });

    const queue: string[] = nodes.filter((n) => inDegree[n.id] === 0).map((n) => n.id);
    const nodeOutputs: Record<string, number[]> = {};
    const updatedNodesMap = new Map<string, WorkspaceNodeData>(nodes.map((n) => [n.id, { ...n.data }]));

    while (queue.length > 0) {
      const currentId = queue.shift()!;
      const nodeData = updatedNodesMap.get(currentId)!;
      let calculatedSeries: number[] = [];

      if (nodeData.type === 'PriceStream') {
        const lookback = Number(nodeData.parameters.lookback || 100);
        // Deterministic price series generation for instant local calculation
        const hash = Math.abs((nodeData.parameters.symbol || 'AAPL').split('').reduce((a: number, b: string) => (a << 5) - a + b.charCodeAt(0), 0));
        let p = 150 + (hash % 50);
        calculatedSeries = [];
        for (let i = 0; i < lookback; i++) {
          const tick = Math.sin((i + hash) / 10) * 2 + ((i * 13) % 7 - 3) * 0.5;
          p = Math.max(10, p + tick);
          calculatedSeries.push(Math.round(p * 100) / 100);
        }
      } else {
        // Collect inputs from incoming edges
        const incomingEdges = edges.filter((e) => e.target === currentId);
        const firstInputSeries = incomingEdges.length > 0 ? (nodeOutputs[incomingEdges[0].source] || []) : [];

        if (firstInputSeries.length > 0) {
          switch (nodeData.type) {
            case 'MovingAverage': {
              const period = Number(nodeData.parameters.period || 20);
              const method = nodeData.parameters.method || 'SMA';
              calculatedSeries = method === 'EMA' 
                ? calculateEma(firstInputSeries, period) 
                : calculateSma(firstInputSeries, period);
              break;
            }
            case 'VolatilityEstimator': {
              const period = Number(nodeData.parameters.period || 30);
              const factor = Number(nodeData.parameters.annualizationFactor || 252);
              calculatedSeries = calculateRollingVolatility(firstInputSeries, period, factor);
              break;
            }
            case 'SignalTrigger': {
              const threshold = Number(nodeData.parameters.threshold || 25);
              const condition = nodeData.parameters.condition || 'GreaterThan';
              calculatedSeries = firstInputSeries.map((v) => 
                condition === 'GreaterThan' ? (v > threshold ? 1 : 0) : (v < threshold ? 1 : 0)
              );
              break;
            }
            case 'DistributionAnalyzer': {
              calculatedSeries = [...firstInputSeries];
              break;
            }
            default:
              calculatedSeries = [...firstInputSeries];
          }
        }
      }

      nodeOutputs[currentId] = calculatedSeries;
      const moments = calculateMoments(calculatedSeries);
      const sparkline = calculatedSeries.slice(-30);

      nodeData.calculatedValues = {
        series: calculatedSeries,
        sparkline,
        metrics: {
          current: calculatedSeries[calculatedSeries.length - 1] ?? 0,
          mean: moments.mean,
          stdDev: moments.stdDev,
        },
      };
      nodeData.diagnostics = {
        entityId: currentId,
        moments,
      };

      for (const { targetId } of adj[currentId] || []) {
        inDegree[targetId]--;
        if (inDegree[targetId] === 0) {
          queue.push(targetId);
        }
      }
    }

    set({
      nodes: nodes.map((n) => ({
        ...n,
        data: updatedNodesMap.get(n.id) || n.data,
      })),
    });
  },

  resetWorkspace: () => {
    set({
      nodes: [],
      edges: [],
      selectedNodeId: null,
      history: [],
      future: [],
    });
  },

  loadWorkspace: (id, name, nodes, edges) => {
    set({
      workspaceId: id,
      workspaceName: name,
      nodes,
      edges,
      selectedNodeId: null,
      history: [],
      future: [],
    });
    get().recalculateGraph();
  },

  undo: () => {
    const { history, future, nodes, edges } = get();
    if (history.length === 0) return;
    const previous = history[history.length - 1];
    set({
      history: history.slice(0, -1),
      future: [{ nodes, edges }, ...future],
      nodes: previous.nodes,
      edges: previous.edges,
    });
    get().recalculateGraph();
  },

  redo: () => {
    const { history, future, nodes, edges } = get();
    if (future.length === 0) return;
    const next = future[0];
    set({
      history: [...history, { nodes, edges }],
      future: future.slice(1),
      nodes: next.nodes,
      edges: next.edges,
    });
    get().recalculateGraph();
  },
}));
