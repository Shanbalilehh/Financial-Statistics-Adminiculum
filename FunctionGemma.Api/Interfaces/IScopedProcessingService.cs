using System.Threading.Tasks;
using Shared.Contracts;

namespace FunctionGemma.Api.Interfaces
{
    public interface IScopedProcessingService
    {
        Task<InferenceResponseMessage> GenerateTokensAsync(InferenceRequestMessage Prompt, CancellationToken stoppingToken);
    }
}