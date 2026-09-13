import React from 'react';
import { NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';

export const VolatilityEstimatorNode: React.FC<NodeProps<any>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const period = Number(nodeData.parameters?.period || 30);
  const factor = Number(nodeData.parameters?.annualizationFactor || 252);
  const currentVal = nodeData.calculatedValues?.metrics?.current;
  const formattedVol = currentVal !== undefined ? `${(currentVal * 100).toFixed(2)}%` : '---';
  const sparkline = nodeData.calculatedValues?.sparkline;

  return (
    <BaseEntityNode
      id={id}
      type="VolatilityEstimator"
      label={`Volatility (${period}d)`}
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={formattedVol}
      metricLabel="Annualized Vol"
      sparklineColor="#f59e0b"
    >
      <div className="space-y-1.5 pt-1">
        <div className="flex items-center justify-between text-[11px] text-slate-400">
          <span>Lookback: <strong className="font-mono text-slate-200">{period}d</strong></span>
          <span className="text-[10px] text-slate-500 font-mono">Factor: √{factor}</span>
        </div>
        <input
          type="range"
          min="5"
          max="120"
          step="5"
          value={period}
          onChange={(e) => updateNodeParameters(id, { period: Number(e.target.value) })}
          className="w-full accent-amber-500 cursor-pointer h-1.5 bg-slate-800 rounded-lg appearance-none"
        />
      </div>
    </BaseEntityNode>
  );
};
