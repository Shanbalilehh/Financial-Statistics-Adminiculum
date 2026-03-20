using Castle.DynamicProxy;
using Castle.Core.Internal; 
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Application.Services
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
                HandleException(ex, invocation);
                _logger.LogDebug("Handling exception: {ex}", ex);
                return default!; // Compiler requires this, but execution won't reach here due to the throw.
            }
        }

        private void HandleException(Exception ex, IInvocation invocation)
        {
            // 1. Identifier la communauté de risque (utilise maintenant invocation.TargetType)
            string communityKey = DetermineRiskCommunity(invocation.TargetType!);

            // 2. Résoudre l'expert D&R spécifique via DI Keyed
            var expert = _serviceProvider.GetKeyedService<IDiagnosticExpert>(communityKey) ?? throw new InvalidOperationException("Unhandled system failure. No diagnostic expert found.");

            // 3. Evaluate the technical exception
            var decision = expert.Evaluate(ex);

            // 4. Execute the architectural exception policy
            switch (decision.Action)
            {
                case DiagnosticAction.Retry:
                    // If using Polly, you might trigger a retry state here. 
                    // For now, we bubble up the semantic exception to be caught by a higher-level retry policy.
                    throw decision.SemanticException ?? ex;

                case DiagnosticAction.FailSafe:
                    // Safely terminate and bubble up the sanitized domain exception (e.g., to the API layer)
                    throw decision.SemanticException ?? new Exception("Safe termination triggered.");

                case DiagnosticAction.TerminateSystem:
                    // Catastrophic failure (e.g., ONNX unmanaged memory corruption leaking into the heap)
                    Environment.FailFast("Catastrophic failure in unmanaged resources.", ex);
                    break;
            }
        }

        private static string DetermineRiskCommunity(Type targetType)
        {
            // Example logic: Map the service to its risk community
            if (targetType.Name.Contains("Orchestrator") || targetType.Name.Contains("TrendAnalysis"))
                return "NlpCommunity";

            return "DefaultCommunity";
        }
    }
}