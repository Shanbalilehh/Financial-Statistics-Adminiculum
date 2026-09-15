import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';

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
      sparklineColor="#a855f7"
    >
      <div className="space-y-1.5 pt-1">
        <div className="flex items-center justify-between text-[11px] text-slate-400">
          <span>Period: <strong className="font-mono text-slate-200">{period}</strong></span>
          <div className="flex space-x-1">
            {['SMA', 'EMA'].map((m) => (
              <button
                key={m}
                onClick={() => updateNodeParameters(id, { method: m })}
                className={`px-1.5 py-0.5 rounded text-[10px] font-mono font-medium ${
                  method === m
                    ? 'bg-purple-600 text-white'
                    : 'bg-slate-800 text-slate-400 hover:text-slate-200'
                }`}
              >
                {m}
              </button>
            ))}
          </div>
        </div>
        <input
          type="range"
          min="5"
          max="200"
          step="5"
          value={period}
          onChange={(e) => updateNodeParameters(id, { period: Number(e.target.value) })}
          className="w-full accent-purple-500 cursor-pointer h-1.5 bg-slate-800 rounded-lg appearance-none"
        />
      </div>
    </BaseEntityNode>
  );
};
