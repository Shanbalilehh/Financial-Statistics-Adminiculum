import React from 'react';

interface NodeSparklineProps {
  data?: number[];
  color?: string;
  height?: number;
}

export const NodeSparkline: React.FC<NodeSparklineProps> = ({
  data = [],
  color = '#0ea5e9',
  height = 36,
}) => {
  if (!data || data.length < 2) {
    return (
      <div 
        style={{ height }} 
        className="w-full bg-slate-900/50 rounded flex items-center justify-center text-[10px] text-slate-500 font-mono"
      >
        Awaiting input stream
      </div>
    );
  }

  const min = Math.min(...data);
  const max = Math.max(...data);
  const range = max - min || 1;
  const width = 180;

  const points = data
    .map((val, idx) => {
      const x = (idx / (data.length - 1)) * width;
      const y = height - ((val - min) / range) * (height - 6) - 3;
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(' ');

  return (
    <div className="w-full bg-slate-900/80 p-1 rounded border border-slate-800">
      <svg width="100%" height={height} viewBox={`0 0 ${width} ${height}`} className="overflow-visible">
        <polyline
          fill="none"
          stroke={color}
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
          points={points}
        />
      </svg>
    </div>
  );
};
