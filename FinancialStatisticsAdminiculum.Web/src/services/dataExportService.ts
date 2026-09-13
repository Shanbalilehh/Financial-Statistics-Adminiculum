export function generateCsvContent(nodes: any[]): string {
  const activeNodes = nodes.filter((n) => n.data?.calculatedValues?.series?.length > 0);
  if (activeNodes.length === 0) {
    return 'Observation\n';
  }

  const maxLength = Math.max(...activeNodes.map((n) => n.data.calculatedValues.series.length));
  const headers = ['Observation', ...activeNodes.map((n) => `"${n.data.label || n.id}"`)];

  const rows: string[] = [headers.join(',')];

  for (let i = 0; i < maxLength; i++) {
    const row = [String(i + 1)];
    for (const node of activeNodes) {
      const series = node.data.calculatedValues.series;
      row.push(series[i] !== undefined ? String(series[i]) : '');
    }
    rows.push(row.join(','));
  }

  return rows.join('\n');
}

export function generateJsonExport(nodes: any[]): string {
  const exportPayload = nodes.map((n) => ({
    id: n.id,
    type: n.data?.type,
    label: n.data?.label,
    parameters: n.data?.parameters,
    metrics: n.data?.calculatedValues?.metrics,
    moments: n.data?.diagnostics?.moments,
    series: n.data?.calculatedValues?.series || [],
  }));

  return JSON.stringify(exportPayload, null, 2);
}

export function downloadFile(content: string, filename: string, mimeType: string): void {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
