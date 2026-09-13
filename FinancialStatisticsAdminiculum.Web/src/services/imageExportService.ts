import { toPng, toSvg } from 'html-to-image';

export class ImageExportService {
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

  static async exportToSvg(element: HTMLElement, filename = 'workspace-vector.svg'): Promise<void> {
    try {
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
