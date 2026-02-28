using Microsoft.ML.OnnxRuntimeGenAI;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using System.Text;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI
{
    public class GemmaOnnxService : INlpEngine, IDisposable
    {
        private readonly Model _model;
        private readonly Tokenizer _tokenizer;
        private readonly string _systemPrompt;

        public GemmaOnnxService(string modelPath, string dynamicToolsJson)
        {
            _model = new Model(modelPath);
            _tokenizer = new Tokenizer(_model);

            _systemPrompt = $"<start_of_turn>user\nYou have access to the following tools:\n{dynamicToolsJson}\n\n";
            
        }

        public async Task<string> ExtractToolCallAsync(string userPrompt)
        {
            return await Task.Run(() =>
            {
                string fullPrompt = $"{_systemPrompt}User: {userPrompt}\n<end_of_turn>\n<start_of_turn>model\n";

                var sequences = _tokenizer.Encode(fullPrompt);

                using var generatorParams = new GeneratorParams(_model);
                generatorParams.SetSearchOption("temperature", 0.0);
                generatorParams.SetSearchOption("max_length", 500);

                using var generator = new Generator(_model, generatorParams);
                generator.AppendTokenSequences(sequences);  

                using var tokenizerStream = _tokenizer.CreateStream();
                var result = new StringBuilder();

                while (!generator.IsDone())
                {
                    generator.GenerateNextToken();
                    var newTokenId = generator.GetSequence(0)[^1];
                    result.Append(tokenizerStream.Decode(newTokenId));
                }

                return result.ToString();
            });
        }

        public void Dispose()
        {
            _tokenizer?.Dispose();
            _model?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}