import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';
import { Input } from '@/components/ui/input';

export const SignalTriggerNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const threshold = Number(nodeData.parameters?.threshold || 25);
  const condition = (nodeData.parameters?.condition as string) || 'GreaterThan';
  const currentVal = nodeData.calculatedValues?.metrics?.current;
  const isTriggered = currentVal === 1;
  const sparkline = nodeData.calculatedValues?.sparkline;

  return (
    <BaseEntityNode
      id={id}
      label="Signal Trigger"
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={isTriggered ? 'ACTIVE ALERT' : 'Normal'}
      metricLabel="Trigger State"
    >
      <div className="space-y-2 pt-1">
        <div className="flex items-center justify-between text-[11px] text-muted-foreground">
          <span>{condition === 'GreaterThan' ? '> Threshold' : '< Threshold'}</span>
          <span className="font-mono text-card-foreground">{threshold}</span>
        </div>
        <div className="flex space-x-1">
          {['GreaterThan', 'LessThan'].map((c) => (
            <button
              key={c}
              onClick={() => updateNodeParameters(id, { condition: c })}
              className={`flex-1 py-1 rounded text-[10px] font-mono transition-colors ${
                condition === c
                  ? 'bg-destructive text-destructive-foreground font-semibold'
                  : 'bg-muted text-muted-foreground hover:text-foreground'
              }`}
            >
              {c === 'GreaterThan' ? 'Val > X' : 'Val < X'}
            </button>
          ))}
        </div>
        <Input
          type="number"
          step="0.5"
          value={threshold}
          onChange={(e) => updateNodeParameters(id, { threshold: Number(e.target.value) })}
          className="h-7 text-xs font-mono"
        />
      </div>
    </BaseEntityNode>
  );
};
