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
//SoA vs AoS for TimeSeries DTO
//SoA zero allocation while can get out of sync by a bug, AoS safe use while allocation overhead.
//Decision: We will use AoS with a readonly record struct to deal with the allocation overhead while still maintaining safety and ease of use.
//
//Refactor: from Span<T> as Domain Entities propierties to Memory<T>. This allows us to 
//mantain Async operations and better memory management while still providing the benefits 
//of Span<T> for performance. (ROLLED BACK) After further consideration, we have decided to stick with Span<T> for our domain entities properties.
//
//Logger configuration via context (appsettings.json) vs Logger configuration statement in program.cs
//appsettings.json cleaner program.cs and centralize configurations. program.cs more direct via methods.
//Decision: We will use appsettings.json to configure the Logger in order to centralize configurations.
//
//Note: Use cancellationToken in all async operations to allow for better control and responsiveness in the application.
///
//EXAMPLE CODE FOR PROGRAM.CS
//
// Create a Logger Configuration
            /*
            Log.Logger = new LoggerConfiguration()
                // Initialize the logs Hierarchy 
                .MinimumLevel.Debug()
                // Enrich: add specifications to events
                .Enrich.FromLogContext()                         
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(new CompactJsonFormatter())     // Sink: structured JSON to stdout
                .WriteTo.File(                                   // Sink: Write structured JSON logs to File
                    new CompactJsonFormatter(),
                    path: "logs/fsa-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14
                )
                .CreateLogger();
            */

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