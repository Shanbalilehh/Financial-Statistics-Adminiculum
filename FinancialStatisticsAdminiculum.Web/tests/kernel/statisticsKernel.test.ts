import { describe, it, expect } from 'vitest';
import {
  calculateSma,
  calculateEma,
  calculateRollingVolatility,
  calculateZScores,
  calculateMoments,
} from '../../src/kernel/statisticsKernel';

describe('statisticsKernel', () => {
  const sampleData = [10, 11, 12, 13, 14, 15, 16, 17, 18, 19];

  it('calculates SMA correctly', () => {
    const period = 3;
    const result = calculateSma(sampleData, period);
    expect(result.length).toBe(sampleData.length);
    expect(result[0]).toBe(10); // initial padding
    expect(result[1]).toBe(10.5); // (10+11)/2
    expect(result[2]).toBe(11); // (10+11+12)/3
    expect(result[3]).toBe(12); // (11+12+13)/3
    expect(result[9]).toBe(18); // (17+18+19)/3
  });

  it('calculates EMA correctly', () => {
    const period = 3;
    const result = calculateEma(sampleData, period);
    expect(result.length).toBe(sampleData.length);
    expect(result[0]).toBe(10);
    // alpha = 2 / (3 + 1) = 0.5
    // index 1: 11 * 0.5 + 10 * 0.5 = 10.5
    expect(result[1]).toBe(10.5);
  });

  it('calculates rolling volatility with annualization', () => {
    const period = 5;
    const result = calculateRollingVolatility(sampleData, period, 252);
    expect(result.length).toBe(sampleData.length);
    expect(result[sampleData.length - 1]).toBeGreaterThan(0);
  });

  it('handles zero variance data without crashing or dividing by zero', () => {
    const flatline = [100, 100, 100, 100, 100];
    const vol = calculateRollingVolatility(flatline, 3, 252);
    expect(vol[4]).toBe(0);
    const z = calculateZScores(flatline);
    expect(z[0]).toBe(0);
  });

  it('computes statistical moments correctly', () => {
    const moments = calculateMoments(sampleData);
    expect(moments.count).toBe(10);
    expect(moments.mean).toBe(14.5);
    expect(moments.stdDev).toBeGreaterThan(0);
    expect(moments.variance).toBeCloseTo(moments.stdDev * moments.stdDev, 5);
    expect(moments.quantiles.p50).toBe(14.5);
  });
});
