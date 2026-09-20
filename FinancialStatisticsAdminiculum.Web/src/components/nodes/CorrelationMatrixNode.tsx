import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';

export const CorrelationMatrixNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const benchmark = (nodeData.parameters?.benchmarkSymbol as string) || 'SPY';
  const currentVal = nodeData.calculatedValues?.metrics?.current;
  const sparkline = nodeData.calculatedValues?.sparkline;
  const formattedR = currentVal !== undefined ? `r = ${currentVal.toFixed(3)}` : '---';

  return (
    <BaseEntityNode
      id={id}
      label={`Correlation (${benchmark})`}
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={formattedR}
      metricLabel="Correlation Coeff"
      sparklineColor="#ec4899"
    >
      <div className="space-y-1.5 pt-1">
        <div className="flex items-center justify-between text-[11px] text-muted-foreground">
          <span>Benchmark:</span>
          <select
            value={benchmark}
            onChange={(e) => updateNodeParameters(id, { benchmarkSymbol: e.target.value })}
            className="rounded bg-muted border border-border px-1.5 py-0.5 text-[10px] font-mono text-card-foreground focus:outline-none focus:border-primary"
          >
            {['SPY', 'QQQ', 'DIA', 'IWM', 'BTC-USD'].map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>
      </div>
    </BaseEntityNode>
  );
};
