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

        public GemmaOnnxService(string modelPath)
        {
            _model = new Model(modelPath);
            _tokenizer = new Tokenizer(_model);

            var toolsJson = """
            [
              {
                "type": "function",
                "function": {
                  "name": "get_moving_average",
                  "description": "Calculates the Simple Moving Average (SMA) for a financial asset over a specific date range.",
                  "parameters": {
                    "type": "object",
                    "properties": {
                      "ticker": {
                        "type": "string",
                        "description": "The financial ticker symbol, e.g., 'XAU' for Gold."
                      },
                      "from": {
                        "type": "string",
                        "description": "The start date in YYYY-MM-DD format."
                      },
                      "to": {
                        "type": "string",
                        "description": "The end date in YYYY-MM-DD format."
                      },
                      "period": {
                        "type": "integer",
                        "description": "The lookback period."
                      }
                    },
                    "required": ["ticker", "from", "to", "period"]
                  }
                }
              }
            ]
            """;

            _systemPrompt = $"<start_of_turn>user\nYou have access to the following tools:\n{toolsJson}\n\n";
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

                // ✅ Pass sequences directly to the Generator, not via GeneratorParams
                using var generator = new Generator(_model, generatorParams);
                generator.AppendTokenSequences(sequences);  // ← correct method

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