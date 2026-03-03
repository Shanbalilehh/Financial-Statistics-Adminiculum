using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialStatisticsAdminiculum.Core.Exceptions
{
    public record RecoveryDecision
    {
        public DiagnosticAction Action { get; init; }
        public SemanticDomainException? SemanticException { get; init; }

        //Constructor for decisions that require a semantic exception (e.g., Retry with a user-friendly message)
        public RecoveryDecision(DiagnosticAction action, SemanticDomainException semanticException)
        {
            Action = action;
            SemanticException = semanticException;
        }
    }
}
