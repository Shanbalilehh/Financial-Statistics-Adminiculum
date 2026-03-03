using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Api.Infrastructure
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. Log the failure. 
            // If it's a SemanticDomainException, the underlying technical details 
            // have already been handled/logged by the Interceptor/Expert, 
            // but we log the API-level failure here.
            _logger.LogError(exception, "An exception reached the API boundary: {Message}", exception.Message);

            // 2. Default API response setup
            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            // 3. Map Semantic exceptions to specific HTTP semantics
            if (exception is SemanticDomainException semanticEx)
            {
                problemDetails.Title = "Domain Rule Violation or Dependency Failure";
                problemDetails.Status = StatusCodes.Status503ServiceUnavailable; // Or 400, depending on the rule
                problemDetails.Detail = semanticEx.Message;

                // Expose the risk community safely to the client if needed for debugging
                problemDetails.Extensions["riskCommunity"] = semanticEx.RiskCommunity;
            }
            else
            {
                // 4. Ultimate Fail-Safe for unhandled/leaked technical exceptions
                problemDetails.Title = "An unexpected system error occurred.";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "The system encountered an unrecoverable fault. Please contact support.";
            }

            // 5. Write the response
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // Return true to signal the exception has been handled and shouldn't propagate further
            return true;
        }
    }
}