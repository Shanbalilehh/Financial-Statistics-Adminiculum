import React from 'react';
import { computeKde, computeCdf } from '../../kernel/distributionCalculations';

interface DistributionPlotProps {
  series?: number[];
  height?: number;
}

export const DistributionPlot: React.FC<DistributionPlotProps> = ({
  series = [],
  height = 140,
}) => {
  if (!series || series.length < 3) {
    return (
      <div 
        style={{ height }}
        className="flex items-center justify-center rounded-lg border border-slate-800 bg-slate-950/60 text-xs text-slate-500 font-mono"
      >
        Insufficient observations for distribution plot (need ≥ 3)
      </div>
    );
  }

  const kdePoints = computeKde(series, 40);
  const maxDensity = Math.max(...kdePoints.map((p) => p.density)) || 1;
  const minX = Math.min(...kdePoints.map((p) => p.x));
  const maxX = Math.max(...kdePoints.map((p) => p.x));
  const rangeX = maxX - minX || 1;

  const width = 280;

  // Build SVG path for KDE curve
  const pointsStr = kdePoints
    .map((pt, idx) => {
      const x = ((pt.x - minX) / rangeX) * width;
      const y = height - (pt.density / maxDensity) * (height - 20) - 10;
      return `${idx === 0 ? 'M' : 'L'} ${x.toFixed(1)} ${y.toFixed(1)}`;
    })
    .join(' ');

  // Fill area path closing down to bottom
  const areaPath = `${pointsStr} L ${width} ${height - 10} L 0 ${height - 10} Z`;

  return (
    <div className="space-y-1.5 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
      <div className="flex items-center justify-between text-[11px] text-slate-400">
        <span className="font-semibold uppercase tracking-wider">Empirical Probability Density (KDE)</span>
        <span className="font-mono text-[10px] text-sky-400">Gaussian Kernel</span>
      </div>

      <div className="relative overflow-hidden rounded bg-slate-900/80 p-1 border border-slate-800">
        <svg width="100%" height={height} viewBox={`0 0 ${width} ${height}`} className="overflow-visible">
          {/* Shaded Area */}
          <path d={areaPath} fill="url(#kdeGradient)" opacity="0.35" />
          
          {/* Density Line */}
          <path
            d={pointsStr}
            fill="none"
            stroke="#38bdf8"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />

          <defs>
            <linearGradient id="kdeGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#38bdf8" />
              <stop offset="100%" stopColor="#0369a1" stopOpacity="0" />
            </linearGradient>
          </defs>
        </svg>

        <div className="flex justify-between text-[9px] font-mono text-slate-500 pt-1">
          <span>Min: {minX.toFixed(2)}</span>
          <span>Max: {maxX.toFixed(2)}</span>
        </div>
      </div>
    </div>
  );
};
