using System;
using System.Collections.Generic;

namespace FinancialStatisticsAdminiculum.Application.DTOs
{
    public class ViewportDto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Zoom { get; set; } = 1.0;
    }

    public class AtomicEntityDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Label { get; set; } = null!;
        public ViewportPositionDto Position { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Status { get; set; } = "Ready";
        public string? ErrorMessage { get; set; }
    }

    public class ViewportPositionDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class EntityConnectionDto
    {
        public string Id { get; set; } = null!;
        public Guid SourceEntityId { get; set; }
        public string SourcePortId { get; set; } = null!;
        public Guid TargetEntityId { get; set; }
        public string TargetPortId { get; set; } = null!;
    }

    public class CreateWorkspaceDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateWorkspaceDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ViewportDto? Viewport { get; set; }
        public List<AtomicEntityDto> Entities { get; set; } = new();
        public List<EntityConnectionDto> Connections { get; set; } = new();
    }

    public class WorkspaceSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int EntityCount { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class WorkspaceDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ViewportDto Viewport { get; set; } = new();
        public List<AtomicEntityDto> Entities { get; set; } = new();
        public List<EntityConnectionDto> Connections { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
