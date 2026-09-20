import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';
import { Slider } from '@/components/ui/slider';

export const VolatilityEstimatorNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
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
      label={`Volatility (${period}d)`}
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={formattedVol}
      metricLabel="Annualized Vol"
    >
      <div className="space-y-2 pt-1">
        <div className="flex items-center justify-between text-[11px] text-muted-foreground">
          <span>Lookback: <strong className="font-mono text-card-foreground">{period}d</strong></span>
          <span className="text-[10px] text-muted-foreground font-mono">Factor: √{factor}</span>
        </div>
        <Slider
          min={5}
          max={120}
          step={5}
          value={[period]}
          onValueChange={(vals) => updateNodeParameters(id, { period: vals[0] })}
          className="pt-1"
        />
      </div>
    </BaseEntityNode>
  );
};
