import React, { useEffect } from 'react';
import { WorkspaceCanvas } from './components/canvas/WorkspaceCanvas';
import { ParameterInspector } from './components/inspector/ParameterInspector';
import { useWorkspaceStore } from './store/workspaceStore';
import { RotateCcw, Undo, Redo, LayoutGrid, Terminal } from 'lucide-react';

import { NlpCommandBar } from './components/nlp/NlpCommandBar';
import { NlpAuditBadge } from './components/nlp/NlpAuditBadge';
import { StatisticsViewDrawer } from './components/inspector/StatisticsViewDrawer';
import { ExportMenu } from './components/export/ExportMenu';

export const App: React.FC = () => {
  const [nlpExplanation, setNlpExplanation] = React.useState<string | null>(null);
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
      const priceId = addEntity('PriceStream', { x: 120, y: 180 }, { symbol: 'AAPL', lookback: 100 });
      addEntity('MovingAverage', { x: 440, y: 180 }, { period: 20, method: 'SMA' });
    }
  }, []);

  return (
    <div className="flex h-screen w-screen flex-col bg-slate-950 font-sans text-slate-100 overflow-hidden">
      {/* Top Header */}
      <header className="flex h-14 items-center justify-between border-b border-slate-800/80 bg-slate-900/80 px-4 backdrop-blur z-20">
        <div className="flex items-center space-x-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-sky-500/20 border border-sky-500/30 text-sky-400">
            <LayoutGrid className="h-4 w-4" />
          </div>
          <div className="flex items-baseline space-x-2">
            <span className="text-sm font-bold tracking-tight text-slate-100">
              Financial Statistics Adminiculum
            </span>
            <span className="text-[10px] rounded bg-slate-800 px-1.5 py-0.5 text-slate-400 font-mono">
              v1.0.0
            </span>
          </div>
          <div className="h-4 w-[1px] bg-slate-800" />
          <input
            type="text"
            value={workspaceName}
            onChange={(e) => setWorkspaceName(e.target.value)}
            className="rounded bg-transparent px-2 py-1 text-xs font-medium text-slate-300 hover:bg-slate-800/50 focus:bg-slate-800 focus:outline-none"
          />
        </div>

        {/* Global Canvas Actions */}
        <div className="flex items-center space-x-2">
          <NlpCommandBar onMutationApplied={(expl) => setNlpExplanation(expl)} />

          <div className="flex items-center space-x-1 rounded-lg border border-slate-800 bg-slate-900/60 p-1">
            <button
              onClick={undo}
              title="Undo (Ctrl+Z)"
              className="rounded p-1 text-slate-400 hover:bg-slate-800 hover:text-slate-200 transition-colors"
            >
              <Undo className="h-4 w-4" />
            </button>
            <button
              onClick={redo}
              title="Redo (Ctrl+Y)"
              className="rounded p-1 text-slate-400 hover:bg-slate-800 hover:text-slate-200 transition-colors"
            >
              <Redo className="h-4 w-4" />
            </button>
            <div className="h-3 w-[1px] bg-slate-800" />
            <button
              onClick={resetWorkspace}
              title="Clear Canvas"
              className="rounded p-1 text-slate-400 hover:bg-slate-800 hover:text-rose-400 transition-colors"
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
        <NlpAuditBadge message={nlpExplanation} onDismiss={() => setNlpExplanation(null)} />
        <StatisticsViewDrawer />
      </main>
    </div>
  );
};

export default App;
