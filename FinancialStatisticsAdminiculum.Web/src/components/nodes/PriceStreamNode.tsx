import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';

export const PriceStreamNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const symbol = (nodeData.parameters?.symbol as string) || 'AAPL';
  const lookback = (nodeData.parameters?.lookback as number) || 252;
  const currentPrice = nodeData.calculatedValues?.metrics?.current;
  const sparkline = nodeData.calculatedValues?.sparkline;

  const handleSymbolChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    updateNodeParameters(id, { symbol: e.target.value });
  };

  return (
    <BaseEntityNode
      id={id}
      label={`Asset: ${symbol}`}
      status={nodeData.status}
      hasInput={false}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={currentPrice}
      metricLabel="Latest Close"
      sparklineColor="#38bdf8"
    >
      <div className="flex items-center justify-between space-x-2 pt-1">
        <select
          value={symbol}
          onChange={handleSymbolChange}
          className="w-full rounded bg-slate-800 border border-slate-700 px-2 py-1 text-xs font-mono text-slate-200 focus:border-sky-500 focus:outline-none"
        >
          <option value="AAPL">AAPL (Apple)</option>
          <option value="MSFT">MSFT (Microsoft)</option>
          <option value="SPY">SPY (S&P 500)</option>
          <option value="QQQ">QQQ (Nasdaq 100)</option>
          <option value="BTC-USD">BTC-USD (Bitcoin)</option>
        </select>
      </div>
      <div className="flex justify-between text-[10px] text-slate-500 font-mono">
        <span>Window: {lookback}d</span>
        <span>Interval: 1d</span>
      </div>
    </BaseEntityNode>
  );
};
