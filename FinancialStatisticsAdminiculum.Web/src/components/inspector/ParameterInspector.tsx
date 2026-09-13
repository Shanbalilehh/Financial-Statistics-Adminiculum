import React from 'react';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { X, Sliders, Info } from 'lucide-react';

export const ParameterInspector: React.FC = () => {
  const selectedNodeId = useWorkspaceStore((s) => s.selectedNodeId);
  const selectNode = useWorkspaceStore((s) => s.selectNode);
  const nodes = useWorkspaceStore((s) => s.nodes);
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const selectedNode = nodes.find((n) => n.id === selectedNodeId);

  if (!selectedNode) return null;

  const { type, label, parameters, calculatedValues } = selectedNode.data;

  return (
    <div className="absolute right-4 top-20 z-20 w-80 rounded-xl border border-slate-800 bg-slate-900/95 p-4 shadow-2xl backdrop-blur">
      <div className="flex items-center justify-between border-b border-slate-800 pb-3">
        <div className="flex items-center space-x-2">
          <Sliders className="h-4 w-4 text-sky-400" />
          <h3 className="font-semibold text-sm text-slate-200">Entity Parameters</h3>
        </div>
        <button
          onClick={() => selectNode(null)}
          className="rounded p-1 text-slate-400 hover:bg-slate-800 hover:text-slate-200"
        >
          <X className="h-4 w-4" />
        </button>
      </div>

      <div className="space-y-4 pt-3 text-xs">
        <div>
          <span className="text-[11px] text-slate-500 uppercase tracking-wider">Type</span>
          <p className="font-mono text-slate-300 font-semibold">{type}</p>
        </div>

        <div>
          <span className="text-[11px] text-slate-500 uppercase tracking-wider">Label</span>
          <p className="text-slate-200 font-medium">{label}</p>
        </div>

        {/* Dynamic Parameter Fields */}
        <div className="space-y-3 rounded-lg bg-slate-950/60 p-3 border border-slate-800/80">
          <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Algorithmic Controls
          </span>

          {Object.entries(parameters).map(([key, val]) => (
            <div key={key} className="space-y-1">
              <label className="text-[11px] text-slate-400 capitalize">{key}:</label>
              {typeof val === 'number' ? (
                <input
                  type="number"
                  value={val}
                  onChange={(e) =>
                    updateNodeParameters(selectedNode.id, {
                      [key]: Number(e.target.value),
                    })
                  }
                  className="w-full rounded bg-slate-900 border border-slate-700 px-2 py-1 font-mono text-slate-200 focus:border-sky-500 focus:outline-none"
                />
              ) : (
                <input
                  type="text"
                  value={String(val)}
                  onChange={(e) =>
                    updateNodeParameters(selectedNode.id, {
                      [key]: e.target.value,
                    })
                  }
                  className="w-full rounded bg-slate-900 border border-slate-700 px-2 py-1 font-mono text-slate-200 focus:border-sky-500 focus:outline-none"
                />
              )}
            </div>
          ))}
        </div>

        {/* Latest Observation Values */}
        {calculatedValues?.metrics && (
          <div className="rounded-lg bg-slate-950/60 p-3 border border-slate-800/80 space-y-1.5 font-mono">
            <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Live Numerical State
            </span>
            {Object.entries(calculatedValues.metrics).map(([k, v]) => (
              <div key={k} className="flex justify-between text-slate-300">
                <span className="capitalize">{k}:</span>
                <span className="font-bold text-sky-400">{typeof v === 'number' ? v.toFixed(4) : v}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
