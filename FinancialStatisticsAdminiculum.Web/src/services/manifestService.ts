import { downloadFile } from './dataExportService';

export interface WorkspaceManifestJson {
  manifestVersion: string;
  exportedAt: string;
  generator: string;
  workspace: {
    id: string;
    name: string;
    description?: string;
    viewport: { x: number; y: number; zoom: number };
    entities: {
      id: string;
      type: string;
      label: string;
      position: { x: number; y: number };
      parameters: Record<string, any>;
    }[];
    connections: {
      id: string;
      sourceEntityId: string;
      sourcePortId: string;
      targetEntityId: string;
      targetPortId: string;
    }[];
  };
}

export function serializeManifest(
  workspaceId: string,
  name: string,
  nodes: any[],
  edges: any[],
  description?: string
): WorkspaceManifestJson {
  return {
    manifestVersion: '1.0.0',
    exportedAt: new Date().toISOString(),
    generator: 'FinancialStatisticsAdminiculum',
    workspace: {
      id: workspaceId,
      name,
      description,
      viewport: { x: 0, y: 0, zoom: 1.0 },
      entities: nodes.map((n) => ({
        id: n.id,
        type: n.data?.type || n.type,
        label: n.data?.label || n.id,
        position: n.position || { x: 0, y: 0 },
        parameters: n.data?.parameters || {},
      })),
      connections: edges.map((e) => ({
        id: e.id,
        sourceEntityId: e.source,
        sourcePortId: e.sourceHandle || 'out_series',
        targetEntityId: e.target,
        targetPortId: e.targetHandle || 'in_series',
      })),
    },
  };
}

export function validateManifest(manifest: any): boolean {
  if (!manifest || manifest.manifestVersion !== '1.0.0') return false;
  if (!manifest.workspace || !manifest.workspace.name) return false;
  if (!Array.isArray(manifest.workspace.entities)) return false;
  if (!Array.isArray(manifest.workspace.connections)) return false;
  return true;
}

export function downloadManifest(manifest: WorkspaceManifestJson, filename?: string): void {
  const safeName = (filename || manifest.workspace.name || 'workspace')
    .toLowerCase()
    .replace(/[^a-z0-9_-]/g, '-');
  downloadFile(
    JSON.stringify(manifest, null, 2),
    `${safeName}-manifest.json`,
    'application/json'
  );
}
