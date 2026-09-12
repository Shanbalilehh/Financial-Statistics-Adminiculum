using Shared.Entities;

namespace Shared.Contracts
{
    public record InferenceResponseMessage(Guid CorrelationId, string GeneratedText, bool IsSuccess, string? ErrorMessage, State State);
}