using Microsoft.ML.OnnxRuntime;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Exceptions;
using FinancialStatisticsAdminiculum.Application.Services;
using Microsoft.SqlServer.Server;

namespace FinancialStatisticsAdminiculum.Infrastructure.ExceptionHandling
{
    public class NlpDiagnosticExpert : IDiagnosticExpert
    {
        public RecoveryDecision Evaluate(Exception technicalException)
        {
            // Translate infrastructure realities into semantic application rules
            if (technicalException is OnnxRuntimeException onnxEx)
            {
                if (onnxEx.Message.Contains("timeout") || onnxEx.Message.Contains("busy"))
                {
                    // Transient issue, safe to retry
                    return new RecoveryDecision(
                        DiagnosticAction.Retry,
                        new NlpEngineUnavailableException("The local AI model is currently busy. Please try again.")
                    );
                }

                if (onnxEx.Message.Contains("memory") || onnxEx.Message.Contains("allocation"))
                {
                    // Unmanaged memory issue. Do not retry. Kill the process to prevent corruption.
                    return new RecoveryDecision(
                        DiagnosticAction.TerminateSystem,
                        null
                    );
                }
            }

            // Default fail-safe for unpredictable AI engine errors- Future implementation
            return new RecoveryDecision(
                DiagnosticAction.FailSafe,
                new NlpProcessingException("An unexpected error occurred while analyzing the financial prompt.")
            );
        }
    }
}