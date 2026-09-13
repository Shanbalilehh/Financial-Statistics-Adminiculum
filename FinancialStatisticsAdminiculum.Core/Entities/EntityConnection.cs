using System;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class EntityConnection
    {
        protected EntityConnection() { }

        public EntityConnection(string id, Guid sourceEntityId, string sourcePortId, Guid targetEntityId, string targetPortId)
        {
            if (sourceEntityId == targetEntityId)
                throw new ArgumentException("A connection cannot link an entity to itself.", nameof(targetEntityId));
            if (string.IsNullOrWhiteSpace(sourcePortId))
                throw new ArgumentException("Source port ID cannot be empty.", nameof(sourcePortId));
            if (string.IsNullOrWhiteSpace(targetPortId))
                throw new ArgumentException("Target port ID cannot be empty.", nameof(targetPortId));

            Id = string.IsNullOrWhiteSpace(id) ? $"conn_{sourceEntityId}_{sourcePortId}->{targetEntityId}_{targetPortId}" : id;
            SourceEntityId = sourceEntityId;
            SourcePortId = sourcePortId.Trim();
            TargetEntityId = targetEntityId;
            TargetPortId = targetPortId.Trim();
        }

        public string Id { get; private set; } = null!;
        public Guid SourceEntityId { get; private set; }
        public string SourcePortId { get; private set; } = null!;
        public Guid TargetEntityId { get; private set; }
        public string TargetPortId { get; private set; } = null!;
    }
}
