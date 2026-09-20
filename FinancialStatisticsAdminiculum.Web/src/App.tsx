import React, { useEffect, useState } from 'react';
import { WorkspaceCanvas } from './components/canvas/WorkspaceCanvas';
import { ParameterInspector } from './components/inspector/ParameterInspector';
import { useWorkspaceStore } from './store/workspaceStore';
import { RotateCcw, Undo, Redo, LayoutGrid } from 'lucide-react';

import { NlpCommandBar } from './components/nlp/NlpCommandBar';
import { NlpAuditBadge } from './components/nlp/NlpAuditBadge';
import { StatisticsViewDrawer } from './components/inspector/StatisticsViewDrawer';
import { ExportMenu } from './components/export/ExportMenu';
import { ErrorBoundary } from './components/common/ErrorBoundary';

export const App: React.FC = () => {
  const [nlpExplanation, setNlpExplanation] = useState<string | null>(null);
  const [isAiOffline, setIsAiOffline] = useState<boolean>(false);
  const workspaceName = useWorkspaceStore((s) => s.workspaceName);
  const setWorkspaceName = useWorkspaceStore((s) => s.setWorkspaceName);
  const resetWorkspace = useWorkspaceStore((s) => s.resetWorkspace);
  const undo = useWorkspaceStore((s) => s.undo);
  const redo = useWorkspaceStore((s) => s.redo);
  const nodes = useWorkspaceStore((s) => s.nodes);
  const addEntity = useWorkspaceStore((s) => s.addEntity);

  // Initialize with a default pair of entities for immediate observation if completely empty
  useEffect(() => {
    if (nodes.length === 0) {
      addEntity('PriceStream', { x: 120, y: 180 }, { symbol: 'AAPL', lookback: 100 });
      addEntity('MovingAverage', { x: 440, y: 180 }, { period: 20, method: 'SMA' });
    }
  }, []);

  return (
    <ErrorBoundary>
      <div className="flex h-screen w-screen flex-col bg-background font-sans text-foreground overflow-hidden">
        {/* Top Header */}
        <header className="flex h-14 items-center justify-between border-b border-border bg-card/80 px-4 backdrop-blur z-20">
          <div className="flex items-center space-x-3">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary/20 border border-primary/30 text-primary">
              <LayoutGrid className="h-4 w-4" />
            </div>
            <div className="flex items-baseline space-x-2">
              <span className="text-sm font-bold tracking-tight text-foreground">
                Financial Statistics Adminiculum
              </span>
              <span className="text-[10px] rounded bg-muted px-1.5 py-0.5 text-muted-foreground font-mono">
                v1.0.0
              </span>
            </div>
            <div className="h-4 w-[1px] bg-border" />
            <input
              type="text"
              value={workspaceName}
              onChange={(e) => setWorkspaceName(e.target.value)}
              className="rounded bg-transparent px-2 py-1 text-xs font-medium text-foreground hover:bg-muted focus:bg-muted focus:outline-none transition-colors"
            />
          </div>

          {/* Global Canvas Actions */}
          <div className="flex items-center space-x-2">
            <NlpCommandBar
              onMutationApplied={(expl) => setNlpExplanation(expl)}
              isAiOffline={isAiOffline}
              onAiOfflineChange={setIsAiOffline}
            />

            <div className="flex items-center space-x-1 rounded-lg border border-border bg-card/60 p-1">
              <button
                onClick={undo}
                title="Undo (Ctrl+Z)"
                className="rounded p-1 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
              >
                <Undo className="h-4 w-4" />
              </button>
              <button
                onClick={redo}
                title="Redo (Ctrl+Y)"
                className="rounded p-1 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
              >
                <Redo className="h-4 w-4" />
              </button>
              <div className="h-3 w-[1px] bg-border" />
              <button
                onClick={resetWorkspace}
                title="Clear Canvas"
                className="rounded p-1 text-muted-foreground hover:bg-accent hover:text-destructive transition-colors"
              >
                <RotateCcw className="h-4 w-4" />
              </button>
            </div>

            <ExportMenu />
          </div>
        </header>

        {/* Main Canvas Area */}
        <main className="relative flex-1">
          <WorkspaceCanvas />
          <ParameterInspector />
          <NlpAuditBadge
            message={nlpExplanation}
            onDismiss={() => setNlpExplanation(null)}
            isAiOffline={isAiOffline}
            onDismissOffline={() => setIsAiOffline(false)}
          />
          <StatisticsViewDrawer />
        </main>
      </div>
    </ErrorBoundary>
  );
};

export default App;
