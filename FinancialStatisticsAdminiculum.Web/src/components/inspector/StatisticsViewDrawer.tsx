import React, { useState } from 'react';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { DistributionPlot } from './DistributionPlot';
import { MomentsTable } from './MomentsTable';
import { FormulaTraceView } from './FormulaTraceView';
import { detectAnomalies } from '../../kernel/distributionCalculations';
import { BarChart3, X, AlertCircle } from 'lucide-react';

export const StatisticsViewDrawer: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const selectedNodeId = useWorkspaceStore((s) => s.selectedNodeId);
  const nodes = useWorkspaceStore((s) => s.nodes);

  const selectedNode = nodes.find((n) => n.id === selectedNodeId);

  if (!selectedNode) return null;

  const { type, label, parameters, calculatedValues } = selectedNode.data;
  const series = calculatedValues?.series || [];
  const anomalies = detectAnomalies(series, 2.0);

  return (
    <>
      {/* Floating Toggle Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="fixed bottom-6 right-6 z-30 flex items-center space-x-2 rounded-xl border border-sky-500/40 bg-slate-900/90 px-3.5 py-2 text-xs font-semibold text-sky-400 shadow-2xl backdrop-blur hover:bg-slate-800 transition-all"
      >
        <BarChart3 className="h-4 w-4" />
        <span>Statistics View</span>
        <span className="rounded-full bg-sky-500/20 px-1.5 py-0.5 text-[10px] font-mono">
          {series.length} pts
        </span>
      </button>

      {/* Drawer */}
      {isOpen && (
        <div className="fixed inset-y-0 right-0 z-40 w-96 border-l border-slate-800 bg-slate-900/95 p-5 shadow-2xl backdrop-blur overflow-y-auto space-y-4 animate-in slide-in-from-right duration-200">
          <div className="flex items-center justify-between border-b border-slate-800 pb-3">
            <div className="flex items-center space-x-2">
              <BarChart3 className="h-5 w-5 text-sky-400" />
              <div>
                <h3 className="text-sm font-bold text-slate-100">Statistics View</h3>
                <span className="text-[11px] text-slate-400">{label}</span>
              </div>
            </div>
            <button
              onClick={() => setIsOpen(false)}
              className="rounded p-1 text-slate-400 hover:bg-slate-800 hover:text-slate-200"
            >
              <X className="h-4 w-4" />
            </button>
          </div>

          {/* KDE Curve */}
          <DistributionPlot series={series} height={140} />

          {/* Statistical Moments */}
          <MomentsTable moments={selectedNode.data.diagnostics?.moments} />

          {/* Formula Trace */}
          <FormulaTraceView type={type} parameters={parameters} />

          {/* Anomalies section */}
          {anomalies.length > 0 && (
            <div className="rounded-lg border border-rose-500/30 bg-rose-500/10 p-3 space-y-2">
              <div className="flex items-center space-x-2 text-xs font-semibold text-rose-400">
                <AlertCircle className="h-4 w-4" />
                <span>Detected Statistical Anomalies ({anomalies.length})</span>
              </div>
              <div className="max-h-28 overflow-y-auto space-y-1 font-mono text-[10px]">
                {anomalies.slice(0, 5).map((a, i) => (
                  <div key={i} className="flex justify-between text-rose-300">
                    <span>{a.timestamp}:</span>
                    <span className="font-bold">{a.reason}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </>
  );
};
