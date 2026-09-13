import React, { Component, ErrorInfo, ReactNode } from 'react';
import { AlertOctagon, RotateCcw } from 'lucide-react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Uncaught canvas error:', error, errorInfo);
  }

  private handleReset = () => {
    localStorage.removeItem('fsa-workspace-store');
    window.location.reload();
  };

  public render() {
    if (this.state.hasError) {
      return (
        <div className="flex h-screen w-screen flex-col items-center justify-center bg-slate-950 p-6 text-slate-100">
          <div className="max-w-md space-y-4 rounded-2xl border border-rose-500/30 bg-slate-900 p-6 shadow-2xl text-center">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-xl bg-rose-500/20 text-rose-400">
              <AlertOctagon className="h-6 w-6" />
            </div>

            <h2 className="text-lg font-bold text-slate-100">Workspace Recovery Mode</h2>
            <p className="text-xs text-slate-400">
              An unexpected error occurred while evaluating an atomic entity or rendering the canvas topology.
            </p>

            {this.state.error && (
              <div className="rounded bg-slate-950 p-2 font-mono text-[11px] text-rose-400 border border-slate-800 text-left overflow-x-auto">
                {this.state.error.message}
              </div>
            )}

            <button
              onClick={this.handleReset}
              className="inline-flex items-center space-x-2 rounded-lg bg-sky-500 px-4 py-2 text-xs font-semibold text-slate-950 hover:bg-sky-400 transition-all"
            >
              <RotateCcw className="h-4 w-4" />
              <span>Reset Canvas Session</span>
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
