using Microsoft.ML.OnnxRuntimeGenAI;

namespace FunctionGemma.Api.Services
{
    public class GemmaModelFactory : IDisposable
    {
        public Model Model { get; }
        public Tokenizer Tokenizer { get; }
        private readonly ILogger<GemmaModelFactory> _logger;
        public bool IsModelLoaded { get; private set; }

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