import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { NodeSparkline } from '../../src/components/nodes/NodeSparkline';
import { DistributionPlot } from '../../src/components/inspector/DistributionPlot';

describe('React Visx Chart Components', () => {
  describe('NodeSparkline', () => {
    it('renders empty fallback message when data has fewer than 2 points', () => {
      const { getByText } = render(<NodeSparkline data={[10]} />);
      expect(getByText('Awaiting input stream')).toBeDefined();
    });

    it('renders pure SVG DOM elements when valid time series data is provided', () => {
      const data = [100, 102, 99, 105, 108, 107, 112];
      const { container } = render(<NodeSparkline data={data} height={40} />);
      
      const svg = container.querySelector('svg');
      expect(svg).not.toBeNull();
      
      // Check for LinePath and AreaClosed path rendering
      const paths = container.querySelectorAll('path');
      expect(paths.length).toBeGreaterThanOrEqual(1);
    });
  });

  describe('DistributionPlot', () => {
    it('renders fallback when fewer than 3 observations are provided', () => {
      const { getByText } = render(<DistributionPlot series={[1.0, 2.0]} />);
      expect(getByText(/Insufficient observations/)).toBeDefined();
    });

    it('renders pure SVG KDE area, line path, and histogram bars for observation series', () => {
      const series = [10, 12, 11, 15, 14, 13, 16, 18, 17, 19, 15, 14, 12, 11, 13];
      const { container, getByText } = render(<DistributionPlot series={series} width={300} height={180} />);

      expect(getByText(/Empirical Distribution/)).toBeDefined();
      expect(getByText(/React Visx Pure SVG/)).toBeDefined();

      const svg = container.querySelector('svg');
      expect(svg).not.toBeNull();

      // Check for histogram bars
      const rects = container.querySelectorAll('rect');
      expect(rects.length).toBeGreaterThan(0);

      // Check for KDE paths
      const paths = container.querySelectorAll('path');
      expect(paths.length).toBeGreaterThanOrEqual(2);
    });
  });
});
