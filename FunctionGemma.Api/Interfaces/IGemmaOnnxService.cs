namespace FunctionGemma.Api.Interfaces
{
    public interface IGemmaOnnxService
    {
        Task<string> GenerateTokensAsync(string fullPrompt, CancellationToken ct = default);
    }
}