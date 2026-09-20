import React from 'react';
import { Node, NodeProps } from '@xyflow/react';
import { BaseEntityNode } from './BaseEntityNode';
import { WorkspaceNodeData, useWorkspaceStore } from '../../store/workspaceStore';
import { Slider } from '@/components/ui/slider';

export const DistributionAnalyzerNode: React.FC<NodeProps<Node<WorkspaceNodeData>>> = ({ id, data }) => {
  const nodeData = data as WorkspaceNodeData;
  const updateNodeParameters = useWorkspaceStore((s) => s.updateNodeParameters);

  const numBins = Number(nodeData.parameters?.numBins || 40);
  const confidence = Number(nodeData.parameters?.confidenceLevel || 0.95);
  const moments = nodeData.diagnostics?.moments;
  const sparkline = nodeData.calculatedValues?.sparkline;
  const currentVal = nodeData.calculatedValues?.metrics?.current;

  return (
    <BaseEntityNode
      id={id}
      label="Distribution Analyzer"
      status={nodeData.status}
      hasInput={true}
      hasOutput={true}
      sparkline={sparkline}
      currentMetric={currentVal !== undefined ? `Z: ${currentVal.toFixed(2)}` : '---'}
      metricLabel="Latest Z-Score"
    >
      <div className="space-y-2 pt-1">
        <div className="flex items-center justify-between text-[11px] text-muted-foreground">
          <span>Bins: <strong className="font-mono text-card-foreground">{numBins}</strong></span>
          <span className="text-[10px] text-muted-foreground font-mono">CI: {(confidence * 100).toFixed(0)}%</span>
        </div>
        <Slider
          min={10}
          max={100}
          step={5}
          value={[numBins]}
          onValueChange={(vals) => updateNodeParameters(id, { numBins: vals[0] })}
          className="pt-1"
        />
        {moments && (
          <div className="grid grid-cols-2 gap-1 text-[10px] font-mono text-muted-foreground pt-1 border-t border-border">
            <div>μ: <span className="text-card-foreground">{moments.mean.toFixed(2)}</span></div>
            <div>σ: <span className="text-card-foreground">{moments.stdDev.toFixed(2)}</span></div>
            <div>Skew: <span className="text-card-foreground">{moments.skewness.toFixed(2)}</span></div>
            <div>Kurt: <span className="text-card-foreground">{moments.kurtosis.toFixed(2)}</span></div>
          </div>
        )}
      </div>
    </BaseEntityNode>
  );
};
