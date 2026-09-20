import React, { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from '@/components/ui/table';
import { CsvDetector } from '@/services/csvDetector';
import { CsvUploadPreview } from '@/schemas/workspace';
import { useWorkspaceStore } from '@/store/workspaceStore';
import { FileUp, CheckCircle2, AlertCircle } from 'lucide-react';

interface CsvUploadDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

export const CsvUploadDialog: React.FC<CsvUploadDialogProps> = ({ isOpen, onClose }) => {
  const [rawText, setRawText] = useState<string | null>(null);
  const [fileName, setFileName] = useState<string | null>(null);
  const [preview, setPreview] = useState<CsvUploadPreview | null>(null);
  const [delimiter, setDelimiter] = useState<',' | ';' | '\t' | '|'>(',');
  const [decimalSeparator, setDecimalSeparator] = useState<'.' | ','>('.');
  const [error, setError] = useState<string | null>(null);

  const addEntity = useWorkspaceStore((s) => s.addEntity);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setFileName(file.name);
    setError(null);

    const reader = new FileReader();
    reader.onload = (event) => {
      const text = event.target?.result as string;
      setRawText(text);
      try {
        const parsed = CsvDetector.parse(text);
        setPreview(parsed);
        setDelimiter(parsed.delimiter);
        setDecimalSeparator(parsed.decimalSeparator);
      } catch (err: any) {
        setError(err.message || 'Failed to parse CSV file.');
      }
    };
    reader.readAsText(file);
  };

  const handleReparse = (newDelim: ',' | ';' | '\t' | '|', newDec: '.' | ',') => {
    if (!rawText) return;
    try {
      const parsed = CsvDetector.parse(rawText, {
        delimiter: newDelim,
        decimalSeparator: newDec,
      });
      setPreview(parsed);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Parsing error with selected options.');
    }
  };

  const handleConfirm = () => {
    if (!preview || !fileName) return;

    // Find date and price/return columns
    const dateCol = preview.columns.find((c) => c.inferredType === 'datetime')?.header || preview.columns[0]?.header;
    const priceCol = preview.columns.find((c) => c.inferredType === 'numeric')?.header || preview.columns[1]?.header;

    // Add PriceStream node with uploaded dataset
    addEntity(
      'PriceStream',
      { x: 150 + Math.random() * 50, y: 150 + Math.random() * 50 },
      {
        symbol: fileName.replace(/\.csv$/i, ''),
        source: 'UserUpload',
        dateColumn: dateCol,
        priceColumn: priceCol,
        totalRows: preview.totalRows,
        delimiter: preview.delimiter,
        decimalSeparator: preview.decimalSeparator,
      }
    );

    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-w-3xl max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center space-x-2">
            <FileUp className="h-5 w-5 text-primary" />
            <span>Upload Custom CSV Dataset (FR-003a)</span>
          </DialogTitle>
          <DialogDescription>
            Auto-detect delimiter, decimal format, and data types with instant tabular preview before ingestion.
          </DialogDescription>
        </DialogHeader>

        {!rawText ? (
          <div className="flex flex-col items-center justify-center border-2 border-dashed border-border rounded-xl p-8 space-y-3 bg-muted/20">
            <FileUp className="h-10 w-10 text-muted-foreground" />
            <p className="text-sm font-medium text-foreground">Select a CSV file to inspect and upload</p>
            <input
              type="file"
              accept=".csv,.txt"
              onChange={handleFileChange}
              className="text-xs file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-xs file:font-semibold file:bg-primary file:text-primary-foreground hover:file:bg-primary/90 cursor-pointer"
            />
          </div>
        ) : (
          <div className="space-y-4">
            {/* Controls */}
            <div className="grid grid-cols-2 gap-4 rounded-lg bg-muted/40 p-3 text-xs">
              <div>
                <label className="font-medium text-muted-foreground block mb-1">Delimiter</label>
                <select
                  value={delimiter}
                  onChange={(e) => {
                    const val = e.target.value as ',' | ';' | '\t' | '|';
                    setDelimiter(val);
                    handleReparse(val, decimalSeparator);
                  }}
                  className="w-full rounded border border-input bg-background px-2 py-1 text-xs"
                >
                  <option value=",">Comma (,)</option>
                  <option value=";">Semicolon (;)</option>
                  <option value="	">Tab (\t)</option>
                  <option value="|">Pipe (|)</option>
                </select>
              </div>

              <div>
                <label className="font-medium text-muted-foreground block mb-1">Decimal Separator</label>
                <select
                  value={decimalSeparator}
                  onChange={(e) => {
                    const val = e.target.value as '.' | ',';
                    setDecimalSeparator(val);
                    handleReparse(delimiter, val);
                  }}
                  className="w-full rounded border border-input bg-background px-2 py-1 text-xs"
                >
                  <option value=".">Period (.) - US / Standard</option>
                  <option value=",">Comma (,) - European</option>
                </select>
              </div>
            </div>

            {error && (
              <div className="flex items-center space-x-2 text-destructive bg-destructive/10 p-2.5 rounded-lg text-xs">
                <AlertCircle className="h-4 w-4 shrink-0" />
                <span>{error}</span>
              </div>
            )}

            {preview && (
              <div className="space-y-2">
                <div className="flex items-center justify-between text-xs">
                  <span className="font-semibold text-foreground">
                    Preview: {preview.totalRows} observations detected ({fileName})
                  </span>
                  <div className="flex items-center space-x-1.5">
                    {preview.columns.map((col) => (
                      <Badge key={col.header} variant="secondary" className="text-[10px]">
                        {col.header}: {col.inferredType}
                      </Badge>
                    ))}
                  </div>
                </div>

                <div className="rounded-md border border-border max-h-60 overflow-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        {preview.columns.map((col) => (
                          <TableHead key={col.header} className="text-xs font-semibold">
                            {col.header}
                          </TableHead>
                        ))}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {preview.previewRows.map((row, rIdx) => (
                        <TableRow key={rIdx}>
                          {preview.columns.map((col) => (
                            <TableCell key={col.header} className="text-xs font-mono">
                              {row[col.header]}
                            </TableCell>
                          ))}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            )}
          </div>
        )}

        <DialogFooter className="mt-4">
          <Button variant="outline" size="sm" onClick={onClose}>
            Cancel
          </Button>
          <Button
            size="sm"
            onClick={handleConfirm}
            disabled={!preview || Boolean(error)}
            className="flex items-center space-x-1"
          >
            <CheckCircle2 className="h-4 w-4" />
            <span>Confirm & Add to Canvas</span>
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
