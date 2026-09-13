import { describe, it, expect, beforeEach } from 'vitest';
import { UndoManager } from '../../src/store/undoManager';
import { CanvasMutation } from '../../src/types/workspace';

describe('undoManager', () => {
  let undoManager: UndoManager;

  beforeEach(() => {
    undoManager = new UndoManager();
  });

  it('records mutations and supports undo and redo', () => {
    const mutation: CanvasMutation = {
      action: 'ADD_ENTITY',
      payload: { id: 'node-1', type: 'VolatilityEstimator' },
    };

    undoManager.push(mutation);
    expect(undoManager.canUndo()).toBe(true);
    expect(undoManager.canRedo()).toBe(false);

    const undone = undoManager.undo();
    expect(undone).toEqual(mutation);
    expect(undoManager.canUndo()).toBe(false);
    expect(undoManager.canRedo()).toBe(true);

    const redone = undoManager.redo();
    expect(redone).toEqual(mutation);
  });
});
