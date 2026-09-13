using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.DTOs;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    public interface IWorkspaceService
    {
        Task<IEnumerable<WorkspaceSummaryDto>> GetAllWorkspacesAsync(CancellationToken ct = default);
        Task<WorkspaceDetailDto?> GetWorkspaceByIdAsync(Guid id, CancellationToken ct = default);
        Task<WorkspaceDetailDto> CreateWorkspaceAsync(CreateWorkspaceDto dto, CancellationToken ct = default);
        Task<WorkspaceDetailDto> UpdateWorkspaceAsync(Guid id, UpdateWorkspaceDto dto, CancellationToken ct = default);
        Task<bool> DeleteWorkspaceAsync(Guid id, CancellationToken ct = default);
    }
}
