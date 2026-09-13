import { StatisticalMoment } from '../types/workspace';

/**
 * High-precision mathematical and statistical calculation kernel
 * Designed for <200ms reactive evaluations on dataset sizes up to 10,000 items.
 */

export function calculateSma(data: number[], period: number): number[] {
  if (!data || data.length === 0) return [];
  const safePeriod = Math.max(1, Math.min(period, data.length));
  const result: number[] = new Array(data.length);

  let currentSum = 0;
  for (let i = 0; i < data.length; i++) {
    currentSum += data[i];
    if (i < safePeriod) {
      result[i] = currentSum / (i + 1);
    } else {
      currentSum -= data[i - safePeriod];
      result[i] = currentSum / safePeriod;
    }
    result[i] = Math.round(result[i] * 10000) / 10000;
  }
  return result;
}

export function calculateEma(data: number[], period: number): number[] {
  if (!data || data.length === 0) return [];
  const safePeriod = Math.max(1, period);
  const result: number[] = new Array(data.length);
  const alpha = 2 / (safePeriod + 1);

  result[0] = data[0];
  for (let i = 1; i < data.length; i++) {
    result[i] = data[i] * alpha + result[i - 1] * (1 - alpha);
    result[i] = Math.round(result[i] * 10000) / 10000;
  }
  return result;
}

export function calculateLogReturns(data: number[]): number[] {
  if (!data || data.length < 2) return [];
  const returns: number[] = new Array(data.length - 1);
  for (let i = 1; i < data.length; i++) {
    const prev = data[i - 1];
    const curr = data[i];
    returns[i - 1] = prev > 0 && curr > 0 ? Math.log(curr / prev) : 0;
  }
  return returns;
}

export function calculateRollingVolatility(data: number[], period: number, annualizationFactor = 252): number[] {
  if (!data || data.length === 0) return [];
  const safePeriod = Math.max(2, Math.min(period, data.length));
  const returns = calculateLogReturns(data);
  const result: number[] = new Array(data.length).fill(0);

  const factor = Math.sqrt(annualizationFactor);

  for (let i = 0; i < data.length; i++) {
    if (i < 1) {
      result[i] = 0;
      continue;
    }

    const returnIdx = i - 1;
    const windowStart = Math.max(0, returnIdx - safePeriod + 1);
    const window = returns.slice(windowStart, returnIdx + 1);

    if (window.length < 2) {
      result[i] = 0;
      continue;
    }

    const mean = window.reduce((acc, v) => acc + v, 0) / window.length;
    const variance = window.reduce((acc, v) => acc + Math.pow(v - mean, 2), 0) / (window.length - 1);
    const stdDev = Math.sqrt(Math.max(0, variance));
    result[i] = Math.round(stdDev * factor * 10000) / 10000;
  }

  return result;
}

export function calculateZScores(data: number[]): number[] {
  if (!data || data.length === 0) return [];
  const moments = calculateMoments(data);
  if (moments.stdDev === 0) {
    return new Array(data.length).fill(0);
  }
  return data.map(v => Math.round(((v - moments.mean) / moments.stdDev) * 100) / 100);
}

export function calculateMoments(data: number[]): StatisticalMoment {
  if (!data || data.length === 0) {
    return {
      count: 0,
      mean: 0,
      variance: 0,
      stdDev: 0,
      skewness: 0,
      kurtosis: 0,
      quantiles: { p01: 0, p05: 0, p25: 0, p50: 0, p75: 0, p95: 0, p99: 0 },
    };
  }

  const n = data.length;
  const mean = data.reduce((acc, v) => acc + v, 0) / n;

  let sumDiffSq = 0;
  let sumDiffCube = 0;
  let sumDiffQuad = 0;

  for (let i = 0; i < n; i++) {
    const diff = data[i] - mean;
    sumDiffSq += diff * diff;
    sumDiffCube += diff * diff * diff;
    sumDiffQuad += diff * diff * diff * diff;
  }

  const variance = n > 1 ? sumDiffSq / (n - 1) : 0;
  const stdDev = Math.sqrt(Math.max(0, variance));

  // Fisher-Pearson skewness
  let skewness = 0;
  if (stdDev > 0 && n > 2) {
    skewness = (sumDiffCube / n) / Math.pow(stdDev, 3);
  }

  // Excess kurtosis
  let kurtosis = 0;
  if (variance > 0 && n > 3) {
    kurtosis = ((sumDiffQuad / n) / Math.pow(variance, 2)) - 3;
  }

  // Empirical Quantiles via sorting
  const sorted = [...data].sort((a, b) => a - b);
  const getQuantile = (q: number) => {
    const pos = (sorted.length - 1) * q;
    const base = Math.floor(pos);
    const rest = pos - base;
    if (sorted[base + 1] !== undefined) {
      return sorted[base] + rest * (sorted[base + 1] - sorted[base]);
    }
    return sorted[base];
  };

  return {
    count: n,
    mean: Math.round(mean * 10000) / 10000,
    variance: Math.round(variance * 10000) / 10000,
    stdDev: Math.round(stdDev * 10000) / 10000,
    skewness: Math.round(skewness * 1000) / 1000,
    kurtosis: Math.round(kurtosis * 1000) / 1000,
    quantiles: {
      p01: Math.round(getQuantile(0.01) * 10000) / 10000,
      p05: Math.round(getQuantile(0.05) * 10000) / 10000,
      p25: Math.round(getQuantile(0.25) * 10000) / 10000,
      p50: Math.round(getQuantile(0.50) * 10000) / 10000,
      p75: Math.round(getQuantile(0.75) * 10000) / 10000,
      p95: Math.round(getQuantile(0.95) * 10000) / 10000,
      p99: Math.round(getQuantile(0.99) * 10000) / 10000,
    },
  };
}
