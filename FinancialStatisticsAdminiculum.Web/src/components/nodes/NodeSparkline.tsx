import React from 'react';
import { LinePath, AreaClosed } from '@visx/shape';
import { scaleLinear } from '@visx/scale';
import { LinearGradient } from '@visx/gradient';
import { curveMonotoneX } from '@visx/curve';

export interface NodeSparklineProps {
  data?: number[];
  color?: string;
  height?: number;
}

export const NodeSparkline: React.FC<NodeSparklineProps> = ({
  data = [],
  color = 'hsl(var(--primary))',
  height = 36,
}) => {
  if (!data || data.length < 2) {
    return (
      <div 
        style={{ height }} 
        className="w-full bg-slate-900/50 rounded flex items-center justify-center text-[10px] text-muted-foreground font-mono"
      >
        Awaiting input stream
      </div>
    );
  }

  const width = 180;
  const padding = 4;

  const min = Math.min(...data);
  const max = Math.max(...data);

  const xScale = scaleLinear<number>({
    domain: [0, data.length - 1],
    range: [padding, width - padding],
  });

  const yScale = scaleLinear<number>({
    domain: [min, max === min ? min + 1 : max],
    range: [height - padding, padding],
  });

  const points = data.map((val, idx) => ({ x: idx, y: val }));

  return (
    <div className="w-full bg-slate-900/80 p-1 rounded border border-border/50">
      <svg width="100%" height={height} viewBox={`0 0 ${width} ${height}`} className="overflow-visible">
        <LinearGradient id="sparkline-grad" from={color} to={color} toOpacity={0} />
        <AreaClosed
          data={points}
          x={(d) => xScale(d.x)}
          y={(d) => yScale(d.y)}
          yScale={yScale}
          curve={curveMonotoneX}
          fill="url(#sparkline-grad)"
          fillOpacity={0.2}
        />
        <LinePath
          data={points}
          x={(d) => xScale(d.x)}
          y={(d) => yScale(d.y)}
          curve={curveMonotoneX}
          stroke={color}
          strokeWidth={1.5}
          strokeLinecap="round"
        />
      </svg>
    </div>
  );
};
