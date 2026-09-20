import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';
import { Slider } from '@/components/ui/slider';

export const MovingAverageNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const period = Number(nodeData.parameters?.period || 20);
  const method = (nodeData.parameters?.method as string) || 'SMA';
  const currentVal = nodeData.calculatedValues?.metrics?.current;
  const sparkline = nodeData.calculatedValues?.sparkline;

  return (
    <BaseEntityNode
      id={id}
      label={`${method} (${period})`}
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={currentVal}
      metricLabel={`${method} Value`}
    >
      <div className="space-y-2 pt-1">
        <div className="flex items-center justify-between text-[11px] text-muted-foreground">
          <span>Period: <strong className="font-mono text-card-foreground">{period}</strong></span>
          <div className="flex space-x-1">
            {['SMA', 'EMA'].map((m) => (
              <button
                key={m}
                onClick={() => updateNodeParameters(id, { method: m })}
                className={`px-1.5 py-0.5 rounded text-[10px] font-mono font-medium transition-colors ${
                  method === m
                    ? 'bg-primary text-primary-foreground'
                    : 'bg-muted text-muted-foreground hover:text-foreground'
                }`}
              >
                {m}
              </button>
            ))}
          </div>
        </div>
        <Slider
          min={5}
          max={200}
          step={5}
          value={[period]}
          onValueChange={(vals) => updateNodeParameters(id, { period: vals[0] })}
          className="pt-1"
        />
      </div>
    </BaseEntityNode>
  );
};
