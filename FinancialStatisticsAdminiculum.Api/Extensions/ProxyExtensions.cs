using Castle.DynamicProxy;

namespace FinancialStatisticsAdminiculum.Api.Extensions
{
    public static class ProxyExtensions
    {
        public static void AddProxiedScoped<TInterface, TImplementation, TInterceptor>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
            where TInterceptor : class, IInterceptor 
        {
            services.AddScoped<TImplementation>();
            services.AddScoped<TInterface>(provider =>
            {
                var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                var interceptor = provider.GetRequiredService<TInterceptor>();
                var implementation = provider.GetRequiredService<TImplementation>();
                
                return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
            });
        }
    }
}
