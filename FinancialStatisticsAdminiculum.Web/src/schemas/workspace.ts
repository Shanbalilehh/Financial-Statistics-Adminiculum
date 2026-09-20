import { z } from 'zod';

// --- Port & Connection Schemas ---
export const DataTypeSchema = z.enum([
  'TimeSeriesArray',
  'ScalarMetric',
  'BooleanCondition',
  'Matrix'
]);

export const PortDirectionSchema = z.enum(['Input', 'Output']);

export const EntityPortSchema = z.object({
  id: z.string().min(1),
  entityId: z.string().uuid(),
  name: z.string().min(1).max(60),
  direction: PortDirectionSchema,
  dataType: DataTypeSchema,
});

export const EntityConnectionSchema = z.object({
  id: z.string().min(1),
  sourceEntityId: z.string().uuid(),
  sourcePortId: z.string().min(1),
  targetEntityId: z.string().uuid(),
  targetPortId: z.string().min(1),
});

// --- Entity Type & Parameters ---
export const EntityTypeSchema = z.enum([
  'PriceStream',
  'RollingWindow',
  'MovingAverage',
  'VolatilityEstimator',
  'DistributionAnalyzer',
  'CorrelationMatrix',
  'SignalTrigger',
  'CustomTransform',
]);

export const EntityStatusSchema = z.enum([
  'Ready',
  'Computing',
  'Stale',
  'Error',
  'Warning'
]);

export const AtomicEntitySchema = z.object({
  id: z.string().uuid(),
  workspaceId: z.string().uuid(),
  type: EntityTypeSchema,
  label: z.string().min(1).max(60),
  position: z.object({ x: z.number(), y: z.number() }),
  parameters: z.record(z.unknown()),
  inputPorts: z.array(EntityPortSchema),
  outputPorts: z.array(EntityPortSchema),
  status: EntityStatusSchema,
  errorMessage: z.string().nullable().optional(),
  lastCalculatedAt: z.string().datetime().nullable().optional(),
});

// --- Workspace & Manifest ---
export const ViewportStateSchema = z.object({
  x: z.number(),
  y: z.number(),
  zoom: z.number().min(0.1).max(4.0),
});

export const WorkspaceSettingsSchema = z.object({
  defaultTimeHorizon: z.string().default('1Y'),
  samplingInterval: z.enum(['1m', '5m', '1h', '1d']).default('1d'),
  theme: z.string().default('system'),
});

export const WorkspaceSchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1).max(100),
  description: z.string().max(500).nullable().optional(),
  viewport: ViewportStateSchema,
  settings: WorkspaceSettingsSchema,
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
  entities: z.array(AtomicEntitySchema),
  connections: z.array(EntityConnectionSchema),
});

export const WorkspaceManifestSchema = z.object({
  manifestVersion: z.literal('1.0.0'),
  workspace: WorkspaceSchema,
  exportedAt: z.string().datetime(),
});

// --- CSV Upload Preview Schema (FR-003a) ---
export const CsvColumnSchema = z.object({
  index: z.number().int().nonnegative(),
  header: z.string(),
  inferredType: z.enum(['datetime', 'numeric', 'string', 'boolean']),
  sampleValues: z.array(z.string()),
});

export const CsvUploadPreviewSchema = z.object({
  delimiter: z.enum([',', ';', '\t', '|']),
  decimalSeparator: z.enum(['.', ',']),
  dateFormat: z.string(),
  totalRows: z.number().int().nonnegative(),
  columns: z.array(CsvColumnSchema),
  previewRows: z.array(z.record(z.string())),
});

// --- Inferred TypeScript Types ---
export type EntityPort = z.infer<typeof EntityPortSchema>;
export type EntityConnection = z.infer<typeof EntityConnectionSchema>;
export type AtomicEntity = z.infer<typeof AtomicEntitySchema>;
export type Workspace = z.infer<typeof WorkspaceSchema>;
export type WorkspaceManifest = z.infer<typeof WorkspaceManifestSchema>;
export type CsvUploadPreview = z.infer<typeof CsvUploadPreviewSchema>;
