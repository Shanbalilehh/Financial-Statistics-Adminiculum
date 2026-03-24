using Microsoft.ML.OnnxRuntime;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Exceptions;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Infrastructure.ExceptionHandling
{
    public class NlpDiagnosticExpert : IDiagnosticExpert
    {
        private readonly ILogger<NlpDiagnosticExpert> _logger;
        public NlpDiagnosticExpert(ILogger<NlpDiagnosticExpert> logger)
        {
            _logger = logger;

        }
        public RecoveryDecision Evaluate(Exception technicalException)
        {
            if (technicalException is OnnxRuntimeException onnxEx)
            {
                _logger.LogError(onnxEx, "ONNX Runtime infrastructure failure: {ErrorMessage}", onnxEx.Message);
                if (IsTransient(onnxEx))
                {
                    _logger.LogWarning("ONNX Trasient error (Retry)");
                    return new RecoveryDecision(
                        DiagnosticAction.Retry,
                        new NlpEngineUnavailableException("The local AI model is currently busy. Please try again.")
                    );
                }

                if (IsCritical(onnxEx))
                {
                    _logger.LogCritical("ONNX Critical error (TerminateSystem)");
                    return new RecoveryDecision(
                        DiagnosticAction.TerminateSystem,
                        new NlpCriticalException("Fatal error System Terminated")
                    );
                }
            }

            _logger.LogCritical("ONNX Unexpected error (FailSafe)");
            return new RecoveryDecision(
                DiagnosticAction.FailSafe,
                new NlpUnexpectedException("An unexpected error occurred while analyzing the financial prompt.")
            );
        }

        private static bool IsTransient(OnnxRuntimeException ex)
        {
            var msg = ex.Message;
            string[] trasientStrings = ["timeout", "busy", "EP_FAIL", "ORT_EP_FAIL"];
            string pattern = string.Join("|", trasientStrings.Select(Regex.Escape));
            return Regex.IsMatch(msg, pattern, RegexOptions.Compiled);
        }

        private static bool IsCritical(OnnxRuntimeException ex)
        {
            var msg = ex.Message.AsSpan();
            string[] criticalStrings = ["memory", "allocation", "ORT_NO_MEMORY", "out of memory"];
            string pattern = string.Join("|", criticalStrings.Select(Regex.Escape));
            return Regex.IsMatch(msg, pattern, RegexOptions.Compiled);
        }
        
    }
}