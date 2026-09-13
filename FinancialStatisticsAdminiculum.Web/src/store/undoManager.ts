import { CanvasMutation } from '../types/workspace';

export class UndoManager {
  private undoStack: CanvasMutation[] = [];
  private redoStack: CanvasMutation[] = [];

  push(mutation: CanvasMutation): void {
    this.undoStack.push(mutation);
    this.redoStack = []; // Clear redo on new action
  }

  canUndo(): boolean {
    return this.undoStack.length > 0;
  }

  canRedo(): boolean {
    return this.redoStack.length > 0;
  }

  undo(): CanvasMutation | null {
    if (!this.canUndo()) return null;
    const mutation = this.undoStack.pop()!;
    this.redoStack.push(mutation);
    return mutation;
  }

  redo(): CanvasMutation | null {
    if (!this.canRedo()) return null;
    const mutation = this.redoStack.pop()!;
    this.undoStack.push(mutation);
    return mutation;
  }

  clear(): void {
    this.undoStack = [];
    this.redoStack = [];
  }
}

export const globalUndoManager = new UndoManager();
