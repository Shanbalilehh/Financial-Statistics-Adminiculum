import { describe, it, expect } from 'vitest';
import { generateCsvContent, generateJsonExport } from '../../src/services/dataExportService';
import { serializeManifest, validateManifest } from '../../src/services/manifestService';

describe('exportServices', () => {
  const sampleNodes = [
    {
      id: 'node-1',
      data: {
        label: 'Price Stream (AAPL)',
        calculatedValues: {
          series: [150.25, 151.0, 152.75],
        },
      },
    },
    {
      id: 'node-2',
      data: {
        label: 'SMA (20)',
        calculatedValues: {
          series: [149.0, 149.5, 150.1],
        },
      },
    },
  ];

  it('generates well-formed CSV with timestamp and series columns', () => {
    const csv = generateCsvContent(sampleNodes as any);
    expect(csv).toContain('Observation,Price Stream (AAPL),SMA (20)');
    expect(csv).toContain('1,150.25,149');
    expect(csv).toContain('3,152.75,150.1');
  });

  it('generates structured JSON export', () => {
    const jsonStr = generateJsonExport(sampleNodes as any);
    const parsed = JSON.parse(jsonStr);
    expect(parsed.length).toBe(2);
    expect(parsed[0].label).toBe('Price Stream (AAPL)');
    expect(parsed[0].series.length).toBe(3);
  });

  it('serializes and validates workspace manifest adhering to schema', () => {
    const manifest = serializeManifest(
      'ws-123',
      'My Alpha Research',
      [{ id: 'n1', type: 'PriceStream', position: { x: 0, y: 0 }, data: { parameters: { symbol: 'AAPL' } } }] as any,
      []
    );

    expect(manifest.manifestVersion).toBe('1.0.0');
    expect(manifest.workspace.name).toBe('My Alpha Research');
    expect(validateManifest(manifest)).toBe(true);
  });
});
