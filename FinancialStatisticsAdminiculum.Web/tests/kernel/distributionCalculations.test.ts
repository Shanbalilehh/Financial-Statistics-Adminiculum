import { describe, it, expect } from 'vitest';
import {
  computeKde,
  computeCdf,
  detectAnomalies,
  getFormulaTrace,
} from '../../src/kernel/distributionCalculations';

describe('distributionCalculations', () => {
  const data = [12, 14, 15, 16, 17, 18, 19, 21, 22, 100]; // 100 is an extreme outlier

  it('computes KDE points with positive densities', () => {
    const kde = computeKde(data, 30);
    expect(kde.length).toBe(30);
    expect(kde[15].density).toBeGreaterThan(0);
  });

  it('computes CDF spanning 0 to 1', () => {
    const cdf = computeCdf(data);
    expect(cdf.length).toBe(data.length);
    expect(cdf[0].cumulativeProb).toBeCloseTo(0.1, 2);
    expect(cdf[cdf.length - 1].cumulativeProb).toBe(1.0);
  });

  it('detects extreme outliers exceeding 2 standard deviations', () => {
    const anomalies = detectAnomalies(data, 2.0);
    expect(anomalies.length).toBeGreaterThan(0);
    expect(anomalies[0].value).toBe(100);
    expect(anomalies[0].zScore).toBeGreaterThan(2.0);
  });

  it('generates mathematical formula traces with substituted parameters', () => {
    const trace = getFormulaTrace('MovingAverage', { period: 20, method: 'SMA' });
    expect(trace).toContain('SMA_{t}');
    expect(trace).toContain('N = 20');

    const volTrace = getFormulaTrace('VolatilityEstimator', { period: 30, annualizationFactor: 252 });
    expect(volTrace).toContain('sqrt(252)');
    expect(volTrace).toContain('N = 30');
  });
});
