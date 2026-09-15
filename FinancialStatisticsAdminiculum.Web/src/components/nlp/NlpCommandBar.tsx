import React, { useState, useEffect, useRef } from 'react';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { ApiClient } from '../../services/apiClient';
import { globalUndoManager } from '../../store/undoManager';
import { Sparkles, Terminal, ArrowRight, Loader2, X } from 'lucide-react';

interface NlpCommandBarProps {
  onMutationApplied?: (explanation: string) => void;
}

export const NlpCommandBar: React.FC<NlpCommandBarProps> = ({ onMutationApplied }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [prompt, setPrompt] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const workspaceId = useWorkspaceStore((s) => s.workspaceId);
  const addEntity = useWorkspaceStore((s) => s.addEntity);
  const addConnection = useWorkspaceStore((s) => s.addConnection);
  const inputRef = useRef<HTMLInputElement>(null);

  // Keyboard shortcut: Cmd+K or Ctrl+K
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        setIsOpen((prev) => !prev);
      }
      if (e.key === 'Escape' && isOpen) {
        setIsOpen(false);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen]);

  useEffect(() => {
    if (isOpen) {
      setTimeout(() => inputRef.current?.focus(), 50);
    } else {
      setError(null);
    }
  }, [isOpen]);

  const handleExecute = async (commandText?: string) => {
    const textToRun = (commandText || prompt).trim();
    if (!textToRun || isLoading) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await ApiClient.sendNlpCommand(workspaceId, textToRun);

      if (response.mutations && response.mutations.length > 0) {
        // Map of created IDs
        const idMap = new Map<string, string>();

        for (const mut of response.mutations) {
          globalUndoManager.push(mut);

          if (mut.action === 'ADD_ENTITY') {
            const p = mut.payload;
            const newId = addEntity(p.type, p.position || { x: 300, y: 200 }, p.parameters);
            if (p.id) idMap.set(p.id, newId);
          } else if (mut.action === 'ADD_CONNECTION') {
            const c = mut.payload;
            const actualSource = idMap.get(c.sourceEntityId) || c.sourceEntityId;
            const actualTarget = idMap.get(c.targetEntityId) || c.targetEntityId;

            addConnection({
              source: actualSource,
              target: actualTarget,
              sourceHandle: c.sourcePortId || 'out_series',
              targetHandle: c.targetPortId || 'in_series',
            });
          }
        }

        onMutationApplied?.(response.explanation || 'Applied NLP canvas mutations.');
        setPrompt('');
        setIsOpen(false);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to execute command.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <button
        onClick={() => setIsOpen(true)}
        className="flex items-center space-x-2 rounded-lg border border-slate-700/80 bg-slate-800/80 px-3 py-1.5 text-xs text-slate-300 hover:border-sky-500 hover:text-white transition-all shadow-sm"
      >
        <Sparkles className="h-3.5 w-3.5 text-sky-400" />
        <span className="font-medium">NLP Command</span>
        <kbd className="rounded bg-slate-900 px-1.5 py-0.5 text-[10px] font-mono text-slate-400 border border-slate-800">
          ⌘K
        </kbd>
      </button>

      {isOpen && (
        <div className="fixed inset-0 z-50 flex items-start justify-center pt-24 bg-slate-950/70 backdrop-blur-sm">
          <div className="w-full max-w-2xl rounded-2xl border border-slate-800 bg-slate-900/95 p-4 shadow-2xl ring-1 ring-slate-800">
            <div className="flex items-center space-x-3 border-b border-slate-800 pb-3">
              <Terminal className="h-5 w-5 text-sky-400" />
              <input
                ref={inputRef}
                type="text"
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleExecute()}
                placeholder="Ask in natural language (e.g. 'Add a 30-day volatility estimator for AAPL and trigger alert when vol > 25%')..."
                className="flex-1 bg-transparent text-sm text-slate-100 placeholder-slate-500 focus:outline-none"
                disabled={isLoading}
              />
              {isLoading ? (
                <Loader2 className="h-4 w-4 animate-spin text-sky-400" />
              ) : (
                <button
                  onClick={() => handleExecute()}
                  disabled={!prompt.trim()}
                  className="rounded-lg bg-sky-500 px-2.5 py-1 text-xs font-semibold text-slate-950 hover:bg-sky-400 disabled:opacity-40 transition-all flex items-center space-x-1"
                >
                  <span>Run</span>
                  <ArrowRight className="h-3.5 w-3.5" />
                </button>
              )}
              <button
                onClick={() => setIsOpen(false)}
                className="rounded p-1 text-slate-500 hover:text-slate-300"
              >
                <X className="h-4 w-4" />
              </button>
            </div>

            {error && (
              <div className="mt-3 rounded-lg bg-rose-500/10 border border-rose-500/20 p-2 text-xs text-rose-400 font-mono">
                {error}
              </div>
            )}

            {/* Quick Prompts */}
            <div className="mt-3 space-y-1.5">
              <span className="text-[10px] font-semibold uppercase tracking-wider text-slate-500">
                Suggested Commands
              </span>
              <div className="flex flex-wrap gap-1.5">
                {[
                  'Add a 30-day volatility estimator for AAPL and wire trigger when vol > 25%',
                  'Compute 20-day moving average for SPY',
                  'Add 50-day EMA for MSFT',
                ].map((s) => (
                  <button
                    key={s}
                    onClick={() => {
                      setPrompt(s);
                      handleExecute(s);
                    }}
                    className="rounded-md bg-slate-800/80 border border-slate-700/60 px-2 py-1 text-[11px] text-slate-300 hover:bg-slate-700/80 hover:text-sky-300 text-left transition-all"
                  >
                    {s}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
};
