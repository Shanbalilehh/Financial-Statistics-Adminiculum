export type EntityType = 
  | 'PriceStream'
  | 'RollingWindow'
  | 'MovingAverage'
  | 'VolatilityEstimator'
  | 'DistributionAnalyzer'
  | 'CorrelationMatrix'
  | 'SignalTrigger'
  | 'CustomTransform';

export type EntityStatus = 'Ready' | 'Computing' | 'Stale' | 'Error' | 'Warning';

export type DataType = 'TimeSeriesArray' | 'ScalarMetric' | 'BooleanCondition' | 'Matrix';

export interface EntityPort {
  id: string;
  name: string;
  direction: 'input' | 'output';
  dataType: DataType;
}

export interface EntityPosition {
  x: number;
  y: number;
}

export interface StatisticalMoment {
  count: number;
  mean: number;
  variance: number;
  stdDev: number;
  skewness: number;
  kurtosis: number;
  quantiles: {
    p01: number;
    p05: number;
    p25: number;
    p50: number;
    p75: number;
    p95: number;
    p99: number;
  };
}

export interface HistogramBin {
  binStart: number;
  binEnd: number;
  count: number;
  density: number;
}

export interface AnomalyEvent {
  timestamp: string;
  value: number;
  zScore: number;
  reason: string;
}

export interface DiagnosticRecord {
  entityId: string;
  moments?: StatisticalMoment;
  histogram?: HistogramBin[];
  anomalies?: AnomalyEvent[];
  formulaTrace?: string;
}

export interface AtomicEntity {
  id: string;
  workspaceId: string;
  type: EntityType;
  label: string;
  position: EntityPosition;
  parameters: Record<string, any>;
  status: EntityStatus;
  errorMessage?: string;
  lastCalculatedAt?: string;
  calculatedValues?: {
    timestamps?: string[];
    series?: number[];
    metrics?: Record<string, number>;
    sparkline?: number[];
  };
  diagnostics?: DiagnosticRecord;
}

export interface EntityConnection {
  id: string;
  sourceEntityId: string;
  sourcePortId: string;
  targetEntityId: string;
  targetPortId: string;
}

export interface ViewportState {
  x: number;
  y: number;
  zoom: number;
}

export interface WorkspaceSummary {
  id: string;
  name: string;
  description?: string;
  entityCount: number;
  updatedAt: string;
}

export interface WorkspaceDetail {
  id: string;
  name: string;
  description?: string;
  viewport: ViewportState;
  entities: AtomicEntity[];
  connections: EntityConnection[];
  createdAt: string;
  updatedAt: string;
}

export interface Observation {
  timestamp: string;
  close: number;
  volume: number;
}

export interface TimeSeriesData {
  symbol: string;
  interval: string;
  observations: Observation[];
}

export type MutationAction = 
  | 'ADD_ENTITY'
  | 'REMOVE_ENTITY'
  | 'UPDATE_PARAMETERS'
  | 'ADD_CONNECTION'
  | 'REMOVE_CONNECTION';

export interface CanvasMutation {
  action: MutationAction;
  payload: any;
}

export interface NlpCommandResponse {
  commandId: string;
  prompt: string;
  status: 'Executed' | 'Failed' | 'Rejected';
  resolvedTool?: string;
  extractedArguments?: Record<string, any>;
  mutations: CanvasMutation[];
  explanation?: string;
}
