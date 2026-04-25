using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FinancialStatisticsAdminiculum.Core.Exceptions;
using FinancialStatisticsAdminiculum.Core.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.ExceptionHandling
{
    public class SecurityExceptionInterceptor : IInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SecurityExceptionInterceptor> _logger;

        public SecurityExceptionInterceptor(IServiceProvider serviceProvider, ILogger<SecurityExceptionInterceptor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            var returnType = invocation.Method.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];
                var method = typeof(SecurityExceptionInterceptor)
                    .GetMethod(nameof(HandleAsyncWithResult), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(resultType);

                invocation.ReturnValue = method.Invoke(this, new[] { invocation.ReturnValue, invocation });
            }
            else if (returnType == typeof(Task))
            {
                var task = invocation.ReturnValue as Task ?? throw new InvalidOperationException("The return value is NULL when a Task was expected.");
                invocation.ReturnValue = HandleAsync(task, invocation);
            }
        }

        private async Task HandleAsync(Task task, IInvocation invocation)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Handling exception: {ex}", ex);
                HandleException(ex, invocation);
            }
        }

        private async Task<T> HandleAsyncWithResult<T>(Task<T> task, IInvocation invocation)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Handling exception: {ex}", ex);
                HandleException(ex, invocation);
                return default!; 
            }
        }

        private void HandleException(Exception ex, IInvocation invocation)
        {
            // 1. Identify the risk community using our new strong-typed attribute
            string communityKey = DetermineRiskCommunity(invocation);

            _logger.LogInformation("Routing exception to {CommunityKey} expert.", communityKey);

            // 2. Resolve the specific D&R expert dynamically via DI Keyed Services
            var expert = _serviceProvider.GetKeyedService<IDiagnosticExpert>(communityKey);

            if (expert == null)
            {
                _logger.LogCritical("No diagnostic expert found for community: {CommunityKey}. Initiating fail-safe.", communityKey);
                throw new Exception("Unhandled system failure. Security interceptor could not find a diagnostic expert.", ex);
            }

            // 3. Evaluate the technical exception
            var decision = expert.Evaluate(ex);

            // 4. Execute the architectural exception policy
            switch (decision.Action)
            {
                case DiagnosticAction.Retry:
                    throw decision.SemanticException ?? ex;

                case DiagnosticAction.FailSafe:
                    throw decision.SemanticException ?? new Exception("Safe termination triggered.");

                case DiagnosticAction.TerminateSystem:
                    // Catastrophic failure handling
                    Environment.FailFast($"Catastrophic failure in {communityKey} unmanaged resources.", ex);
                    break;
            }
        }

        private static string DetermineRiskCommunity(IInvocation invocation)
        {
            // Look at the interface being proxied (e.g., INlpEngine)
            Type? declaringType = invocation.Method.DeclaringType;

            if (declaringType != null)
            {
                // Read the custom attribute
                var attribute = (RiskCommunityAttribute?)Attribute.GetCustomAttribute(declaringType, typeof(RiskCommunityAttribute));

                if (attribute != null)
                {
                    return attribute.CommunityName;
                }
            }

            // Fallback default if a developer forgot to tag the interface
            return "SystemCommunity";
        }
    }
}