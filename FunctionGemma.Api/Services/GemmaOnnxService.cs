using FunctionGemma.Api.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Text;
using Shared.Contracts;
using Shared.Entities;


namespace FunctionGemma.Api.Services
{
    public class GemmaOnnxService : IScopedProcessingService
    {
        private readonly GemmaModelFactory _factory;
        private readonly ILogger<GemmaOnnxService> _logger;
        public GemmaOnnxService(GemmaModelFactory factory, ILogger<GemmaOnnxService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<InferenceResponseMessage> GenerateTokensAsync(InferenceRequestMessage message, CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting token generation for prompt length: {Length}", message.Prompt.Length);

            var result = await Task.Run(() =>
            {
                using var sequences = _factory.Tokenizer.Encode(message.Prompt);
                using var generatorParams = new GeneratorParams(_factory.Model);

                generatorParams.SetSearchOption("temperature", 0.0);
                generatorParams.SetSearchOption("max_length", 2048);

                using var generator = new Generator(_factory.Model, generatorParams);
                generator.AppendTokenSequences(sequences);

                using var tokenizerStream = _factory.Tokenizer.CreateStream();
                var sb = new StringBuilder();

                while (!generator.IsDone())
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    generator.GenerateNextToken();
                    var newTokenId = generator.GetSequence(0)[^1];
                    sb.Append(tokenizerStream.Decode(newTokenId));
                }

                string result = sb.ToString();
                _logger.LogDebug("Generation complete. Produced {CharCount} chars.", result.Length);
                return result;
            }, stoppingToken);
            return new InferenceResponseMessage(message.CorrelationId, result, true, null, State.textCall);
        }
    }
}