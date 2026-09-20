import React, { useState, useRef } from 'react';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { ImageExportService } from '../../services/imageExportService';
import { PdfExportService } from '../../services/pdfExportService';
import { generateCsvContent, generateJsonExport, downloadFile } from '../../services/dataExportService';
import { serializeManifest, validateManifest, downloadManifest } from '../../services/manifestService';
import { Download, FileText, Image, Code2, Table, Upload, Loader2, Check, FileUp } from 'lucide-react';
import { CsvUploadDialog } from './CsvUploadDialog';

export const ExportMenu: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [isExporting, setIsExporting] = useState(false);
  const [isCsvDialogOpen, setIsCsvDialogOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const fileInputRef = useRef<HTMLInputElement>(null);
  const workspaceId = useWorkspaceStore((s) => s.workspaceId);
  const workspaceName = useWorkspaceStore((s) => s.workspaceName);
  const nodes = useWorkspaceStore((s) => s.nodes);
  const edges = useWorkspaceStore((s) => s.edges);
  const loadWorkspace = useWorkspaceStore((s) => s.loadWorkspace);

  const notifySuccess = (msg: string) => {
    setSuccessMessage(msg);
    setTimeout(() => setSuccessMessage(null), 3000);
  };

  const handleExportPng = async () => {
    const canvasEl = document.querySelector('.react-flow') as HTMLElement;
    if (!canvasEl) return;
    setIsExporting(true);
    try {
      await ImageExportService.exportToPng(canvasEl, `${workspaceName}-canvas-300dpi.png`, 3);
      notifySuccess('PNG image exported successfully');
    } finally {
      setIsExporting(false);
      setIsOpen(false);
    }
  };

  const handleExportSvg = async () => {
    const canvasEl = document.querySelector('.react-flow') as HTMLElement;
    if (!canvasEl) return;
    setIsExporting(true);
    try {
      await ImageExportService.exportToSvg(canvasEl, `${workspaceName}-vector.svg`);
      notifySuccess('Vector SVG exported successfully');
    } finally {
      setIsExporting(false);
      setIsOpen(false);
    }
  };

  const handleExportPdf = async () => {
    setIsExporting(true);
    try {
      await PdfExportService.exportAnalyticalDossier(workspaceName, nodes);
      notifySuccess('Publication PDF generated');
    } finally {
      setIsExporting(false);
      setIsOpen(false);
    }
  };

  const handleExportCsv = () => {
    const csv = generateCsvContent(nodes);
    downloadFile(csv, `${workspaceName}-data.csv`, 'text/csv');
    notifySuccess('CSV data exported');
    setIsOpen(false);
  };

  const handleExportJson = () => {
    const jsonStr = generateJsonExport(nodes);
    downloadFile(jsonStr, `${workspaceName}-data.json`, 'application/json');
    notifySuccess('JSON data exported');
    setIsOpen(false);
  };

  const handleExportManifest = () => {
    const manifest = serializeManifest(workspaceId, workspaceName, nodes, edges);
    downloadManifest(manifest, workspaceName);
    notifySuccess('Workspace manifest exported');
    setIsOpen(false);
  };

  const handleImportManifest = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const json = JSON.parse(event.target?.result as string);
        if (!validateManifest(json)) {
          alert('Invalid workspace manifest schema.');
          return;
        }
        const ws = json.workspace;
        const loadedNodes = ws.entities.map((ent: any) => ({
          id: ent.id,
          type: ent.type,
          position: ent.position,
          data: {
            id: ent.id,
            type: ent.type,
            label: ent.label,
            parameters: ent.parameters,
            status: 'Ready',
          },
        }));
        const loadedEdges = ws.connections.map((c: any) => ({
          id: c.id,
          source: c.sourceEntityId,
          target: c.targetEntityId,
          sourceHandle: c.sourcePortId,
          targetHandle: c.targetPortId,
          animated: true,
        }));

        loadWorkspace(ws.id, ws.name, loadedNodes, loadedEdges);
        notifySuccess(`Workspace "${ws.name}" loaded successfully`);
        setIsOpen(false);
      } catch (err) {
        alert('Failed to parse manifest JSON file.');
      }
    };
    reader.readAsText(file);
  };

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        disabled={isExporting}
        className="flex items-center space-x-1.5 rounded-lg border border-slate-700/80 bg-slate-800/80 px-3 py-1.5 text-xs font-semibold text-slate-200 hover:border-emerald-500 hover:text-white transition-all shadow-sm disabled:opacity-50"
      >
        {isExporting ? (
          <Loader2 className="h-3.5 w-3.5 animate-spin text-emerald-400" />
        ) : (
          <Download className="h-3.5 w-3.5 text-emerald-400" />
        )}
        <span>Export</span>
      </button>

      {isOpen && (
        <div className="absolute right-0 top-10 z-50 w-64 rounded-xl border border-slate-800 bg-slate-900/95 p-2 shadow-2xl backdrop-blur ring-1 ring-slate-800 text-xs animate-in fade-in zoom-in-95 duration-150">
          <div className="px-2 py-1 text-[10px] font-semibold uppercase tracking-wider text-slate-500">
            Publish & Export
          </div>

          <div className="space-y-0.5">
            <button
              onClick={handleExportPng}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <Image className="h-4 w-4 text-sky-400" />
              <div className="text-left">
                <span className="font-medium block">High-Res PNG</span>
                <span className="text-[10px] text-slate-500">300 DPI raster capture</span>
              </div>
            </button>

            <button
              onClick={handleExportSvg}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <Code2 className="h-4 w-4 text-purple-400" />
              <div className="text-left">
                <span className="font-medium block">Vector SVG</span>
                <span className="text-[10px] text-slate-500">Lossless resolution diagram</span>
              </div>
            </button>

            <button
              onClick={handleExportPdf}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <FileText className="h-4 w-4 text-rose-400" />
              <div className="text-left">
                <span className="font-medium block">Analytical PDF Dossier</span>
                <span className="text-[10px] text-slate-500">Multi-page publication report</span>
              </div>
            </button>

            <button
              onClick={handleExportCsv}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <Table className="h-4 w-4 text-emerald-400" />
              <div className="text-left">
                <span className="font-medium block">Raw Series CSV</span>
                <span className="text-[10px] text-slate-500">Full decimal precision tables</span>
              </div>
            </button>

            <button
              onClick={handleExportJson}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <Code2 className="h-4 w-4 text-amber-400" />
              <div className="text-left">
                <span className="font-medium block">Structured JSON</span>
                <span className="text-[10px] text-slate-500">Calculated metrics & series</span>
              </div>
            </button>

            <div className="my-1 border-t border-slate-800" />

            <button
              onClick={handleExportManifest}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <Download className="h-4 w-4 text-blue-400" />
              <div className="text-left">
                <span className="font-medium block">Workspace Manifest</span>
                <span className="text-[10px] text-slate-500">Portable snapshot file</span>
              </div>
            </button>

            <button
              onClick={() => fileInputRef.current?.click()}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <Upload className="h-4 w-4 text-teal-400" />
              <div className="text-left">
                <span className="font-medium block">Import Manifest...</span>
                <span className="text-[10px] text-slate-500">Restore canvas from JSON</span>
              </div>
            </button>

            <button
              onClick={() => {
                setIsOpen(false);
                setIsCsvDialogOpen(true);
              }}
              className="w-full flex items-center space-x-2.5 rounded-lg px-2.5 py-2 text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
            >
              <FileUp className="h-4 w-4 text-emerald-400" />
              <div className="text-left">
                <span className="font-medium block">Upload Custom CSV...</span>
                <span className="text-[10px] text-slate-500">Auto-detect & preview data</span>
              </div>
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept=".json"
              onChange={handleImportManifest}
              className="hidden"
            />
          </div>
        </div>
      )}

      <CsvUploadDialog
        isOpen={isCsvDialogOpen}
        onClose={() => setIsCsvDialogOpen(false)}
      />

      {successMessage && (
        <div className="fixed bottom-6 right-44 z-50 flex items-center space-x-2 rounded-lg bg-emerald-500/20 border border-emerald-500/30 px-3 py-2 text-xs font-semibold text-emerald-300 shadow-xl backdrop-blur">
          <Check className="h-4 w-4" />
          <span>{successMessage}</span>
        </div>
      )}
    </div>
  );
};
