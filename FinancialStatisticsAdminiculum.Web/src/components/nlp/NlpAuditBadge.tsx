import React, { useEffect, useState } from 'react';
import { useWorkspaceStore } from '../../store/workspaceStore';
import { Sparkles, Undo2, X } from 'lucide-react';

interface NlpAuditBadgeProps {
  message: string | null;
  onDismiss: () => void;
}

export const NlpAuditBadge: React.FC<NlpAuditBadgeProps> = ({ message, onDismiss }) => {
  const undo = useWorkspaceStore((s) => s.undo);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    if (message) {
      setVisible(true);
      const timer = setTimeout(() => {
        setVisible(false);
        onDismiss();
      }, 8000);
      return () => clearTimeout(timer);
    } else {
      setVisible(false);
    }
  }, [message]);

  if (!visible || !message) return null;

  return (
    <div className="fixed bottom-6 left-1/2 -translate-x-1/2 z-40 flex items-center space-x-3 rounded-xl border border-sky-500/40 bg-slate-900/95 px-4 py-2.5 shadow-2xl backdrop-blur ring-1 ring-sky-500/20 animate-in fade-in slide-in-from-bottom-3 duration-200">
      <div className="flex items-center space-x-2 text-sky-400">
        <Sparkles className="h-4 w-4" />
        <span className="text-xs font-semibold text-slate-200">NLP Action:</span>
      </div>

      <p className="max-w-md text-xs text-slate-300 truncate font-mono">
        {message}
      </p>

      <button
        onClick={() => {
          undo();
          setVisible(false);
          onDismiss();
        }}
        className="flex items-center space-x-1 rounded-md bg-sky-500/20 border border-sky-500/30 px-2 py-1 text-[11px] font-semibold text-sky-300 hover:bg-sky-500/30 transition-all"
      >
        <Undo2 className="h-3.5 w-3.5" />
        <span>Undo</span>
      </button>

      <button
        onClick={() => {
          setVisible(false);
          onDismiss();
        }}
        className="rounded p-1 text-slate-500 hover:text-slate-300"
      >
        <X className="h-3.5 w-3.5" />
      </button>
    </div>
  );
};
