using FunctionGemma.Api.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Text;


namespace FunctionGemma.Api.Services
{
    public class GemmaOnnxService : IGemmaOnnxService
    {
        private readonly GemmaModelFactory _factory;
        private readonly ILogger<GemmaOnnxService> _logger;
        public GemmaOnnxService(GemmaModelFactory factory, ILogger<GemmaOnnxService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<string> GenerateTokensAsync(string fullPrompt, CancellationToken ct = default)
        {
            _logger.LogDebug("Starting token generation for prompt length: {Length}", fullPrompt.Length);

            return await Task.Run(() =>
            {
                using var sequences = _factory.Tokenizer.Encode(fullPrompt);
                using var generatorParams = new GeneratorParams(_factory.Model);

                generatorParams.SetSearchOption("temperature", 0.0);
                generatorParams.SetSearchOption("max_length", 2048);

                using var generator = new Generator(_factory.Model, generatorParams);
                generator.AppendTokenSequences(sequences);

                using var tokenizerStream = _factory.Tokenizer.CreateStream();
                var sb = new StringBuilder();

                while (!generator.IsDone())
                {
                    ct.ThrowIfCancellationRequested();
                    generator.GenerateNextToken();
                    var newTokenId = generator.GetSequence(0)[^1];
                    sb.Append(tokenizerStream.Decode(newTokenId));
                }

                string result = sb.ToString();
                _logger.LogDebug("Generation complete. Produced {CharCount} chars.", result.Length);
                return result;
            }, ct);
        }
    }
}