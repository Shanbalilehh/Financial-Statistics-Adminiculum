import { describe, it, expect, beforeEach } from 'vitest';
import { useWorkspaceStore } from '../../src/store/workspaceStore';

describe('workspaceStore', () => {
  beforeEach(() => {
    useWorkspaceStore.getState().resetWorkspace();
  });

  it('starts with a clean unguided workspace without tutorials or modals', () => {
    const state = useWorkspaceStore.getState();
    expect(state.nodes.length).toBe(0);
    expect(state.edges.length).toBe(0);
    expect(state.selectedNodeId).toBeNull();
  });

  it('adds an atomic entity node correctly', () => {
    const store = useWorkspaceStore.getState();
    store.addEntity('PriceStream', { x: 100, y: 100 }, { symbol: 'AAPL', lookback: 100 });

    const nodes = useWorkspaceStore.getState().nodes;
    expect(nodes.length).toBe(1);
    expect(nodes[0].data.type).toBe('PriceStream');
    expect(nodes[0].data.parameters.symbol).toBe('AAPL');
  });

  it('updates parameters and triggers local recalculation', () => {
    const store = useWorkspaceStore.getState();
    store.addEntity('MovingAverage', { x: 200, y: 200 }, { period: 20, method: 'SMA' });
    const nodeId = useWorkspaceStore.getState().nodes[0].id;

    store.updateNodeParameters(nodeId, { period: 50 });

    const updatedNode = useWorkspaceStore.getState().nodes.find(n => n.id === nodeId);
    expect(updatedNode?.data.parameters.period).toBe(50);
  });

  it('prevents self-referencing connections', () => {
    const store = useWorkspaceStore.getState();
    store.addEntity('MovingAverage', { x: 100, y: 100 });
    const nodeId = useWorkspaceStore.getState().nodes[0].id;

    const added = store.addConnection({
      source: nodeId,
      target: nodeId,
      sourceHandle: 'out_series',
      targetHandle: 'in_series',
    });

    expect(added).toBe(false);
    expect(useWorkspaceStore.getState().edges.length).toBe(0);
  });

  it('reverts all batch mutations in a single undo step', () => {
    const store = useWorkspaceStore.getState();
    expect(store.nodes.length).toBe(0);

    const mutations = [
      { action: 'ADD_ENTITY', payload: { id: 'p1', type: 'PriceStream', position: { x: 0, y: 0 } } },
      { action: 'ADD_ENTITY', payload: { id: 'v1', type: 'VolatilityEstimator', position: { x: 200, y: 0 } } },
      { action: 'ADD_CONNECTION', payload: { sourceEntityId: 'p1', targetEntityId: 'v1' } },
    ];

    store.applyBatchMutations(mutations);

    expect(useWorkspaceStore.getState().nodes.length).toBe(2);
    expect(useWorkspaceStore.getState().edges.length).toBe(1);

    // Single undo reverts the entire batch
    useWorkspaceStore.getState().undo();

    expect(useWorkspaceStore.getState().nodes.length).toBe(0);
    expect(useWorkspaceStore.getState().edges.length).toBe(0);
  });
});
