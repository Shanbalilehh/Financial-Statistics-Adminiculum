import { toPng, toSvg } from 'html-to-image';

export class ImageExportService {
  /**
   * Export DOM element or SVG element as high-resolution PNG (300+ DPI at scale >= 3)
   */
  static async exportToPng(element: HTMLElement, filename = 'workspace-capture.png', scale = 3): Promise<void> {
    try {
      const dataUrl = await toPng(element, {
        pixelRatio: scale,
        backgroundColor: '#020617', // slate-950
        cacheBust: true,
      });

      const link = document.createElement('a');
      link.download = filename;
      link.href = dataUrl;
      link.click();
    } catch (err) {
      console.error('Failed to export PNG:', err);
      throw err;
    }
  }

  /**
   * Export native vector SVG directly using XMLSerializer for pure Visx SVG elements
   */
  static exportSvgElement(svgElement: SVGSVGElement, filename = 'visx-vector.svg'): void {
    try {
      const serializer = new XMLSerializer();
      let source = serializer.serializeToString(svgElement);

      // Ensure XML namespace attributes are present
      if (!source.match(/^<svg[^>]+xmlns="http:\/\/www\.w3\.org\/2000\/svg"/)) {
        source = source.replace(/^<svg/, '<svg xmlns="http://www.w3.org/2000/svg"');
      }
      if (!source.match(/^<svg[^>]+xmlns:xlink="http:\/\/www\.w3\.org\/1999\/xlink"/)) {
        source = source.replace(/^<svg/, '<svg xmlns:xlink="http://www.w3.org/1999/xlink"');
      }

      const blob = new Blob([source], { type: 'image/svg+xml;charset=utf-8' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.download = filename;
      link.href = url;
      link.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Failed to export pure SVG element:', err);
      throw err;
    }
  }

  /**
   * Export arbitrary DOM element as SVG using html-to-image with fallback
   */
  static async exportToSvg(element: HTMLElement, filename = 'workspace-vector.svg'): Promise<void> {
    try {
      const svg = element instanceof SVGSVGElement ? element : element.querySelector<SVGSVGElement>('svg');
      if (svg) {
        this.exportSvgElement(svg, filename);
        return;
      }

      const dataUrl = await toSvg(element, {
        backgroundColor: '#020617',
      });

      const link = document.createElement('a');
      link.download = filename;
      link.href = dataUrl;
      link.click();
    } catch (err) {
      console.error('Failed to export SVG:', err);
      throw err;
    }
  }
}
