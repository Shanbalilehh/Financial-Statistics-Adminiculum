import React from 'react';
import { Group } from '@visx/group';
import { LinePath, AreaClosed, Bar } from '@visx/shape';
import { scaleLinear } from '@visx/scale';
import { curveBasis } from '@visx/curve';
import { LinearGradient } from '@visx/gradient';
import { AxisBottom, AxisLeft } from '@visx/axis';
import { GridRows, GridColumns } from '@visx/grid';
import { computeKde, KdePoint } from '../../kernel/distributionCalculations';

export interface DistributionPlotProps {
  series?: number[];
  height?: number;
  width?: number;
}

export const DistributionPlot: React.FC<DistributionPlotProps> = ({
  series = [],
  height = 180,
  width = 300,
}) => {
  if (!series || series.length < 3) {
    return (
      <div 
        style={{ height }}
        className="flex items-center justify-center rounded-lg border border-border/50 bg-card text-xs text-muted-foreground font-mono"
      >
        Insufficient observations for distribution plot (need ≥ 3)
      </div>
    );
  }

  const margin = { top: 16, right: 16, bottom: 28, left: 36 };
  const innerWidth = Math.max(10, width - margin.left - margin.right);
  const innerHeight = Math.max(10, height - margin.top - margin.bottom);

  // Compute KDE points
  const kdePoints: KdePoint[] = computeKde(series, 40);
  const minX = Math.min(...kdePoints.map((p) => p.x));
  const maxX = Math.max(...kdePoints.map((p) => p.x));
  const maxDensity = Math.max(...kdePoints.map((p) => p.density)) || 1;

  // Compute simple histogram bins (10 bins)
  const numBins = 10;
  const binWidth = (maxX - minX) / numBins || 1;
  const bins = Array.from({ length: numBins }, (_, i) => ({
    x0: minX + i * binWidth,
    x1: minX + (i + 1) * binWidth,
    count: 0,
  }));

  series.forEach((val) => {
    const idx = Math.min(numBins - 1, Math.max(0, Math.floor((val - minX) / binWidth)));
    if (bins[idx]) bins[idx].count++;
  });

  const maxBinCount = Math.max(...bins.map((b) => b.count)) || 1;

  // Visx scales
  const xScale = scaleLinear<number>({
    domain: [minX, maxX],
    range: [0, innerWidth],
  });

  const yScale = scaleLinear<number>({
    domain: [0, maxDensity * 1.1],
    range: [innerHeight, 0],
  });

  const yHistScale = scaleLinear<number>({
    domain: [0, maxBinCount],
    range: [innerHeight, 0],
  });

  return (
    <div className="space-y-1.5 rounded-lg border border-border/60 bg-card/80 p-3">
      <div className="flex items-center justify-between text-[11px] text-muted-foreground">
        <span className="font-semibold uppercase tracking-wider">Empirical Distribution (KDE & Histogram)</span>
        <span className="font-mono text-[10px] text-primary">React Visx Pure SVG</span>
      </div>

      <div className="relative overflow-hidden rounded bg-slate-950/80 p-1 border border-border/40">
        <svg width="100%" height={height} viewBox={`0 0 ${width} ${height}`} className="overflow-visible">
          <LinearGradient id="visx-kde-gradient" from="hsl(var(--primary))" to="hsl(var(--primary))" toOpacity={0} />

          <Group left={margin.left} top={margin.top}>
            {/* Background Grid */}
            <GridRows scale={yScale} width={innerWidth} stroke="hsl(var(--border))" strokeOpacity={0.25} />
            <GridColumns scale={xScale} height={innerHeight} stroke="hsl(var(--border))" strokeOpacity={0.25} />

            {/* Binned Histogram Bars */}
            {bins.map((bin, i) => {
              const barX = xScale(bin.x0);
              const barWidth = Math.max(0, xScale(bin.x1) - barX - 1);
              const barHeight = innerHeight - yHistScale(bin.count);
              return (
                <Bar
                  key={`hist-bar-${i}`}
                  x={barX}
                  y={innerHeight - barHeight}
                  width={barWidth}
                  height={barHeight}
                  fill="hsl(var(--muted-foreground))"
                  fillOpacity={0.25}
                  stroke="hsl(var(--border))"
                  strokeWidth={0.5}
                />
              );
            })}

            {/* KDE Area */}
            <AreaClosed<KdePoint>
              data={kdePoints}
              x={(d) => xScale(d.x)}
              y={(d) => yScale(d.density)}
              yScale={yScale}
              curve={curveBasis}
              fill="url(#visx-kde-gradient)"
              fillOpacity={0.4}
            />

            {/* KDE Line Path */}
            <LinePath<KdePoint>
              data={kdePoints}
              x={(d) => xScale(d.x)}
              y={(d) => yScale(d.density)}
              curve={curveBasis}
              stroke="hsl(var(--primary))"
              strokeWidth={2}
              strokeLinecap="round"
            />

            {/* Axes */}
            <AxisBottom
              top={innerHeight}
              scale={xScale}
              numTicks={4}
              stroke="hsl(var(--border))"
              tickStroke="hsl(var(--border))"
              tickLabelProps={() => ({
                fill: 'hsl(var(--muted-foreground))',
                fontSize: 9,
                textAnchor: 'middle',
              })}
            />
            <AxisLeft
              scale={yScale}
              numTicks={3}
              stroke="hsl(var(--border))"
              tickStroke="hsl(var(--border))"
              tickLabelProps={() => ({
                fill: 'hsl(var(--muted-foreground))',
                fontSize: 9,
                textAnchor: 'end',
                dx: -4,
                dy: 3,
              })}
            />
          </Group>
        </svg>

        <div className="flex justify-between text-[9px] font-mono text-muted-foreground pt-1">
          <span>Min: {minX.toFixed(2)}</span>
          <span>Max: {maxX.toFixed(2)}</span>
        </div>
      </div>
    </div>
  );
};
