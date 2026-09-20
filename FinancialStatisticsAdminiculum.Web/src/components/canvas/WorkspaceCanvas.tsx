import React, { useCallback, useMemo } from 'react';
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  applyNodeChanges,
  applyEdgeChanges,
  NodeChange,
  EdgeChange,
  Connection,
  BackgroundVariant,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';

import { useWorkspaceStore } from '../../store/workspaceStore';
import { PriceStreamNode } from '../nodes/PriceStreamNode';
import { RollingWindowNode } from '../nodes/RollingWindowNode';
import { MovingAverageNode } from '../nodes/MovingAverageNode';
import { VolatilityEstimatorNode } from '../nodes/VolatilityEstimatorNode';
import { DistributionAnalyzerNode } from '../nodes/DistributionAnalyzerNode';
import { CorrelationMatrixNode } from '../nodes/CorrelationMatrixNode';
import { SignalTriggerNode } from '../nodes/SignalTriggerNode';
import { EntityType } from '../../types/workspace';
import { TrendingUp, Cpu, AlertTriangle, Activity, Sliders, BarChart2, Network } from 'lucide-react';

export const WorkspaceCanvas: React.FC = () => {
  const nodes = useWorkspaceStore((s) => s.nodes);
  const edges = useWorkspaceStore((s) => s.edges);
  const addEntity = useWorkspaceStore((s) => s.addEntity);
  const addConnection = useWorkspaceStore((s) => s.addConnection);
  const selectNode = useWorkspaceStore((s) => s.selectNode);

  const nodeTypes = useMemo(
    () => ({
      PriceStream: PriceStreamNode,
      RollingWindow: RollingWindowNode,
      MovingAverage: MovingAverageNode,
      VolatilityEstimator: VolatilityEstimatorNode,
      DistributionAnalyzer: DistributionAnalyzerNode,
      CorrelationMatrix: CorrelationMatrixNode,
      SignalTrigger: SignalTriggerNode,
      CustomTransform: MovingAverageNode,
    }),
    []
  );

  const onNodesChange = useCallback(
    (changes: NodeChange<any>[]) => {
      useWorkspaceStore.setState((state) => ({
        nodes: applyNodeChanges(changes, state.nodes),
      }));
    },
    []
  );

  const onEdgesChange = useCallback(
    (changes: EdgeChange<any>[]) => {
      useWorkspaceStore.setState((state) => ({
        edges: applyEdgeChanges(changes, state.edges),
      }));
    },
    []
  );

  const onConnect = useCallback(
    (connection: Connection) => {
      addConnection(connection);
    },
    [addConnection]
  );

  const handleAddNode = (type: EntityType) => {
    // Offset slightly for each new entity
    const offset = (nodes.length % 5) * 40;
    addEntity(type, { x: 220 + offset, y: 150 + offset });
  };

  return (
    <div className="relative h-full w-full bg-slate-950">
      {/* Palette Toolbar */}
      <div className="absolute top-4 left-4 z-10 flex items-center space-x-2 rounded-xl border border-slate-800 bg-slate-900/90 p-1.5 shadow-2xl backdrop-blur">
        <span className="px-2 text-[11px] font-semibold tracking-wider text-slate-400 uppercase">
          Add Entity:
        </span>
        <button
          onClick={() => handleAddNode('PriceStream')}
          className="flex items-center space-x-1.5 rounded-lg bg-sky-500/10 border border-sky-500/20 px-2.5 py-1 text-xs font-medium text-sky-400 hover:bg-sky-500/20 transition-all"
        >
          <TrendingUp className="h-3.5 w-3.5" />
          <span>Price Stream</span>
        </button>
        <button
          onClick={() => handleAddNode('RollingWindow')}
          className="flex items-center space-x-1.5 rounded-lg bg-emerald-500/10 border border-emerald-500/20 px-2.5 py-1 text-xs font-medium text-emerald-400 hover:bg-emerald-500/20 transition-all"
        >
          <Sliders className="h-3.5 w-3.5" />
          <span>Rolling Window</span>
        </button>
        <button
          onClick={() => handleAddNode('MovingAverage')}
          className="flex items-center space-x-1.5 rounded-lg bg-purple-500/10 border border-purple-500/20 px-2.5 py-1 text-xs font-medium text-purple-400 hover:bg-purple-500/20 transition-all"
        >
          <Activity className="h-3.5 w-3.5" />
          <span>Moving Average</span>
        </button>
        <button
          onClick={() => handleAddNode('VolatilityEstimator')}
          className="flex items-center space-x-1.5 rounded-lg bg-amber-500/10 border border-amber-500/20 px-2.5 py-1 text-xs font-medium text-amber-400 hover:bg-amber-500/20 transition-all"
        >
          <Cpu className="h-3.5 w-3.5" />
          <span>Volatility</span>
        </button>
        <button
          onClick={() => handleAddNode('DistributionAnalyzer')}
          className="flex items-center space-x-1.5 rounded-lg bg-cyan-500/10 border border-cyan-500/20 px-2.5 py-1 text-xs font-medium text-cyan-400 hover:bg-cyan-500/20 transition-all"
        >
          <BarChart2 className="h-3.5 w-3.5" />
          <span>Distribution</span>
        </button>
        <button
          onClick={() => handleAddNode('CorrelationMatrix')}
          className="flex items-center space-x-1.5 rounded-lg bg-pink-500/10 border border-pink-500/20 px-2.5 py-1 text-xs font-medium text-pink-400 hover:bg-pink-500/20 transition-all"
        >
          <Network className="h-3.5 w-3.5" />
          <span>Correlation</span>
        </button>
        <button
          onClick={() => handleAddNode('SignalTrigger')}
          className="flex items-center space-x-1.5 rounded-lg bg-rose-500/10 border border-rose-500/20 px-2.5 py-1 text-xs font-medium text-rose-400 hover:bg-rose-500/20 transition-all"
        >
          <AlertTriangle className="h-3.5 w-3.5" />
          <span>Signal Trigger</span>
        </button>
      </div>

      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onPaneClick={() => selectNode(null)}
        fitView
        className="bg-slate-950"
      >
        <Background variant={BackgroundVariant.Dots} gap={24} size={1} color="#334155" />
        <Controls className="!bg-slate-900 !border-slate-800 !text-slate-300 [&>button]:!border-slate-800" />
        <MiniMap
          nodeStrokeWidth={3}
          nodeColor={(n) => {
            if (n.type === 'PriceStream') return '#38bdf8';
            if (n.type === 'RollingWindow') return '#10b981';
            if (n.type === 'MovingAverage') return '#a855f7';
            if (n.type === 'VolatilityEstimator') return '#f59e0b';
            if (n.type === 'DistributionAnalyzer') return '#06b6d4';
            if (n.type === 'CorrelationMatrix') return '#ec4899';
            if (n.type === 'SignalTrigger') return '#ef4444';
            return '#64748b';
          }}
          className="!bg-slate-900 !border-slate-800"
        />
      </ReactFlow>
    </div>
  );
};
