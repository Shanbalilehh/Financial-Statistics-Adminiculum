using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class ViewportState
    {
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;
        public double Zoom { get; set; } = 1.0;
    }

    public class Workspace
    {
        private readonly List<AtomicEntity> _entities = new();
        private readonly List<EntityConnection> _connections = new();

        // EF Core parameterless constructor
        protected Workspace() { }

        public Workspace(string name, string? description = null, Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                throw new ArgumentException("Workspace name must be between 1 and 100 characters.", nameof(name));
            if (description != null && description.Length > 500)
                throw new ArgumentException("Workspace description cannot exceed 500 characters.", nameof(description));

            Id = id ?? Guid.NewGuid();
            Name = name.Trim();
            Description = description?.Trim();
            Viewport = new ViewportState();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public ViewportState Viewport { get; private set; } = new();
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<AtomicEntity> Entities => _entities.AsReadOnly();
        public IReadOnlyCollection<EntityConnection> Connections => _connections.AsReadOnly();

        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                throw new ArgumentException("Workspace name must be between 1 and 100 characters.", nameof(name));
            if (description != null && description.Length > 500)
                throw new ArgumentException("Workspace description cannot exceed 500 characters.", nameof(description));

            Name = name.Trim();
            Description = description?.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateViewport(double x, double y, double zoom)
        {
            if (zoom < 0.1 || zoom > 4.0)
                throw new ArgumentOutOfRangeException(nameof(zoom), "Zoom must be between 0.1 and 4.0.");

            Viewport = new ViewportState { X = x, Y = y, Zoom = zoom };
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void AddEntity(AtomicEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (_entities.Exists(e => e.Id == entity.Id))
                throw new InvalidOperationException($"Entity with ID {entity.Id} already exists in this workspace.");

            _entities.Add(entity);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void RemoveEntity(Guid entityId)
        {
            var entity = _entities.Find(e => e.Id == entityId);
            if (entity != null)
            {
                _entities.Remove(entity);
                _connections.RemoveAll(c => c.SourceEntityId == entityId || c.TargetEntityId == entityId);
                UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        public void AddConnection(EntityConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);

            if (connection.SourceEntityId == connection.TargetEntityId)
                throw new InvalidOperationException("An entity cannot connect to itself.");

            if (!_entities.Exists(e => e.Id == connection.SourceEntityId))
                throw new InvalidOperationException($"Source entity {connection.SourceEntityId} not found in workspace.");

            if (!_entities.Exists(e => e.Id == connection.TargetEntityId))
                throw new InvalidOperationException($"Target entity {connection.TargetEntityId} not found in workspace.");

            // Check if connection already exists
            if (_connections.Exists(c => c.Id == connection.Id || 
                (c.SourceEntityId == connection.SourceEntityId && c.SourcePortId == connection.SourcePortId &&
                 c.TargetEntityId == connection.TargetEntityId && c.TargetPortId == connection.TargetPortId)))
            {
                return; // Idempotent connection add
            }

            _connections.Add(connection);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void RemoveConnection(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId)) return;
            var removed = _connections.RemoveAll(c => c.Id == connectionId);
            if (removed > 0)
                UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Clear()
        {
            _entities.Clear();
            _connections.Clear();
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
