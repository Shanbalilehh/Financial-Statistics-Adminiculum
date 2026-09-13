import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { getFormulaTrace } from '../kernel/distributionCalculations';

export class PdfExportService {
  static async exportAnalyticalDossier(
    workspaceName: string,
    nodes: any[],
    author = 'Quantitative Researcher'
  ): Promise<void> {
    const doc = new jsPDF({
      orientation: 'portrait',
      unit: 'mm',
      format: 'a4',
    });

    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();

    // Palette
    const primaryColor: [number, number, number] = [14, 165, 233]; // sky-500
    const darkBg: [number, number, number] = [15, 23, 42]; // slate-900

    // Header Banner
    doc.setFillColor(darkBg[0], darkBg[1], darkBg[2]);
    doc.rect(0, 0, pageWidth, 42, 'F');

    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(18);
    doc.text('Financial Statistics Analytical Dossier', 14, 18);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(148, 163, 184); // slate-400
    doc.text(`Workspace: ${workspaceName}`, 14, 26);
    doc.text(
      `Generated: ${new Date().toISOString().replace('T', ' ').substring(0, 19)} UTC | Author: ${author}`,
      14,
      32
    );

    let currentY = 52;

    // Section 1: Executive Topology Inventory
    doc.setTextColor(15, 23, 42);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.text('1. Active Atomic Entity Topology', 14, currentY);
    currentY += 6;

    const topologyRows = nodes.map((n) => [
      n.id,
      n.data?.type || 'Entity',
      n.data?.label || 'Node',
      JSON.stringify(n.data?.parameters || {}),
      n.data?.status || 'Ready',
    ]);

    autoTable(doc, {
      startY: currentY,
      head: [['ID', 'Type', 'Label', 'Parameters', 'Status']],
      body: topologyRows,
      theme: 'grid',
      headStyles: { fillColor: primaryColor, textColor: 255, fontStyle: 'bold' },
      styles: { fontSize: 8, font: 'helvetica' },
      columnStyles: {
        0: { cellWidth: 35 },
        1: { cellWidth: 32 },
        2: { cellWidth: 35 },
        3: { cellWidth: 60 },
        4: { cellWidth: 20 },
      },
    });

    currentY = (doc as any).lastAutoTable.finalY + 12;

    // Section 2: Statistical Moments & Distribution Summary
    if (currentY > pageHeight - 60) {
      doc.addPage();
      currentY = 20;
    }

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.text('2. Statistical Moments & Metric Inventory', 14, currentY);
    currentY += 6;

    const statsRows = nodes
      .filter((n) => n.data?.diagnostics?.moments)
      .map((n) => {
        const m = n.data.diagnostics.moments;
        return [
          n.data.label,
          m.count.toString(),
          m.mean.toFixed(4),
          m.stdDev.toFixed(4),
          m.variance.toFixed(4),
          m.skewness.toFixed(3),
          m.kurtosis.toFixed(3),
          m.quantiles.p50.toFixed(2),
        ];
      });

    if (statsRows.length > 0) {
      autoTable(doc, {
        startY: currentY,
        head: [['Entity', 'N', 'Mean (μ)', 'StdDev (σ)', 'Variance (s²)', 'Skew (g₁)', 'Kurt (g₂)', 'Median (p50)']],
        body: statsRows,
        theme: 'striped',
        headStyles: { fillColor: [71, 85, 105], textColor: 255, fontStyle: 'bold' },
        styles: { fontSize: 8, font: 'helvetica' },
      });
      currentY = (doc as any).lastAutoTable.finalY + 12;
    }

    // Section 3: Mathematical Methodology & Equation Traces
    if (currentY > pageHeight - 60) {
      doc.addPage();
      currentY = 20;
    }

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.text('3. Mathematical Specifications & Formula Traces', 14, currentY);
    currentY += 6;

    const formulaRows = nodes.map((n) => [
      n.data?.label || n.id,
      n.data?.type || 'Entity',
      getFormulaTrace(n.data?.type, n.data?.parameters || {}),
    ]);

    autoTable(doc, {
      startY: currentY,
      head: [['Entity', 'Type', 'Active Equation Trace']],
      body: formulaRows,
      theme: 'plain',
      headStyles: { fillColor: [51, 65, 85], textColor: 255 },
      styles: { fontSize: 8, font: 'courier' },
      columnStyles: {
        0: { cellWidth: 40 },
        1: { cellWidth: 35 },
        2: { cellWidth: 105 },
      },
    });

    // Save PDF
    const safeTitle = (workspaceName || 'workspace')
      .toLowerCase()
      .replace(/[^a-z0-9_-]/g, '-');
    doc.save(`${safeTitle}-dossier.pdf`);
  }
}
