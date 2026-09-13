import React from 'react';
import { NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';

export const SignalTriggerNode: React.FC<NodeProps<any>> = ({ id, data }) => {
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
      type="SignalTrigger"
      label="Signal Trigger"
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={isTriggered ? 'ACTIVE ALERT' : 'Normal'}
      metricLabel="Trigger State"
      sparklineColor={isTriggered ? '#ef4444' : '#10b981'}
    >
      <div className="space-y-1.5 pt-1">
        <div className="flex items-center justify-between text-[11px] text-slate-400">
          <span>{condition === 'GreaterThan' ? '> Threshold' : '< Threshold'}</span>
          <span className="font-mono text-slate-200">{threshold}</span>
        </div>
        <div className="flex space-x-1">
          {['GreaterThan', 'LessThan'].map((c) => (
            <button
              key={c}
              onClick={() => updateNodeParameters(id, { condition: c })}
              className={`flex-1 py-1 rounded text-[10px] font-mono ${
                condition === c
                  ? 'bg-rose-600 text-white font-semibold'
                  : 'bg-slate-800 text-slate-400 hover:text-slate-200'
              }`}
            >
              {c === 'GreaterThan' ? 'Val > X' : 'Val < X'}
            </button>
          ))}
        </div>
        <input
          type="number"
          step="0.5"
          value={threshold}
          onChange={(e) => updateNodeParameters(id, { threshold: Number(e.target.value) })}
          className="w-full rounded bg-slate-800 border border-slate-700 px-2 py-1 text-xs font-mono text-slate-200 focus:border-rose-500 focus:outline-none"
        />
      </div>
    </BaseEntityNode>
  );
};
