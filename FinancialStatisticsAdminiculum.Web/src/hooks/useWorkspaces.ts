import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ApiClient } from '../services/apiClient';
import { WorkspaceDetail } from '../types/workspace';

export function useWorkspaces() {
  return useQuery({
    queryKey: ['workspaces'],
    queryFn: () => ApiClient.listWorkspaces(),
  });
}

export function useWorkspace(id: string | null) {
  return useQuery({
    queryKey: ['workspace', id],
    queryFn: () => (id ? ApiClient.getWorkspace(id) : null),
    enabled: Boolean(id),
  });
}

export function useCreateWorkspace() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ name, description }: { name: string; description?: string }) =>
      ApiClient.createWorkspace(name, description),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
    },
  });
}

export function useUpdateWorkspace() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, workspace }: { id: string; workspace: Partial<WorkspaceDetail> }) =>
      ApiClient.updateWorkspace(id, workspace),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      queryClient.invalidateQueries({ queryKey: ['workspace', variables.id] });
    },
  });
}

export function useDeleteWorkspace() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => ApiClient.deleteWorkspace(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
    },
  });
}

export function useMarketAssets() {
  return useQuery({
    queryKey: ['marketAssets'],
    queryFn: () => ApiClient.listAssets(),
    staleTime: 1000 * 60 * 60, // 1 hour
  });
}

export function useMarketSeries(symbol: string, interval = '1d', lookback = 252) {
  return useQuery({
    queryKey: ['marketSeries', symbol, interval, lookback],
    queryFn: () => ApiClient.getSeries(symbol, interval, lookback),
    enabled: Boolean(symbol),
    staleTime: 1000 * 60 * 15, // 15 minutes
  });
}
