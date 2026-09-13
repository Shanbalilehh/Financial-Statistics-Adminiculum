import React from 'react';
import { StatisticalMoment } from '../../types/workspace';

interface MomentsTableProps {
  moments?: StatisticalMoment;
}

export const MomentsTable: React.FC<MomentsTableProps> = ({ moments }) => {
  if (!moments || moments.count === 0) {
    return (
      <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3 text-xs text-slate-500 font-mono text-center">
        No moments computed yet
      </div>
    );
  }

  const { count, mean, variance, stdDev, skewness, kurtosis, quantiles } = moments;

  return (
    <div className="space-y-2 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
      <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
        Statistical Moments & Summary
      </span>

      <div className="grid grid-cols-2 gap-2 text-xs font-mono">
        <div className="flex justify-between rounded bg-slate-900/80 p-2 border border-slate-800">
          <span className="text-slate-500">Count (N):</span>
          <span className="font-bold text-slate-200">{count}</span>
        </div>
        <div className="flex justify-between rounded bg-slate-900/80 p-2 border border-slate-800">
          <span className="text-slate-500">Mean (μ):</span>
          <span className="font-bold text-sky-400">{mean.toFixed(4)}</span>
        </div>
        <div className="flex justify-between rounded bg-slate-900/80 p-2 border border-slate-800">
          <span className="text-slate-500">Std Dev (σ):</span>
          <span className="font-bold text-amber-400">{stdDev.toFixed(4)}</span>
        </div>
        <div className="flex justify-between rounded bg-slate-900/80 p-2 border border-slate-800">
          <span className="text-slate-500">Variance (s²):</span>
          <span className="font-bold text-slate-200">{variance.toFixed(4)}</span>
        </div>
        <div className="flex justify-between rounded bg-slate-900/80 p-2 border border-slate-800">
          <span className="text-slate-500">Skewness (g₁):</span>
          <span className={`font-bold ${skewness > 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
            {skewness.toFixed(3)}
          </span>
        </div>
        <div className="flex justify-between rounded bg-slate-900/80 p-2 border border-slate-800">
          <span className="text-slate-500">Kurtosis (g₂):</span>
          <span className="font-bold text-purple-400">{kurtosis.toFixed(3)}</span>
        </div>
      </div>

      {/* Quantiles Grid */}
      <div className="pt-2 border-t border-slate-800/80">
        <span className="text-[10px] font-semibold uppercase tracking-wider text-slate-500 block mb-1.5">
          Empirical Quantiles
        </span>
        <div className="grid grid-cols-4 gap-1.5 text-[11px] font-mono text-center">
          <div className="rounded bg-slate-900/60 p-1 border border-slate-800/60">
            <span className="block text-[9px] text-slate-500">p05</span>
            <span className="text-slate-200">{quantiles.p05.toFixed(2)}</span>
          </div>
          <div className="rounded bg-slate-900/60 p-1 border border-slate-800/60">
            <span className="block text-[9px] text-slate-500">p25 (Q1)</span>
            <span className="text-slate-200">{quantiles.p25.toFixed(2)}</span>
          </div>
          <div className="rounded bg-slate-900/60 p-1 border border-slate-800/60">
            <span className="block text-[9px] text-sky-400">p50 (Med)</span>
            <span className="text-sky-300 font-bold">{quantiles.p50.toFixed(2)}</span>
          </div>
          <div className="rounded bg-slate-900/60 p-1 border border-slate-800/60">
            <span className="block text-[9px] text-slate-500">p95</span>
            <span className="text-slate-200">{quantiles.p95.toFixed(2)}</span>
          </div>
        </div>
      </div>
    </div>
  );
};
