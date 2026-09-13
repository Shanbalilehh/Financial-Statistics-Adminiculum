using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.DTOs;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Application.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkspaceService> _logger;

        public WorkspaceService(IUnitOfWork unitOfWork, ILogger<WorkspaceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkspaceSummaryDto>> GetAllWorkspacesAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all workspaces");
            var workspaces = await _unitOfWork.Workspaces.GetAllAsync(ct);

            return workspaces.Select(w => new WorkspaceSummaryDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                EntityCount = w.Entities.Count,
                UpdatedAt = w.UpdatedAt
            }).OrderByDescending(w => w.UpdatedAt);
        }

        public async Task<WorkspaceDetailDto?> GetWorkspaceByIdAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving workspace {WorkspaceId}", id);
            var results = await _unitOfWork.Workspaces.FindAsync(w => w.Id == id, ct);
            var workspace = results.FirstOrDefault();

            if (workspace == null)
            {
                _logger.LogWarning("Workspace {WorkspaceId} not found", id);
                return null;
            }

            return MapToDetailDto(workspace);
        }

        public async Task<WorkspaceDetailDto> CreateWorkspaceAsync(CreateWorkspaceDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Creating new workspace with name: {Name}", dto.Name);

            var workspace = new Workspace(dto.Name, dto.Description);
            await _unitOfWork.Workspaces.AddAsync(workspace, ct);
            await _unitOfWork.CompleteAsync(ct);

            return MapToDetailDto(workspace);
        }

        public async Task<WorkspaceDetailDto> UpdateWorkspaceAsync(Guid id, UpdateWorkspaceDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Updating workspace {WorkspaceId}", id);

            var results = await _unitOfWork.Workspaces.FindAsync(w => w.Id == id, ct);
            var workspace = results.FirstOrDefault() 
                ?? throw new KeyNotFoundException($"Workspace with ID {id} was not found.");

            workspace.UpdateDetails(dto.Name, dto.Description);

            if (dto.Viewport != null)
            {
                workspace.UpdateViewport(dto.Viewport.X, dto.Viewport.Y, dto.Viewport.Zoom);
            }

            workspace.Clear();

            foreach (var entityDto in dto.Entities)
            {
                if (!Enum.TryParse<EntityType>(entityDto.Type, true, out var entityType))
                {
                    entityType = EntityType.CustomTransform;
                }

                var entity = new AtomicEntity(
                    entityDto.Id,
                    id,
                    entityType,
                    entityDto.Label,
                    new EntityPosition(entityDto.Position.X, entityDto.Position.Y),
                    entityDto.Parameters
                );

                if (Enum.TryParse<EntityStatus>(entityDto.Status, true, out var status))
                {
                    entity.SetStatus(status, entityDto.ErrorMessage);
                }

                workspace.AddEntity(entity);
            }

            foreach (var connDto in dto.Connections)
            {
                var connection = new EntityConnection(
                    connDto.Id,
                    connDto.SourceEntityId,
                    connDto.SourcePortId,
                    connDto.TargetEntityId,
                    connDto.TargetPortId
                );

                workspace.AddConnection(connection);
            }

            await _unitOfWork.CompleteAsync(ct);
            return MapToDetailDto(workspace);
        }

        public async Task<bool> DeleteWorkspaceAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting workspace {WorkspaceId}", id);
            var results = await _unitOfWork.Workspaces.FindAsync(w => w.Id == id, ct);
            var workspace = results.FirstOrDefault();

            if (workspace == null)
            {
                return false;
            }

            _unitOfWork.Workspaces.Remove(workspace);
            await _unitOfWork.CompleteAsync(ct);
            return true;
        }

        private static WorkspaceDetailDto MapToDetailDto(Workspace w)
        {
            return new WorkspaceDetailDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                Viewport = new ViewportDto
                {
                    X = w.Viewport?.X ?? 0,
                    Y = w.Viewport?.Y ?? 0,
                    Zoom = w.Viewport?.Zoom ?? 1.0
                },
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt,
                Entities = w.Entities.Select(e => new AtomicEntityDto
                {
                    Id = e.Id,
                    Type = e.Type.ToString(),
                    Label = e.Label,
                    Position = new ViewportPositionDto { X = e.Position.X, Y = e.Position.Y },
                    Parameters = e.Parameters,
                    Status = e.Status.ToString(),
                    ErrorMessage = e.ErrorMessage
                }).ToList(),
                Connections = w.Connections.Select(c => new EntityConnectionDto
                {
                    Id = c.Id,
                    SourceEntityId = c.SourceEntityId,
                    SourcePortId = c.SourcePortId,
                    TargetEntityId = c.TargetEntityId,
                    TargetPortId = c.TargetPortId
                }).ToList()
            };
        }
    }
}
