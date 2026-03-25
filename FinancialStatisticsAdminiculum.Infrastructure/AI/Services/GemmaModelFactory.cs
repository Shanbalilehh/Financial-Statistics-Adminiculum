using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI.Services
{
    public class GemmaModelFactory : IDisposable
    {
        public Model Model { get; }
        public Tokenizer Tokenizer { get; }
        private readonly ILogger<GemmaModelFactory> _logger;

        public GemmaModelFactory(string modelPath, ILogger<GemmaModelFactory> logger)
        {
            _logger = logger;
            _logger.LogInformation("Loading FunctionGemma model into memory from {ModelPath}", modelPath);
            
            Model = new Model(modelPath);
            Tokenizer = new Tokenizer(Model);
            
            _logger.LogInformation("Model and Tokenizer loaded successfully.");
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing unmanaged ONNX resources.");
            Tokenizer?.Dispose();
            Model?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}