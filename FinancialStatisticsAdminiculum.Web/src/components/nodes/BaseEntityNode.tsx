import React, { ReactNode } from 'react';
import { Handle, Position } from '@xyflow/react';
import { EntityStatus } from '../../types/workspace';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { Trash2, Activity, Settings2 } from 'lucide-react';
import { NodeSparkline } from './NodeSparkline';

interface BaseEntityNodeProps {
  id: string;
  type: string;
  label: string;
  status: EntityStatus;
  hasInput?: boolean;
  hasOutput?: boolean;
  sparkline?: number[];
  currentMetric?: number | string;
  metricLabel?: string;
  sparklineColor?: string;
  children?: ReactNode;
}

export const BaseEntityNode: React.FC<BaseEntityNodeProps> = ({
  id,
  type,
  label,
  status,
  hasInput = true,
  hasOutput = true,
  sparkline,
  currentMetric,
  metricLabel = 'Value',
  sparklineColor = '#0ea5e9',
  children,
}) => {
  const selectedNodeId = useWorkspaceStore((s) => s.selectedNodeId);
  const selectNode = useWorkspaceStore((s) => s.selectNode);
  const removeEntity = useWorkspaceStore((s) => s.removeEntity);

  const isSelected = selectedNodeId === id;

  const statusColor = {
    Ready: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
    Computing: 'bg-sky-500/20 text-sky-400 border-sky-500/30 animate-pulse',
    Stale: 'bg-amber-500/20 text-amber-400 border-amber-500/30',
    Error: 'bg-rose-500/20 text-rose-400 border-rose-500/30',
    Warning: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
  }[status] || 'bg-slate-700 text-slate-300';

  return (
    <div
      onClick={() => selectNode(id)}
      className={`relative min-w-[220px] max-w-[260px] rounded-lg border bg-slate-900/95 text-slate-100 shadow-xl backdrop-blur transition-all duration-150 ${
        isSelected
          ? 'border-sky-500 shadow-sky-500/20 ring-1 ring-sky-500'
          : 'border-slate-800 hover:border-slate-700'
      }`}
    >
      {hasInput && (
        <Handle
          type="target"
          position={Position.Left}
          id="in_series"
          className="!h-3 !w-3 !rounded-full !border-2 !border-slate-900 !bg-sky-400 hover:!scale-125 transition-transform"
        />
      )}

      {/* Header */}
      <div className="flex items-center justify-between border-b border-slate-800/80 px-3 py-2">
        <div className="flex items-center space-x-2">
          <Activity className="h-4 w-4 text-sky-400" />
          <span className="text-xs font-semibold uppercase tracking-wider text-slate-300">
            {label}
          </span>
        </div>
        <div className="flex items-center space-x-1.5">
          <span className={`rounded-full border px-1.5 py-0.5 text-[9px] font-medium ${statusColor}`}>
            {status}
          </span>
          <button
            onClick={(e) => {
              e.stopPropagation();
              removeEntity(id);
            }}
            className="rounded p-1 text-slate-500 hover:bg-slate-800 hover:text-rose-400 transition-colors"
            title="Remove entity"
          >
            <Trash2 className="h-3.5 w-3.5" />
          </button>
        </div>
      </div>

      {/* Body Content */}
      <div className="space-y-2.5 p-3">
        {currentMetric !== undefined && (
          <div className="flex items-baseline justify-between">
            <span className="text-[11px] text-slate-400">{metricLabel}</span>
            <span className="font-mono text-sm font-bold text-slate-100">
              {typeof currentMetric === 'number' ? currentMetric.toFixed(4) : currentMetric}
            </span>
          </div>
        )}

        {sparkline && (
          <NodeSparkline data={sparkline} color={sparklineColor} height={32} />
        )}

        {children}
      </div>

      {hasOutput && (
        <Handle
          type="source"
          position={Position.Right}
          id="out_series"
          className="!h-3 !w-3 !rounded-full !border-2 !border-slate-900 !bg-emerald-400 hover:!scale-125 transition-transform"
        />
      )}
    </div>
  );
};
