import React, { ReactNode } from 'react';
import { Handle, Position } from '@xyflow/react';
import { EntityStatus } from '../../types/workspace';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { Trash2, Activity } from 'lucide-react';
import { NodeSparkline } from './NodeSparkline';
import { Badge } from '@/components/ui/badge';

interface BaseEntityNodeProps {
  id: string;
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

  const statusVariant: 'default' | 'secondary' | 'destructive' | 'outline' =
    status === 'Error'
      ? 'destructive'
      : status === 'Ready'
      ? 'outline'
      : 'secondary';

  return (
    <div
      onClick={() => selectNode(id)}
      className={`relative min-w-[220px] max-w-[260px] rounded-lg border bg-card text-card-foreground shadow-xl backdrop-blur transition-all duration-150 ${
        isSelected
          ? 'border-primary shadow-primary/20 ring-1 ring-primary'
          : 'border-border hover:border-muted-foreground/50'
      }`}
    >
      {hasInput && (
        <Handle
          type="target"
          position={Position.Left}
          id="in_series"
          className="!h-3 !w-3 !rounded-full !border-2 !border-card !bg-primary hover:!scale-125 transition-transform"
        />
      )}

      {/* Header */}
      <div className="flex items-center justify-between border-b border-border px-3 py-2">
        <div className="flex items-center space-x-2">
          <Activity className="h-4 w-4 text-primary" />
          <span className="text-xs font-semibold uppercase tracking-wider text-card-foreground">
            {label}
          </span>
        </div>
        <div className="flex items-center space-x-1.5">
          <Badge variant={statusVariant} className="text-[9px] px-1.5 py-0">
            {status}
          </Badge>
          <button
            onClick={(e) => {
              e.stopPropagation();
              removeEntity(id);
            }}
            className="rounded p-1 text-muted-foreground hover:bg-accent hover:text-destructive transition-colors"
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
            <span className="text-[11px] text-muted-foreground">{metricLabel}</span>
            <span className="font-mono text-sm font-bold text-card-foreground">
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
          className="!h-3 !w-3 !rounded-full !border-2 !border-card !bg-primary hover:!scale-125 transition-transform"
        />
      )}
    </div>
  );
};
