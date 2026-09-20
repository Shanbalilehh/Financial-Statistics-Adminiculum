import { AnomalyEvent, EntityType } from '../types/workspace';
import { calculateMoments } from './statisticsKernel';

export interface KdePoint {
  x: number;
  density: number;
}

export interface CdfPoint {
  x: number;
  cumulativeProb: number;
}

/**
 * Computes Gaussian Kernel Density Estimation (KDE)
 * Uses Silverman's rule-of-thumb for optimal bandwidth h:
 * h = 1.06 * min(stdDev, IQR/1.34) * n^(-1/5)
 */
export function computeKde(data: number[], numPoints = 40): KdePoint[] {
  if (!data || data.length < 2) return [];

  const n = data.length;
  const moments = calculateMoments(data);
  const stdDev = moments.stdDev || 1;
  const iqr = moments.quantiles.p75 - moments.quantiles.p25;
  const spread = iqr > 0 ? Math.min(stdDev, iqr / 1.34) : stdDev;
  const bandwidth = Math.max(0.001, 1.06 * spread * Math.pow(n, -0.2));

  const min = Math.min(...data) - 2 * bandwidth;
  const max = Math.max(...data) + 2 * bandwidth;
  const step = (max - min) / (numPoints - 1);

  const points: KdePoint[] = new Array(numPoints);
  const sqrt2Pi = Math.sqrt(2 * Math.PI);

  for (let i = 0; i < numPoints; i++) {
    const x = min + i * step;
    let sumKernel = 0;
    for (let j = 0; j < n; j++) {
      const u = (x - data[j]) / bandwidth;
      sumKernel += Math.exp(-0.5 * u * u) / sqrt2Pi;
    }
    const density = sumKernel / (n * bandwidth);
    const roundedDensity = Math.round(density * 10000) / 10000;
    points[i] = {
      x: Math.round(x * 1000) / 1000,
      density: roundedDensity > 0 ? roundedDensity : (density > 0 ? 0.0001 : 0),
    };
  }

  return points;
}

/**
 * Computes empirical Cumulative Distribution Function (CDF)
 */
export function computeCdf(data: number[]): CdfPoint[] {
  if (!data || data.length === 0) return [];
  const sorted = [...data].sort((a, b) => a - b);
  const n = sorted.length;

  return sorted.map((val, idx) => ({
    x: Math.round(val * 1000) / 1000,
    cumulativeProb: Math.round(((idx + 1) / n) * 10000) / 10000,
  }));
}

/**
 * Detects statistical anomalies (points with |z-score| > threshold)
 */
export function detectAnomalies(data: number[], zThreshold = 2.0): AnomalyEvent[] {
  if (!data || data.length < 3) return [];
  const moments = calculateMoments(data);
  if (moments.stdDev === 0) return [];

  const anomalies: AnomalyEvent[] = [];
  data.forEach((val, idx) => {
    const z = (val - moments.mean) / moments.stdDev;
    if (Math.abs(z) >= zThreshold) {
      anomalies.push({
        timestamp: `Point #${idx + 1}`,
        value: val,
        zScore: Math.round(z * 100) / 100,
        reason: `Exceeds ${zThreshold}σ boundary (z = ${z > 0 ? '+' : ''}${z.toFixed(2)})`,
      });
    }
  });

  return anomalies;
}

/**
 * Returns formatted mathematical formula with substituted parameters
 */
export function getFormulaTrace(type: EntityType, parameters: Record<string, any>): string {
  switch (type) {
    case 'PriceStream':
      return `P_t = \\text{Price}(\\text{Symbol} = "${parameters.symbol || 'AAPL'}", t \\in [0, ${parameters.lookback || 252}])`;
    case 'MovingAverage': {
      const p = parameters.period || 20;
      const m = parameters.method || 'SMA';
      if (m === 'EMA') {
        const alpha = (2 / (p + 1)).toFixed(4);
        return `EMA_{t} = \\alpha \\cdot P_t + (1 - \\alpha) \\cdot \\text{EMA}_{t-1} \\quad \\text{where } \\alpha = \\frac{2}{${p} + 1} = ${alpha}`;
      }
      return `SMA_{t} = \\frac{1}{N} \\sum_{i=0}^{N-1} P_{t-i} \\quad \\text{with window } N = ${p}`;
    }
    case 'VolatilityEstimator': {
      const n = parameters.period || 30;
      const factor = parameters.annualizationFactor || 252;
      return `\\sigma_{\\text{ann}} = \\sqrt{${factor}} \\times \\sqrt{\\frac{1}{N-1} \\sum_{i=1}^{N} (r_{t-i} - \\bar{r})^2} \\quad (N = ${n}, \\text{Factor} = \\text{sqrt(${factor})} = ${Math.sqrt(factor).toFixed(2)})`;
    }
    case 'SignalTrigger': {
      const thresh = parameters.threshold || 25.0;
      const cond = parameters.condition === 'GreaterThan' ? '>' : '<';
      return `\\text{Signal}_t = \\begin{cases} 1 & \\text{if } X_t ${cond} ${thresh} \\\\ 0 & \\text{otherwise} \\end{cases}`;
    }
    default:
      return `Y_t = f(X_t) \\quad \\text{with parameters } ${JSON.stringify(parameters)}`;
  }
}
