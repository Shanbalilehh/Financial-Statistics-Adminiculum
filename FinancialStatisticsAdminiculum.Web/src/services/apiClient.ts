import { WorkspaceSummary, WorkspaceDetail, TimeSeriesData, NlpCommandResponse } from '../types/workspace';

const API_BASE = '/api';

export class ApiClient {
  static async listWorkspaces(): Promise<WorkspaceSummary[]> {
    const res = await fetch(`${API_BASE}/workspaces`);
    if (!res.ok) throw new Error(`Failed to list workspaces: ${res.statusText}`);
    return res.json();
  }

  static async getWorkspace(id: string): Promise<WorkspaceDetail> {
    const res = await fetch(`${API_BASE}/workspaces/${id}`);
    if (!res.ok) throw new Error(`Failed to get workspace: ${res.statusText}`);
    return res.json();
  }

  static async createWorkspace(name: string, description?: string): Promise<WorkspaceDetail> {
    const res = await fetch(`${API_BASE}/workspaces`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, description }),
    });
    if (!res.ok) throw new Error(`Failed to create workspace: ${res.statusText}`);
    return res.json();
  }

  static async updateWorkspace(id: string, workspace: Partial<WorkspaceDetail>): Promise<WorkspaceDetail> {
    const res = await fetch(`${API_BASE}/workspaces/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(workspace),
    });
    if (!res.ok) throw new Error(`Failed to update workspace: ${res.statusText}`);
    return res.json();
  }

  static async deleteWorkspace(id: string): Promise<void> {
    const res = await fetch(`${API_BASE}/workspaces/${id}`, {
      method: 'DELETE',
    });
    if (!res.ok) throw new Error(`Failed to delete workspace: ${res.statusText}`);
  }

  static async listAssets(): Promise<{ symbol: string; name: string; type: string }[]> {
    const res = await fetch(`${API_BASE}/marketdata/assets`);
    if (!res.ok) throw new Error(`Failed to list assets: ${res.statusText}`);
    return res.json();
  }

  static async getSeries(symbol: string, interval = '1d', lookback = 252): Promise<TimeSeriesData> {
    const res = await fetch(`${API_BASE}/marketdata/series?symbol=${encodeURIComponent(symbol)}&interval=${interval}&lookback=${lookback}`);
    if (!res.ok) throw new Error(`Failed to get series for ${symbol}: ${res.statusText}`);
    return res.json();
  }

  static async sendNlpCommand(workspaceId: string, prompt: string): Promise<NlpCommandResponse> {
    const res = await fetch(`${API_BASE}/workspaces/${workspaceId}/nlp-command`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ prompt }),
    });
    if (!res.ok) throw new Error(`Failed to process NLP command: ${res.statusText}`);
    return res.json();
  }
}
