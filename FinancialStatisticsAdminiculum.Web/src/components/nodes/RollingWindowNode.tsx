import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';
import { Slider } from '@/components/ui/slider';

export const RollingWindowNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const windowSize = Number(nodeData.parameters?.windowSize || 30);
  const currentVal = nodeData.calculatedValues?.metrics?.current;
  const sparkline = nodeData.calculatedValues?.sparkline;

  return (
    <BaseEntityNode
      id={id}
      label={`Rolling Window (${windowSize}d)`}
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={currentVal !== undefined ? currentVal.toFixed(2) : '---'}
      metricLabel="Window Mean"
    >
      <div className="space-y-2 pt-1">
        <div className="flex items-center justify-between text-[11px] text-muted-foreground">
          <span>Window: <strong className="font-mono text-card-foreground">{windowSize}d</strong></span>
        </div>
        <Slider
          min={5}
          max={120}
          step={5}
          value={[windowSize]}
          onValueChange={(vals) => updateNodeParameters(id, { windowSize: vals[0] })}
          className="pt-1"
        />
      </div>
    </BaseEntityNode>
  );
};
