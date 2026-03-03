using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface IDiagnosticExpert
    {
        RecoveryDecision Evaluate(Exception technicalException);
    }
}
