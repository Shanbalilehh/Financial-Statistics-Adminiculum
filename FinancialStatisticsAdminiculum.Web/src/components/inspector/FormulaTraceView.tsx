import React from 'react';
import { EntityType } from '../../types/workspace';
import { getFormulaTrace } from '../../kernel/distributionCalculations';
import { BookOpen } from 'lucide-react';

interface FormulaTraceViewProps {
  type: EntityType;
  parameters: Record<string, any>;
}

export const FormulaTraceView: React.FC<FormulaTraceViewProps> = ({ type, parameters }) => {
  const formula = getFormulaTrace(type, parameters);

  return (
    <div className="space-y-2 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
      <div className="flex items-center space-x-2 text-[11px] text-slate-400">
        <BookOpen className="h-3.5 w-3.5 text-sky-400" />
        <span className="font-semibold uppercase tracking-wider">
          Mathematical Specification
        </span>
      </div>

      <div className="rounded bg-slate-900/90 p-2.5 border border-slate-800 font-mono text-xs text-sky-200 overflow-x-auto">
        <code>{formula}</code>
      </div>

      <p className="text-[10px] text-slate-500 italic">
        Parameters dynamically evaluated in-memory with high precision.
      </p>
    </div>
  );
};
