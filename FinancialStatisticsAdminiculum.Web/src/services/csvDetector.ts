import { CsvUploadPreview } from '../schemas/workspace';

export interface CsvDetectionOptions {
  delimiter?: ',' | ';' | '\t' | '|';
  decimalSeparator?: '.' | ',';
  dateFormat?: string;
}

export class CsvDetector {
  static detectDelimiter(sampleLines: string[]): ',' | ';' | '\t' | '|' {
    const delimiters: Array<',' | ';' | '\t' | '|'> = [',', ';', '\t', '|'];
    let bestDelimiter: ',' | ';' | '\t' | '|' = ',';
    let maxConsistency = -1;

    for (const d of delimiters) {
      const counts = sampleLines.map((line) => line.split(d).length);
      if (counts.length > 0 && counts[0] > 1) {
        const isConsistent = counts.every((c) => c === counts[0]);
        if (isConsistent && counts[0] > maxConsistency) {
          maxConsistency = counts[0];
          bestDelimiter = d;
        }
      }
    }

    return bestDelimiter;
  }

  static detectDecimalSeparator(values: string[]): '.' | ',' {
    let periodCount = 0;
    let commaCount = 0;

    for (const val of values) {
      const trimmed = val.trim();
      if (/^-?\d+\.\d+$/.test(trimmed)) periodCount++;
      if (/^-?\d+,\d+$/.test(trimmed)) commaCount++;
    }

    return commaCount > periodCount ? ',' : '.';
  }

  static inferType(value: string, decimalSep: '.' | ','): 'datetime' | 'numeric' | 'string' | 'boolean' {
    const trimmed = value.trim().toLowerCase();
    if (trimmed === 'true' || trimmed === 'false') return 'boolean';

    // Check date
    if (
      /^\d{4}-\d{2}-\d{2}/.test(trimmed) ||
      /^\d{1,2}\/\d{1,2}\/\d{2,4}/.test(trimmed) ||
      !isNaN(Date.parse(trimmed)) && isNaN(Number(trimmed))
    ) {
      return 'datetime';
    }

    // Check numeric
    const normalizedNumber = decimalSep === ',' ? trimmed.replace(',', '.') : trimmed;
    if (!isNaN(Number(normalizedNumber)) && trimmed !== '') {
      return 'numeric';
    }

    return 'string';
  }

  static parse(rawText: string, options?: CsvDetectionOptions): CsvUploadPreview {
    const lines = rawText.split(/\r?\n/).filter((l) => l.trim().length > 0);
    if (lines.length === 0) {
      throw new Error('CSV content is empty.');
    }

    const sampleLines = lines.slice(0, 15);
    const delimiter = options?.delimiter || this.detectDelimiter(sampleLines);

    const headers = lines[0].split(delimiter).map((h) => h.trim().replace(/^["']|["']$/g, ''));
    const dataLines = lines.slice(1);

    // Sample data for inference
    const sampleValuesByCol: string[][] = headers.map(() => []);
    for (const line of dataLines.slice(0, 50)) {
      const parts = line.split(delimiter);
      parts.forEach((part, idx) => {
        if (idx < headers.length) {
          sampleValuesByCol[idx].push(part.trim().replace(/^["']|["']$/g, ''));
        }
      });
    }

    const decimalSeparator = options?.decimalSeparator || this.detectDecimalSeparator(sampleValuesByCol.flat());

    const columns = headers.map((header, idx) => {
      const samples = sampleValuesByCol[idx] || [];
      const nonEmpties = samples.filter((s) => s.length > 0);
      const inferredType = nonEmpties.length > 0
        ? this.inferType(nonEmpties[0], decimalSeparator)
        : 'string';

      return {
        index: idx,
        header,
        inferredType,
        sampleValues: samples.slice(0, 5),
      };
    });

    const previewRows = dataLines.slice(0, 10).map((line) => {
      const parts = line.split(delimiter).map((p) => p.trim().replace(/^["']|["']$/g, ''));
      const row: Record<string, string> = {};
      headers.forEach((h, idx) => {
        row[h] = parts[idx] || '';
      });
      return row;
    });

    return {
      delimiter,
      decimalSeparator,
      dateFormat: options?.dateFormat || 'YYYY-MM-DD',
      totalRows: dataLines.length,
      columns,
      previewRows,
    };
  }
}
