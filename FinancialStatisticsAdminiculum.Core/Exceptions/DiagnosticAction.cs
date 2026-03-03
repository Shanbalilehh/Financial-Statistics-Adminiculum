using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialStatisticsAdminiculum.Core.Exceptions
{
    public enum DiagnosticAction
    {
        Retry,              // Bounded retry via Polly or custom strategy
        FailSafe,           // Stop operation, return sanitized domain exception
        TerminateSystem     // Unrecoverable state (e.g., unmanaged memory leak), fail fast
    }
}
