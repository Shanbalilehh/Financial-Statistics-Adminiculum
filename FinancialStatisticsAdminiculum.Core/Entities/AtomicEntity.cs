using System;
using System.Collections.Generic;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public enum EntityType
    {
        PriceStream,
        RollingWindow,
        MovingAverage,
        VolatilityEstimator,
        DistributionAnalyzer,
        CorrelationMatrix,
        SignalTrigger,
        CustomTransform
    }

    public enum EntityStatus
    {
        Ready,
        Computing,
        Stale,
        Error,
        Warning
    }

    public class EntityPosition
    {
        public double X { get; set; }
        public double Y { get; set; }

        public EntityPosition() { }
        public EntityPosition(double x, double y)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
                throw new ArgumentException("Position coordinates must be finite real numbers.");
            X = x;
            Y = y;
        }
    }

    public class AtomicEntity
    {
        protected AtomicEntity() { }

        public AtomicEntity(
            Guid id,
            Guid workspaceId,
            EntityType type,
            string label,
            EntityPosition position,
            Dictionary<string, object>? parameters = null)
        {
            if (string.IsNullOrWhiteSpace(label) || label.Length > 60)
                throw new ArgumentException("Entity label must be between 1 and 60 characters.", nameof(label));

            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            WorkspaceId = workspaceId;
            Type = type;
            Label = label.Trim();
            Position = position ?? new EntityPosition(0, 0);
            Parameters = parameters ?? new Dictionary<string, object>();
            Status = EntityStatus.Ready;
        }

        public Guid Id { get; private set; }
        public Guid WorkspaceId { get; private set; }
        public EntityType Type { get; private set; }
        public string Label { get; private set; } = null!;
        public EntityPosition Position { get; private set; } = new();
        public Dictionary<string, object> Parameters { get; private set; } = new();
        public EntityStatus Status { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTimeOffset? LastCalculatedAt { get; private set; }

        public void UpdatePosition(double x, double y)
        {
            Position = new EntityPosition(x, y);
        }

        public void UpdateParameters(Dictionary<string, object> newParameters)
        {
            Parameters = newParameters ?? new Dictionary<string, object>();
            LastCalculatedAt = DateTimeOffset.UtcNow;
        }

        public void SetStatus(EntityStatus status, string? errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
            if (status == EntityStatus.Ready)
                LastCalculatedAt = DateTimeOffset.UtcNow;
        }

        public void Rename(string label)
        {
            if (string.IsNullOrWhiteSpace(label) || label.Length > 60)
                throw new ArgumentException("Entity label must be between 1 and 60 characters.", nameof(label));
            Label = label.Trim();
        }
    }
}
