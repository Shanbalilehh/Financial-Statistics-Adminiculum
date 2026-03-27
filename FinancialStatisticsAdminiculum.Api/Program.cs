using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.Repositories;
using FinancialStatisticsAdminiculum.Infrastructure.Persistence;
using FinancialStatisticsAdminiculum.Application.AI.Services;
using FinancialStatisticsAdminiculum.Infrastructure.ExceptionHandling;
using FinancialStatisticsAdminiculum.Infrastructure.AI.Services;
using FinancialStatisticsAdminiculum.Application.AI.Tools;
using Microsoft.EntityFrameworkCore;
using FinancialStatisticsAdminiculum.Application.Services;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Api.Infrastructure;
using FinancialStatisticsAdminiculum.Api.Extensions;
using Castle.DynamicProxy;
using Serilog;
using Serilog.Formatting.Compact;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;

namespace FinancialStatisticsAdminiculum.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create a Logger Configuration
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
            try
            {
                // uses ASP.NET Core WebApplication to create a services environment(IServiceCollection) with preconfigured defaults
                var builder = WebApplication.CreateBuilder(args);

                // Sets Serilog as logging provider
                builder.Host.UseSerilog();

                // Connection string appsetting.json
                var connectionString = builder.Configuration.GetConnectionString("LocalConnection");

                // Add services to the container.
                // Add Dbcontext service
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
                builder.Services.AddControllers();

                // Register the Generic Repository
                // "Scoped" is correct because DbContext is Scoped.
                builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                // Register Unit of Work
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Register FunctionGemmaParser
                builder.Services.AddSingleton<IFunctionGemmaParser, FunctionGemmaParser>();

                // Register ToolResolver
                builder.Services.AddScoped<IToolResolver, ToolResolver>();

                //AI Service Registration with Dynamic JSON Schema  
                builder.Services.AddScoped<IAiSchemaAggregator, AiSchemaAggregator>();

                // Model Singleton registration
                builder.Services.AddSingleton(sp =>
                new GemmaModelFactory(
                    @"/home/chi/models/functiongemma_oga",
                    sp.GetRequiredService<ILogger<GemmaModelFactory>>()
                ));

                // Register execution tool handlers
                builder.Services.AddKeyedScoped<IGemmaTool, SmaToolHandler>(SmaToolHandler.ToolName);

                // Register the Database Seeder
                builder.Services.AddScoped<DatabaseSeeder>();

                //Exception Handling
                // 1. Register the ASP.NET Core Global Exception Handler
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

                // 2. Register Castle DynamicProxy Mechanics
                builder.Services.AddSingleton<ProxyGenerator>();
                builder.Services.AddTransient<SecurityExceptionInterceptor>();

                // 3. Register the Diagnostic & Repair (D&R) Experts using Keyed DI
                builder.Services.AddKeyedScoped<IDiagnosticExpert, NlpDiagnosticExpert>("NlpCommunity");
                builder.Services.AddKeyedScoped<IDiagnosticExpert, PersistenceDiagnosticExpert>("PersistenceCommunity");

                // 4. Register Proxied Application Services
                // Assuming you created the ProxyExtensions class we discussed earlier.
                builder.Services.AddProxiedScoped<IOrchestratorService, OrchestratorService, SecurityExceptionInterceptor>();
                builder.Services.AddProxiedScoped<ITrendAnalysisService, TrendAnalysisService, SecurityExceptionInterceptor>();
                builder.Services.AddProxiedScoped<IGemmaOnnxService, GemmaOnnxService, SecurityExceptionInterceptor>();


                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Resolve Seeder
                using (var scope = app.Services.CreateScope())
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                    await seeder.SeedAsync();
                }

                app.UseExceptionHandler();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // Redirects Http requests to Https
                app.UseHttpsRedirection();

                // Request Serilog Logging with options
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });

                // Add Routing
                app.UseRouting();

                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();                             
            }
        }
    }
}
