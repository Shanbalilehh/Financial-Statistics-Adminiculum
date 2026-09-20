import { ApiClient } from './apiClient';
import { Node, Edge } from '@xyflow/react';

export class PersistenceService {
  private static saveTimeout: any = null;
  private static readonly DEBOUNCE_MS = 2000;

  static triggerAutoSave(
    workspaceId: string,
    workspaceName: string,
    nodes: Node[],
    edges: Edge[]
  ): void {
    // 1. Immediate local cache save for offline safety
    try {
      const localState = {
        workspaceId,
        workspaceName,
        nodes,
        edges,
        savedAt: new Date().toISOString(),
      };
      localStorage.setItem(`fsa_workspace_${workspaceId}`, JSON.stringify(localState));
    } catch {
      // Non-blocking if localStorage is full or disabled
    }

    // 2. Debounced PostgreSQL backend persistence
    if (this.saveTimeout) {
      clearTimeout(this.saveTimeout);
    }

    this.saveTimeout = setTimeout(async () => {
      try {
        await ApiClient.updateWorkspace(workspaceId, {
          name: workspaceName,
          entities: nodes.map((n) => ({
            id: n.id,
            workspaceId,
            type: ((n.type as any) || 'CustomTransform'),
            label: ((n.data as any)?.label as string) || n.id,
            position: n.position,
            parameters: ((n.data as any)?.parameters as Record<string, any>) || {},
            status: ((n.data as any)?.status as any) || 'Ready',
          })),
          connections: edges.map((e) => ({
            id: e.id,
            sourceEntityId: e.source,
            sourcePortId: (e.sourceHandle as string) || 'out_series',
            targetEntityId: e.target,
            targetPortId: (e.targetHandle as string) || 'in_series',
          })),
        });
      } catch (err) {
        // Silently log; local cache already preserved
        console.warn('Auto-save to PostgreSQL backend postponed (offline or sync delay):', err);
      }
    }, this.DEBOUNCE_MS);
  }
}
