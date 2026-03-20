using Microsoft.ML.OnnxRuntime;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Exceptions;
using System;
using System.Text.RegularExpressions;

namespace FinancialStatisticsAdminiculum.Infrastructure.ExceptionHandling
{
    public class NlpDiagnosticExpert : IDiagnosticExpert
    {
        public RecoveryDecision Evaluate(Exception technicalException)
        {
            if (technicalException is OnnxRuntimeException onnxEx)
            {
                if (IsTransient(onnxEx))
                    return new RecoveryDecision(
                        DiagnosticAction.Retry,
                        new NlpEngineUnavailableException("The local AI model is currently busy. Please try again.")
                    );

                if (IsCritical(onnxEx))
                    return new RecoveryDecision(
                        DiagnosticAction.TerminateSystem,
                        new NlpCriticalException("Fatal error System Terminated")
                    );
            }

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