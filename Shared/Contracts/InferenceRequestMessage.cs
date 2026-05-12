namespace Shared.Contracts
{
    public record InferenceRequestMessage(Guid CorrelationId, string Prompt);
}