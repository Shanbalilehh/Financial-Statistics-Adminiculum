//Custom Generic Repository pattern vs EFcore Repository pattern
//Custom Generic Repository pattern is more flexible and can be tailored to specific needs like mocking while EFcore Repository pattern is more standardized and easier to use with EFcore.
//Decision: We will use a custom generic repository pattern for our data access layer to allow for more flexibility and customization, while still adhering to best practices and patterns for data access in C#.
//
//Custom UnitOfWork pattern vs EFcore UnitOfWork pattern
//Custom UnitOfWork pattern allows for more control and customization, while EFcore UnitOfWork pattern is more standardized and easier to use with EFcore.
//Decision: We will use a custom UnitOfWork pattern for our data access layer to allow for more control and customization, while still adhering to best practices and patterns for data access in C#.
//
//Pricepoint atomic(timeseries = pricepoint array) vs vectorzed timeseries atomic
//Vectorized is more computational efficent but atomic is more flexible and easier to work with
//Decision: We will use a hybrid approach where we will use a vectorized timeseries for the main data structure and an atomic timeseries for the individual price points. This allows us to take advantage of the computational efficiency of the vectorized timeseries while still maintaining the flexibility of the atomic timeseries for individual price points.
//
//Vectorized timeseries with array vs Vectorized timeseries with span
//Array is better for math.NET while span is better for performance and memory management. 
//Decision: We will use a hybrid approach to accesss array-like data and span-like data, separating the responsabilities.
//
//Trading-Day vs Calendar-Day
//Trading day is better for financial data while calendar day is better for general data.
//Decision: We will use trading-day for financial data and calendar-day for general data.
//
//SoA v AoS for TimeSeries DTO
//SoA zero allocation while can get out of sync by a bug, AoS safe use while allocation overhead.
//Decision: We will use AoS with a readonly record struct to deal with the allocation overhead while still maintaining safety and ease of use.
//
//Refactor: from Span<T> as Domain Entities propierties to Memory<T>. This allows us to 
//mantain Async operations and better memory management while still providing the benefits 
//of Span<T> for performance. (ROLLED BACK) After further consideration, we have decided to stick with Span<T> for our domain entities properties.
//
//

//Code for Program.cs exception handlindg
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

//... other statements, var builder = WebApplication.CreateBuilder(args);
//Singleton ProxyGenerator
builder.Services.AddSingleton<ProxyGenerator>();
//Transient Interceptor
builder.Services.AddTrasient<Interceptor>;
//... service registration
builder.Services.AddScoped<OrchestratorService>();
//Registering the proxied service
builder.Services.AddProxiedScoped<IApplicationService, OrchestratorService, Interceptor>();

//code ExtensionMethod for adding proxied services to the DI container
public static class ProxyExtensions
{
    public static void AddProxiedScoped<TInterface, TImplementation, TInterceptor>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : IInterceptor<TInterface, TImplementation>
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

//